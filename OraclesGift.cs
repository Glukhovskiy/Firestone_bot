using MelonLoader;
using UnityEngine;

namespace Firestone_bot
{
    public static class OraclesGift
    {
        private static bool _oraclesGiftClicked = false;
        private static float _oraclesGiftClickTime = 0f;
        private static float _lastExecutionTime = 0f;
        private const float EXECUTION_INTERVAL = 300f; // 5 минут
        
        public static void ProcessOraclesGift()
        {
            // Проверяем, прошло ли 5 минут с последнего выполнения
            if (Time.time - _lastExecutionTime < EXECUTION_INTERVAL)
            {
                BotMain.NextFunction();
                return;
            }
            
            if (!_oraclesGiftClicked)
            {
                GameObject oraclesGiftButton = FindOraclesGiftButton();
                if (oraclesGiftButton != null)
                {
                    DebugManager.DebugLog("Кнопка OraclesGift найдена");
                    var button = oraclesGiftButton.GetComponent<UnityEngine.UI.Button>();
                    if (button != null && button.interactable)
                    {
                        button.onClick.Invoke();
                        _oraclesGiftClicked = true;
                        _oraclesGiftClickTime = Time.time;
                        MelonLogger.Msg("Нажата кнопка OraclesGift");
                        return;
                    }
                }
                else
                {
                    DebugManager.DebugLog("Кнопка OraclesGift не найдена");
                    _lastExecutionTime = Time.time;
                    BotMain.NextFunction();
                    ResetState();
                    return;
                }
            }
            else if (Time.time - _oraclesGiftClickTime >= 1f)
            {
                ProcessOraclesGiftWindow();
            }
        }
        
        private static void ProcessOraclesGiftWindow()
        {
            try
            {
                DebugManager.DebugLog("Обработка окна OraclesGift");
                
                GameObject oraclesGiftStoreButton = FindOraclesGiftStoreButton();
                if (oraclesGiftStoreButton != null)
                {
                    DebugManager.DebugLog($"Найдена кнопка: {oraclesGiftStoreButton.name}");
                    
                    var button = oraclesGiftStoreButton.GetComponent<UnityEngine.UI.Button>();
                    if (button != null)
                    {
                        DebugManager.DebugLog($"Компонент Button найден, interactable: {button.interactable}");
                        if (button.interactable)
                        {
                            button.onClick.Invoke();
                            MelonLogger.Msg("Нажата кнопка oraclesGift в магазине");
                        }
                        else
                        {
                            DebugManager.DebugLog("Кнопка oraclesGift неактивна");
                        }
                    }
                    else
                    {
                        DebugManager.DebugLog("Компонент Button не найден");
                    }
                }
                else
                {
                    DebugManager.DebugLog("Кнопка oraclesGift в магазине не найдена");
                }
                
                CloseOraclesGiftWindow();
                _lastExecutionTime = Time.time;
                BotMain.NextFunction();
                ResetState();
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка обработки OraclesGift: {ex.Message}");
            }
        }
        
        private static GameObject FindOraclesGiftButton()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "OraclesGift")
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
                MelonLogger.Error($"Ошибка поиска кнопки OraclesGift: {ex.Message}");
                return null;
            }
        }
        
        private static GameObject FindOraclesGiftStoreButton()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "oraclesGift")
                    {
                        Transform current = obj.transform;
                        string path = "";
                        while (current != null)
                        {
                            path = current.name + "/" + path;
                            current = current.parent;
                        }
                        
                        if (path.Contains("OracleStore/bg/submenus/valueBundles/"))
                        {
                            // Проверяем сам объект
                            var button = obj.GetComponent<UnityEngine.UI.Button>();
                            if (button != null)
                                return obj;
                            
                            // Проверяем дочерние объекты
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
                MelonLogger.Error($"Ошибка поиска oraclesGift в магазине: {ex.Message}");
                return null;
            }
        }
        
        private static void CloseOraclesGiftWindow()
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
                        
                        if (path.Contains("OracleStore/"))
                        {
                            var button = obj.GetComponent<UnityEngine.UI.Button>();
                            if (button != null && button.interactable)
                            {
                                button.onClick.Invoke();
                                MelonLogger.Msg("Закрыто окно OraclesGift");
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
            _oraclesGiftClicked = false;
            _oraclesGiftClickTime = 0f;
        }
    }
}