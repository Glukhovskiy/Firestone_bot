using MelonLoader;
using UnityEngine;

namespace FirestoneBot
{
    public static class MeteoriteResearch
    {
        private static bool _meteoriteClicked = false;
        private static float _meteoriteClickTime = 0f;
        private static GameObject[] _researchList = null;
        private static int _currentResearchIndex = 0;
        private static bool _researchWindowOpen = false;
        private static float _lastResearchClickTime = 0f;
        
        public static void ProcessMeteoriteResearch()
        {
            DebugManager.DebugLog("ProcessMeteoriteResearch вызвана");
            
            if (!_meteoriteClicked)
            {
                GameObject meteoriteButton = FindMeteoriteResearchButton();
                if (meteoriteButton != null)
                {
                    DebugManager.DebugLog("Кнопка MeteoriteResearch найдена");
                    var button = meteoriteButton.GetComponent<UnityEngine.UI.Button>();
                    if (button != null && button.interactable)
                    {
                        button.onClick.Invoke();
                        _meteoriteClicked = true;
                        _meteoriteClickTime = Time.time;
                        MelonLogger.Msg("Нажата кнопка MeteoriteResearch");
                    }
                }
                else
                {
                    DebugManager.DebugLog("Кнопка MeteoriteResearch не найдена");
                    BotMain.NextFunction();
                    ResetState();
                }
            }
            else if (Time.time - _meteoriteClickTime >= 1f)
            {
                ProcessResearchWindow();
            }
        }
        
        private static void ProcessResearchWindow()
        {
            try
            {
                if (_researchList == null)
                {
                    DebugManager.DebugLog("Поиск списка исследований");
                    _researchList = FindResearchButtons();
                    _currentResearchIndex = 0;
                }
                
                if (!_researchWindowOpen)
                {
                    if (_currentResearchIndex >= _researchList.Length)
                    {
                        DebugManager.DebugLog("Все исследования обработаны");
                        CloseResearchWindow();
                        BotMain.NextFunction();
                        ResetState();
                        return;
                    }
                    
                    GameObject currentResearch = _researchList[_currentResearchIndex];
                    if (currentResearch != null)
                    {
                        var button = currentResearch.GetComponent<UnityEngine.UI.Button>();
                        if (button != null && button.interactable)
                        {
                            button.onClick.Invoke();
                            _researchWindowOpen = true;
                            MelonLogger.Msg($"Открыто исследование {_currentResearchIndex + 1}");
                        }
                        else
                        {
                            _currentResearchIndex++;
                        }
                    }
                    else
                    {
                        _currentResearchIndex++;
                    }
                }
                else
                {
                    GameObject researchBtn = FindResearchButton();
                    if (researchBtn != null)
                    {
                        var resBtn = researchBtn.GetComponent<UnityEngine.UI.Button>();
                        if (resBtn != null && resBtn.interactable)
                        {
                            if (Time.time - _lastResearchClickTime >= 1f)
                            {
                                resBtn.onClick.Invoke();
                                _lastResearchClickTime = Time.time;
                                MelonLogger.Msg("Нажата кнопка исследования");
                            }
                        }
                        else
                        {
                            DebugManager.DebugLog("Кнопка стала неактивной, закрываем окно");
                            CloseCurrentResearchWindow();
                        }
                    }
                    else
                    {
                        DebugManager.DebugLog("Кнопка исчезла, закрываем окно");
                        CloseCurrentResearchWindow();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка обработки исследований: {ex.Message}");
            }
        }
        
        private static void CloseCurrentResearchWindow()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "closeButton")
                    {
                        var button = obj.GetComponent<UnityEngine.UI.Button>();
                        if (button != null && button.interactable)
                        {
                            button.onClick.Invoke();
                            _researchWindowOpen = false;
                            _currentResearchIndex++;
                            DebugManager.DebugLog("Закрыто окно текущего исследования");
                            break;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка закрытия окна: {ex.Message}");
            }
        }
        
        private static GameObject FindResearchButton()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "researchButton")
                    {
                        return obj;
                    }
                }
                
                return null;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка поиска researchButton: {ex.Message}");
                return null;
            }
        }
        
        private static GameObject[] FindResearchButtons()
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            System.Collections.Generic.List<GameObject> researchButtons = new System.Collections.Generic.List<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.activeInHierarchy && obj.name.StartsWith("research"))
                {
                    Transform current = obj.transform;
                    string path = "";
                    while (current != null)
                    {
                        path = current.name + "/" + path;
                        current = current.parent;
                    }
                    
                    if (path.Contains("meteoriteResearch/") && path.Contains("tree"))
                    {
                        var button = obj.GetComponent<UnityEngine.UI.Button>();
                        if (button != null && button.interactable)
                        {
                            researchButtons.Add(obj);
                        }
                    }
                }
            }
            
            return researchButtons.ToArray();
        }
        
        private static void CloseResearchWindow()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "closeButton")
                    {
                        Transform current = obj.transform;
                        string path = "";
                        while (current != null)
                        {
                            path = current.name + "/" + path;
                            current = current.parent;
                        }
                        
                        if (path.Contains("Library/"))
                        {
                            var button = obj.GetComponent<UnityEngine.UI.Button>();
                            if (button != null && button.interactable)
                            {
                                button.onClick.Invoke();
                                MelonLogger.Msg("Закрыто окно исследований");
                                break;
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка закрытия окна: {ex.Message}");
            }
        }
        
        private static GameObject FindMeteoriteResearchButton()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "MeteoriteResearch")
                    {
                        Transform current = obj.transform;
                        string path = "";
                        while (current != null)
                        {
                            path = current.name + "/" + path;
                            current = current.parent;
                        }
                        
                        if (path.Contains("battleRoot/battleMain/battleCanvas/SafeArea/leftSideUI/notifications/Viewport/grid/"))
                        {
                            var button = obj.GetComponent<UnityEngine.UI.Button>();
                            if (button != null && button.interactable)
                            {
                                return obj;
                            }
                        }
                    }
                }
                
                return null;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка поиска кнопки MeteoriteResearch: {ex.Message}");
                return null;
            }
        }
        
        private static void ResetState()
        {
            _meteoriteClicked = false;
            _meteoriteClickTime = 0f;
            _researchList = null;
            _currentResearchIndex = 0;
            _researchWindowOpen = false;
            _lastResearchClickTime = 0f;
        }
    }
}