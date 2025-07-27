using MelonLoader;
using UnityEngine;
using System.Linq;

namespace Firestone_bot
{
    public static class FirestoneResearch
    {
        private enum State { FindButton, WaitForWindow, ProcessWindow, ProcessResearch, WaitForResearchWindow, CheckStartButton, Complete }
        private static State _state = State.FindButton;
        private static float _waitTimer = 0f;
        private static GameObject[] _researchButtons = null;
        private static int _currentResearchIndex = 0;

        private static string ConvertSpriteNameToDisplayName(string spriteName)
        {
            // Убираем x128 в конце
            if (spriteName.EndsWith("x128"))
                spriteName = spriteName.Substring(0, spriteName.Length - 4);
            
            // Разделяем camelCase на слова с заглавными буквами
            var result = System.Text.RegularExpressions.Regex.Replace(spriteName, "([a-z])([A-Z])", "$1 $2");
            
            // Делаем первую букву заглавной
            if (result.Length > 0)
                result = char.ToUpper(result[0]) + result.Substring(1);
            
            return result;
        }

        

        
        public static void ProcessFirestoneResearch()
        {
            
            switch (_state)
            {
                case State.FindButton:
                    var btn = GameUtils.FindButton("FirestoneResearch");
                    DebugManager.DebugLog($"FirestoneResearch button found: {btn != null}");
                    if (GameUtils.ClickButton(btn))
                    {
                        _waitTimer = Time.time + 1f;
                        _state = State.WaitForWindow;
                        DebugManager.DebugLog("FirestoneResearch button clicked, waiting for window");
                    }
                    else
                    {
                        DebugManager.DebugLog("FirestoneResearch button not found, moving to next function");
                        BotMain.NextFunction();
                        _state = State.FindButton;
                    }
                    break;
                    
                case State.WaitForWindow:
                    if (Time.time >= _waitTimer)
                        _state = State.ProcessWindow;
                    break;
                    
                case State.ProcessWindow:
                    ProcessWindow();
                    break;
                    
                case State.ProcessResearch:
                    if (_researchButtons != null && _currentResearchIndex < _researchButtons.Length)
                    {
                        var researchBtn = _researchButtons[_currentResearchIndex];
                        string displayName = GetResearchDisplayNameFromUI(researchBtn);
                        string key = ExtractResearchKey(researchBtn.name, GetPath(researchBtn.transform));
                        
                        if (GameUtils.ClickButton(researchBtn))
                        {
                            DebugManager.DebugLog($"Открыто исследование: {key} - {displayName}");
                            _waitTimer = Time.time + 0.1f;
                            _state = State.WaitForResearchWindow;
                        }
                        else
                        {
                            _currentResearchIndex++;
                        }
                    }
                    else
                    {
                        DebugManager.DebugLog("All research processed");
                        _state = State.Complete;
                    }
                    break;
                    
                case State.WaitForResearchWindow:
                    if (Time.time >= _waitTimer)
                        _state = State.CheckStartButton;
                    break;
                    
                case State.CheckStartButton:
                    var startBtn = GameUtils.FindButton("researchActivateButton");
                    if (startBtn != null)
                    {
                        string key = ExtractResearchKey(_researchButtons[_currentResearchIndex].name, GetPath(_researchButtons[_currentResearchIndex].transform));
                        string displayName = GetResearchDisplayNameFromUI(_researchButtons[_currentResearchIndex]);
                        MelonLogger.Msg($"Запущено исследование: {key} - {displayName}");
                        GameUtils.ClickButton(startBtn);
                        _state = State.Complete;
                    }
                    else
                    {
                        DebugManager.DebugLog("Research activate button not found for this research");
                        
                        // Закрываем окно исследования и переходим к следующему
                        var researchCloseBtn = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/popups/FirestoneResearchPreview/closeButton");
                        if (GameUtils.ClickButton(researchCloseBtn))
                            DebugManager.DebugLog("Research preview window closed");
                        
                        _currentResearchIndex++;
                        _state = State.ProcessResearch;
                    }
                    break;
                    
                case State.Complete:
                    DebugManager.DebugLog("Attempting to close FirestoneResearch window");
                    var closeBtn = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/Library/closeButton");
                    if (GameUtils.ClickButton(closeBtn))
                        DebugManager.DebugLog("FirestoneResearch window closed");
                    else
                        DebugManager.DebugLog("Failed to close FirestoneResearch window");
                    
                    // Сброс состояния
                    _researchButtons = null;
                    _currentResearchIndex = 0;
                    
                    BotMain.NextFunction();
                    _state = State.FindButton;
                    break;
            }
        }
        
