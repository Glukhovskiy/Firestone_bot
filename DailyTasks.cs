using MelonLoader;
using UnityEngine;

namespace FirestoneBot
{
    public static class DailyTasks
    {
        private static bool _questsClicked = false;
        private static float _questsClickTime = 0f;
        private static bool _dailyProcessed = false;
        private static bool _weeklyProcessed = false;
        
        public static void ProcessDailyTasks()
        {
            DebugManager.DebugLog("ProcessDailyTasks вызвана");
            
            if (!_questsClicked)
            {
                GameObject questsButton = FindQuestsButton();
                if (questsButton != null)
                {
                    DebugManager.DebugLog("Кнопка Quests найдена");
                    var button = questsButton.GetComponent<UnityEngine.UI.Button>();
                    if (button != null && button.interactable)
                    {
                        button.onClick.Invoke();
                        _questsClicked = true;
                        _questsClickTime = Time.time;
                        MelonLogger.Msg("Нажата кнопка Quests");
                    }
                }
                else
                {
                    DebugManager.DebugLog("Кнопка Quests не найдена");
                    BotMain.NextFunction();
                    ResetState();
                }
            }
            else if (Time.time - _questsClickTime >= 1f)
            {
                ProcessQuestsWindow();
            }
        }
        
        private static void ProcessQuestsWindow()
        {
            try
            {
                if (!_dailyProcessed)
                {
                    DebugManager.DebugLog("Обработка ежедневных квестов");
                    
                    // Сначала нажимаем dailyButton
                    GameObject dailyButton = FindDailyButton();
                    if (dailyButton != null)
                    {
                        var button = dailyButton.GetComponent<UnityEngine.UI.Button>();
                        if (button != null && button.interactable)
                        {
                            button.onClick.Invoke();
                            MelonLogger.Msg("Нажата кнопка ежедневных квестов");
                        }
                    }
                    
                    GameObject[] claimButtons = FindClaimButtons();
                    if (claimButtons.Length > 0)
                    {
                        foreach (GameObject claimButton in claimButtons)
                        {
                            var button = claimButton.GetComponent<UnityEngine.UI.Button>();
                            if (button != null && button.interactable)
                            {
                                button.onClick.Invoke();
                                MelonLogger.Msg("Получена награда за ежедневный квест");
                            }
                        }
                    }
                    else
                    {
                        DebugManager.DebugLog("Награды за ежедневные квесты не найдены");
                    }
                    
                    _dailyProcessed = true;
                    
                    // Переходим к еженедельным квестам
                    GameObject weeklyButton = FindWeeklyButton();
                    if (weeklyButton != null)
                    {
                        var button = weeklyButton.GetComponent<UnityEngine.UI.Button>();
                        if (button != null && button.interactable)
                        {
                            button.onClick.Invoke();
                            MelonLogger.Msg("Нажата кнопка еженедельных квестов");
                        }
                    }
                    else
                    {
                        DebugManager.DebugLog("Кнопка еженедельных квестов не найдена");
                        _weeklyProcessed = true;
                    }
                }
                else if (!_weeklyProcessed)
                {
                    DebugManager.DebugLog("Обработка еженедельных квестов");
                    
                    GameObject[] weeklyClaimButtons = FindWeeklyClaimButtons();
                    if (weeklyClaimButtons.Length > 0)
                    {
                        foreach (GameObject claimButton in weeklyClaimButtons)
                        {
                            var button = claimButton.GetComponent<UnityEngine.UI.Button>();
                            if (button != null && button.interactable)
                            {
                                button.onClick.Invoke();
                                MelonLogger.Msg("Получена награда за еженедельный квест");
                            }
                        }
                    }
                    else
                    {
                        DebugManager.DebugLog("Награды за еженедельные квесты не найдены");
                    }
                    
                    _weeklyProcessed = true;
                }
                
                if (_dailyProcessed && _weeklyProcessed)
                {
                    CloseQuestsWindow();
                    BotMain.NextFunction();
                    ResetState();
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка обработки квестов: {ex.Message}");
            }
        }
        
        private static GameObject FindWeeklyButton()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "weeklyButton")
                    {
                        Transform current = obj.transform;
                        string path = "";
                        while (current != null)
                        {
                            path = current.name + "/" + path;
                            current = current.parent;
                        }
                        
                        if (path.Contains("Character/bg/submenus/quests/bg/submenuButtons/"))
                        {
                            return obj;
                        }
                    }
                }
                
                return null;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка поиска weeklyButton: {ex.Message}");
                return null;
            }
        }
        
        private static GameObject[] FindWeeklyClaimButtons()
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            System.Collections.Generic.List<GameObject> claimButtons = new System.Collections.Generic.List<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.activeInHierarchy && obj.name == "claimButton")
                {
                    Transform current = obj.transform;
                    string path = "";
                    while (current != null)
                    {
                        path = current.name + "/" + path;
                        current = current.parent;
                    }
                    
                    if (path.Contains("quests/") && path.Contains("weekly"))
                    {
                        var button = obj.GetComponent<UnityEngine.UI.Button>();
                        if (button != null && button.interactable)
                        {
                            claimButtons.Add(obj);
                        }
                    }
                }
            }
            
            return claimButtons.ToArray();
        }
        
        private static GameObject[] FindClaimButtons()
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            System.Collections.Generic.List<GameObject> claimButtons = new System.Collections.Generic.List<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.activeInHierarchy && obj.name == "claimButton")
                {
                    Transform current = obj.transform;
                    string path = "";
                    while (current != null)
                    {
                        path = current.name + "/" + path;
                        current = current.parent;
                    }
                    
                    if (path.Contains("quests/"))
                    {
                        var button = obj.GetComponent<UnityEngine.UI.Button>();
                        if (button != null && button.interactable)
                        {
                            claimButtons.Add(obj);
                        }
                    }
                }
            }
            
            return claimButtons.ToArray();
        }
        
        private static void CloseQuestsWindow()
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
                        
                        if (path.Contains("Character/"))
                        {
                            var button = obj.GetComponent<UnityEngine.UI.Button>();
                            if (button != null && button.interactable)
                            {
                                button.onClick.Invoke();
                                MelonLogger.Msg("Закрыто окно квестов");
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
        
        private static GameObject FindQuestsButton()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "Quests")
                    {
                        var button = obj.GetComponent<UnityEngine.UI.Button>();
                        if (button != null && button.interactable)
                        {
                            return obj;
                        }
                    }
                }
                
                return null;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка поиска кнопки Quests: {ex.Message}");
                return null;
            }
        }
        
        private static GameObject FindDailyButton()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "dailyButton")
                    {
                        Transform current = obj.transform;
                        string path = "";
                        while (current != null)
                        {
                            path = current.name + "/" + path;
                            current = current.parent;
                        }
                        
                        if (path.Contains("Character/bg/submenus/quests/bg/submenuButtons/"))
                        {
                            return obj;
                        }
                    }
                }
                
                return null;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка поиска dailyButton: {ex.Message}");
                return null;
            }
        }
        
        private static void ResetState()
        {
            _questsClicked = false;
            _questsClickTime = 0f;
            _dailyProcessed = false;
            _weeklyProcessed = false;
        }
    }
}