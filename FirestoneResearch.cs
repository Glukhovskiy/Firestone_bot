using MelonLoader;
using UnityEngine;

namespace FirestoneBot
{
    public static class FirestoneResearch
    {
        private enum State { FindButton, WaitForWindow, ProcessWindow, ProcessResearch, WaitForResearchWindow, CheckStartButton, Complete }
        private static State _state = State.FindButton;
        private static float _waitTimer = 0f;
        private static GameObject[] _researchButtons = null;
        private static int _currentResearchIndex = 0;
        
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
                        DebugManager.DebugLog($"Processing research {_currentResearchIndex + 1}/{_researchButtons.Length}: {researchBtn.name}");
                        
                        if (GameUtils.ClickButton(researchBtn))
                        {
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
                        DebugManager.DebugLog("Found research activate button, clicking and exiting");
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
                _researchButtons = FindButtonsByPath("firestoneResearch", "tree");
                DebugManager.DebugLog($"Found {_researchButtons.Length} research buttons");
                _currentResearchIndex = 0;
                _state = State.ProcessResearch;
            }
            else
            {
                _state = State.Complete;
            }
        }
        
        private static GameObject[] FindButtonsByPath(string name, string pathContains)
        {
            var buttons = new System.Collections.Generic.List<GameObject>();
            var allButtons = UnityEngine.Object.FindObjectsOfType<UnityEngine.UI.Button>();
            
            DebugManager.DebugLog($"Searching for buttons with name '{name}' containing path '{pathContains}'");
            
            foreach (var button in allButtons)
            {
                if (button.gameObject.activeInHierarchy && button.interactable)
                {
                    var path = GetPath(button.transform);
                    var nameMatch = button.name == name || button.name.StartsWith(name);
                    var pathMatch = path.Contains(pathContains);
                    
                    if (nameMatch && pathMatch)
                    {
                        DebugManager.DebugLog($"Found matching button: {button.name} at {path}");
                        buttons.Add(button.gameObject);
                    }
                }
            }
            
            return buttons.ToArray();
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