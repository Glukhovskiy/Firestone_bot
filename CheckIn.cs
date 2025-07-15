using MelonLoader;
using UnityEngine;

namespace FirestoneBot
{
    public static class CheckIn
    {
        private static bool _checkInClicked = false;
        private static float _checkInClickTime = 0f;
        
        public static void ProcessCheckIn()
        {
            if (!_checkInClicked)
            {
                GameObject checkInButton = FindCheckInButton();
                if (checkInButton != null)
                {
                    DebugManager.DebugLog("Кнопка CheckIn найдена");
                    var button = checkInButton.GetComponent<UnityEngine.UI.Button>();
                    if (button != null && button.interactable)
                    {
                        button.onClick.Invoke();
                        _checkInClicked = true;
                        _checkInClickTime = Time.time;
                        MelonLogger.Msg("Нажата кнопка CheckIn");
                        return;
                    }
                }
                else
                {
                    DebugManager.DebugLog("Кнопка CheckIn не найдена");
                    BotMain.NextFunction();
                    ResetState();
                    return;
                }
            }
            else if (Time.time - _checkInClickTime >= 1f)
            {
                ProcessCheckInWindow();
            }
        }
        
        private static void ProcessCheckInWindow()
        {
            try
            {
                DebugManager.DebugLog("Обработка окна CheckIn");
                
                GameObject checkInStoreButton = FindCheckInStoreButton();
                if (checkInStoreButton != null)
                {
                    var button = checkInStoreButton.GetComponent<UnityEngine.UI.Button>();
                    if (button != null && button.interactable)
                    {
                        button.onClick.Invoke();
                        MelonLogger.Msg("Нажата кнопка checkIn в магазине");
                    }
                    else
                    {
                        DebugManager.DebugLog("Кнопка checkIn неактивна");
                    }
                }
                else
                {
                    DebugManager.DebugLog("Кнопка checkIn в магазине не найдена");
                }
                
                CloseCheckInWindow();
                BotMain.NextFunction();
                ResetState();
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка обработки CheckIn: {ex.Message}");
            }
        }
        
        private static GameObject FindCheckInButton()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "CheckIn")
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
                MelonLogger.Error($"Ошибка поиска кнопки CheckIn: {ex.Message}");
                return null;
            }
        }
        
        private static GameObject FindCheckInStoreButton()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "checkIn")
                    {
                        Transform current = obj.transform;
                        string path = "";
                        while (current != null)
                        {
                            path = current.name + "/" + path;
                            current = current.parent;
                        }
                        
                        if (path.Contains("Store/bg/submenus/dailyRewards/"))
                        {
                            var button = obj.GetComponent<UnityEngine.UI.Button>();
                            if (button != null)
                                return obj;
                            
                            button = obj.GetComponentInChildren<UnityEngine.UI.Button>();
                            if (button != null)
                                return button.gameObject;
                        }
                    }
                }
                
                return null;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка поиска checkIn в магазине: {ex.Message}");
                return null;
            }
        }
        
        private static void CloseCheckInWindow()
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
                        
                        if (path.Contains("Store/"))
                        {
                            var button = obj.GetComponent<UnityEngine.UI.Button>();
                            if (button != null && button.interactable)
                            {
                                button.onClick.Invoke();
                                MelonLogger.Msg("Закрыто окно CheckIn");
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
        
        private static void ResetState()
        {
            _checkInClicked = false;
            _checkInClickTime = 0f;
        }
    }
}