        private static void ProcessWindow()
        {
            DebugManager.DebugLog("Processing FirestoneResearch window");
            
            var claimButtons = FindButtonsByPath("claimButton", "firestoneResearch/");
            DebugManager.DebugLog($"Found {claimButtons.Length} claim buttons");
            
            foreach (var btn in claimButtons)
            {
                if (GameUtils.ClickButton(btn))
                    DebugManager.DebugLog($"Clicked claim button: {btn.name}");
            }
                
            if (claimButtons.Length == 0)
            {
                _researchButtons = FindAndSortResearchButtons();
                DebugManager.DebugLog($"Found {_researchButtons.Length} research buttons");
                _currentResearchIndex = 0;
                _state = State.ProcessResearch;
            }
            else
            {
                _state = State.Complete;
            }
        }
        
        private static GameObject[] FindAndSortResearchButtons()
        {
            var buttons = new System.Collections.Generic.List<(GameObject obj, int priority, string key)>();
            var allButtons = UnityEngine.Object.FindObjectsOfType<UnityEngine.UI.Button>();
            
            foreach (var button in allButtons)
            {
                if (button.gameObject.activeInHierarchy && button.interactable)
                {
                    var path = GetPath(button.transform);
                    var nameMatch = button.name.StartsWith("firestoneResearch");
                    var pathMatch = path.Contains("tree");
                    
                    if (nameMatch && pathMatch)
                    {
                        string displayName = GetResearchDisplayNameFromUI(button.gameObject);
                        int priority = GetResearchPriorityByName(displayName);
                        string key = ExtractResearchKey(button.name, path);
                        if (priority < 999)
                        {
                            buttons.Add((button.gameObject, priority, key));
                            DebugManager.DebugLog($"Найдено исследование: {key} - {displayName} (приоритет: {priority})");
                        }
                    }
                }
            }
            
            var sortedButtons = buttons.OrderBy(x => x.priority).Select(x => x.obj).ToArray();
            DebugManager.DebugLog($"Порядок выполнения исследований:");
            foreach (var btn in buttons.OrderBy(x => x.priority))
            {
                DebugManager.DebugLog($"  {btn.key} (приоритет: {btn.priority})");
            }
            return sortedButtons;
        }
        
        private static GameObject[] FindButtonsByPath(string name, string pathContains)
        {
            var buttons = new System.Collections.Generic.List<GameObject>();
            var allButtons = UnityEngine.Object.FindObjectsOfType<UnityEngine.UI.Button>();
            
            foreach (var button in allButtons)
            {
                if (button.gameObject.activeInHierarchy && button.interactable)
                {
                    var path = GetPath(button.transform);
                    var nameMatch = button.name == name || button.name.StartsWith(name);
                    var pathMatch = path.Contains(pathContains);
                    
                    if (nameMatch && pathMatch)
                    {
                        buttons.Add(button.gameObject);
                    }
                }
            }
            
            return buttons.ToArray();
        }
        

        
        private static int GetResearchPriorityByName(string researchName)
        {
            return ConfigManager.Config.FirestoneResearch.TryGetValue(researchName, out int priority) ? priority : 998;
        }
        
        private static string GetResearchDisplayNameFromUI(GameObject researchButton)
        {
            try
            {
                // Ищем среди всех дочерних объектов
                var children = researchButton.GetComponentsInChildren<Transform>();
                
                // Ищем researchName
                foreach (var child in children)
                {
                    if (child.name == "researchName")
                    {
                        var textComponent = child.GetComponent<UnityEngine.UI.Text>();
                        if (textComponent != null && !string.IsNullOrEmpty(textComponent.text))
                        {
                            return textComponent.text.Trim();
                        }
                    }
                }
                
                // Ищем researchIcon
                foreach (var child in children)
                {
                    if (child.name == "researchIcon")
                    {
                        var imageComponent = child.GetComponent<UnityEngine.UI.Image>();
                        if (imageComponent?.sprite != null)
                        {
                            return ConvertSpriteNameToDisplayName(imageComponent.sprite.name);
                        }
                    }
                }
                
                return researchButton.name;
            }
            catch (System.Exception ex)
            {
                DebugManager.DebugLog($"Ошибка получения названия исследования: {ex.Message}");
                return researchButton.name;
            }
        }
        
        private static string ExtractResearchKey(string buttonName, string path)
        {
            // Извлекаем номер дерева из пути (tree1, tree2, etc.)
            var treeMatch = System.Text.RegularExpressions.Regex.Match(path, @"tree(\d+)");
            if (!treeMatch.Success) return "0/0";
            
            var treeIndex = treeMatch.Groups[1].Value;
            
            // Извлекаем номер исследования из имени кнопки (firestoneResearch0, firestoneResearch1, etc.)
            var researchMatch = System.Text.RegularExpressions.Regex.Match(buttonName, @"firestoneResearch(\d+)");
            if (!researchMatch.Success) return $"{treeIndex}/0";
            
            var researchIndex = researchMatch.Groups[1].Value;
            return $"{treeIndex}/{researchIndex}";
        }
        
        private static string GetPath(Transform transform)
        {
            var path = "";
            var current = transform;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }
    }
}