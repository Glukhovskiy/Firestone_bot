using MelonLoader;
using UnityEngine;
using System.IO;
using System.Linq;

namespace FirestoneBot
{
    public static class FirestoneResearch
    {
        private enum State { FindButton, WaitForWindow, ProcessWindow, ProcessResearch, WaitForResearchWindow, CheckStartButton, Complete }
        private static State _state = State.FindButton;
        private static float _waitTimer = 0f;
        private static GameObject[] _researchButtons = null;
        private static int _currentResearchIndex = 0;
        private static System.Collections.Generic.Dictionary<string, int> _researchPriorities = new();
        private static string _configPath = "Mods/FirestoneResearch.conf";
        
        private static readonly System.Collections.Generic.Dictionary<string, string> _researchNames = new()
        {
            { "1/0", "Attribute damage" },
            { "1/1", "Attribute health" },
            { "1/2", "Attribute armor" },
            { "1/3", "Fist fight" },
            { "1/4", "Guardian power" },
            { "1/5", "Projectiles" },
            { "1/6", "Raining gold" },
            { "1/7", "Critical loot Bonus" },
            { "1/8", "Critical loot Chance" },
            { "1/9", "Weaklings" },
            { "1/10", "Expose Weakness" },
            { "1/11", "Medal of honor" },
            { "1/12", "Prestigious" },
            { "1/13", "Trainer Skills" },
            { "1/14", "Skip wave" },
            { "1/15", "Expeditioner" },
            { "2/0", "Skip stage" },
            { "2/1", "all main attributes" },
            { "2/2", "Prestigious" },
            { "2/3", "Powerless enemy" },
            { "2/4", "Powerless boss" },
            { "2/5", "Raining gold" },
            { "2/6", "Meteorite Hunter" },
            { "2/7", "Attribute damage" },
            { "2/8", "Weaklings" },
            { "2/9", "Guardian power" },
            { "2/10", "Expose Weakness" },
            { "2/11", "Attribute health" },
            { "2/12", "Attribute armor" },
            { "2/13", "Rage heroes" },
            { "2/14", "Mana heroes" },
            { "2/15", "Energy heroes" },
            { "3/0", "Guardian power" },
            { "3/1", "Firestone effect" },
            { "3/2", "Rage heroes" },
            { "3/3", "Mana heroes" },
            { "3/4", "Energy heroes" },
            { "3/5", "Fist fight" },
            { "3/6", "Precision" },
            { "3/7", "Magic spells" },
            { "3/8", "Tank specialization" },
            { "3/9", "Healer specialization" },
            { "3/10", "Damage specialization" },
            { "3/11", "Attribute damage" },
            { "3/12", "Attribute health" },
            { "3/13", "Attribute armor" },
            { "3/14", "Raining gols" },
            { "3/15", "All main attributes" }
        };
        
        public static void ProcessFirestoneResearch()
        {
            // Принудительная перезагрузка конфигурации
            _researchPriorities.Clear();
            LoadConfig();
            
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
                        string key = ExtractResearchKey(researchBtn.name, GetPath(researchBtn.transform));
                        int priority = GetResearchPriority(researchBtn.name, GetPath(researchBtn.transform));
                        
                        if (GameUtils.ClickButton(researchBtn))
                        {
                            string displayName = GetResearchDisplayName(researchBtn.name, GetPath(researchBtn.transform));
                            DebugManager.DebugLog($"Открыто исследование: {key} - {displayName} (приоритет: {priority})");
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
                        string displayName = GetResearchDisplayName(_researchButtons[_currentResearchIndex].name, GetPath(_researchButtons[_currentResearchIndex].transform));
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
                        int priority = GetResearchPriority(button.name, path);
                        string key = ExtractResearchKey(button.name, path);
                        if (priority < 999)
                        {
                            buttons.Add((button.gameObject, priority, key));
                            string displayName = GetResearchDisplayName(button.name, path);
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
        
        private static void LoadConfig()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    MelonLogger.Warning($"Файл конфигурации {_configPath} не найден, используются значения по умолчанию");
                    SetDefaultPriorities();
                    return;
                }
                
                var lines = File.ReadAllLines(_configPath);
                
                foreach (var line in lines)
                {
                    if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue;
                    
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var valueStr = parts[1].Split('#')[0].Trim();
                        
                        if (int.TryParse(valueStr, out int priority))
                        {
                            _researchPriorities[key] = priority;
                        }
                    }
                }
                
                DebugManager.DebugLog($"Загружено {_researchPriorities.Count} приоритетов исследований");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка загрузки конфигурации: {ex.Message}");
                SetDefaultPriorities();
            }
        }
        
        private static void SetDefaultPriorities()
        {
            _researchPriorities = new System.Collections.Generic.Dictionary<string, int>
            {
                { "1/0", 1 }, { "3/0", 2 }, { "2/0", 3 }, { "4/0", 4 }, { "1/1", 5 }
            };
        }
        
        private static int GetResearchPriority(string buttonName, string path)
        {
            var key = ExtractResearchKey(buttonName, path);
            return _researchPriorities.TryGetValue(key, out int priority) ? priority : 998;
        }
        
        private static string GetResearchDisplayName(string buttonName, string path)
        {
            var key = ExtractResearchKey(buttonName, path);
            if (_researchNames.TryGetValue(key, out string displayName))
                return $"tree{key.Split('/')[0]}/firestoneResearch{key.Split('/')[1]} - {displayName}";
            return buttonName;
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