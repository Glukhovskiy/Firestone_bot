using MelonLoader;
using UnityEngine;

namespace Firestone_bot
{
    public static class OracleRituals
    {
        private static bool _oracleRitualsClicked = false;
        private static float _oracleRitualsClickTime = 0f;
        
        public static void ProcessOracleRituals()
        {
            if (!_oracleRitualsClicked)
            {
                GameObject oracleRitualsButton = FindOracleRitualsButton();
                if (oracleRitualsButton != null)
                {
                    DebugManager.DebugLog("Кнопка OracleRituals найдена");
                    var button = oracleRitualsButton.GetComponent<UnityEngine.UI.Button>();
                    if (button != null && button.interactable)
                    {
                        button.onClick.Invoke();
                        _oracleRitualsClicked = true;
                        _oracleRitualsClickTime = Time.time;
                        MelonLogger.Msg("Нажата кнопка OracleRituals");
                        return;
                    }
                }
                else
                {
                    DebugManager.DebugLog("Кнопка OracleRituals не найдена");
                    BotMain.NextFunction();
                    ResetState();
                    return;
                }
            }
            else if (Time.time - _oracleRitualsClickTime >= 1f)
            {
                ProcessOracleRitualsWindow();
            }
        }
        
        private static void ProcessOracleRitualsWindow()
        {
            try
            {
                DebugManager.DebugLog("Обработка окна OracleRituals");
                
                bool foundActiveButtons = false;
                
                // Сначала ищем claimButton
                GameObject[] claimButtons = FindClaimButtons();
                if (claimButtons.Length > 0)
                {
                    foreach (GameObject claimButton in claimButtons)
                    {
                        var button = claimButton.GetComponent<UnityEngine.UI.Button>();
                        if (button != null && button.interactable)
                        {
                            button.onClick.Invoke();
                            MelonLogger.Msg("Получена награда за ритуал");
                            foundActiveButtons = true;
                        }
                    }
                }
                
                // Затем ищем startButton
                GameObject[] startButtons = FindStartButtons();
                if (startButtons.Length > 0)
                {
                    foreach (GameObject startButton in startButtons)
                    {
                        var button = startButton.GetComponent<UnityEngine.UI.Button>();
                        if (button != null && button.interactable)
                        {
                            button.onClick.Invoke();
                            MelonLogger.Msg("Начат ритуал");
                            foundActiveButtons = true;
                        }
                    }
                }
                
                if (!foundActiveButtons)
                {
                    DebugManager.DebugLog("Активные кнопки не найдены");
                }
                
                CloseOracleRitualsWindow();
                BotMain.NextFunction();
                ResetState();
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка обработки OracleRituals: {ex.Message}");
            }
        }
        
        private static GameObject[] FindClaimButtons()
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            System.Collections.Generic.List<GameObject> claimButtons = new System.Collections.Generic.List<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.activeInHierarchy && obj.name == "claimButton")
                {
                    var button = obj.GetComponent<UnityEngine.UI.Button>();
                    if (button != null && button.interactable)
                    {
                        claimButtons.Add(obj);
                    }
                }
            }
            
            return claimButtons.ToArray();
        }
        
        private static GameObject[] FindStartButtons()
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            System.Collections.Generic.List<GameObject> startButtons = new System.Collections.Generic.List<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.activeInHierarchy && obj.name == "startButton")
                {
                    var button = obj.GetComponent<UnityEngine.UI.Button>();
                    if (button != null && button.interactable)
                    {
                        startButtons.Add(obj);
                    }
                }
            }
            
            return startButtons.ToArray();
        }
        
        private static void CloseOracleRitualsWindow()
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
                            MelonLogger.Msg("Закрыто окно OracleRituals");
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
        
        private static GameObject FindOracleRitualsButton()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "OracleRituals")
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
                MelonLogger.Error($"Ошибка поиска кнопки OracleRituals: {ex.Message}");
                return null;
            }
        }
        
        private static void ResetState()
        {
            _oracleRitualsClicked = false;
            _oracleRitualsClickTime = 0f;
        }
    }
}