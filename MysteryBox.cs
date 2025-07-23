using MelonLoader;
using UnityEngine;

namespace Firestone_bot
{
    public static class MysteryBox
    {
        private static bool _mysteryBoxClicked = false;
        private static float _mysteryBoxClickTime = 0f;
        
        public static void ProcessMysteryBox()
        {
            if (!_mysteryBoxClicked)
            {
                GameObject mysteryBoxButton = FindMysteryBoxButton();
                if (mysteryBoxButton != null)
                {
                    DebugManager.DebugLog("Кнопка MysteryBox найдена");
                    var button = mysteryBoxButton.GetComponent<UnityEngine.UI.Button>();
                    if (button != null && button.interactable)
                    {
                        button.onClick.Invoke();
                        _mysteryBoxClicked = true;
                        _mysteryBoxClickTime = Time.time;
                        MelonLogger.Msg("Нажата кнопка MysteryBox");
                    }
                }
                else
                {
                    DebugManager.DebugLog("Кнопка MysteryBox не найдена");
                    BotMain.NextFunction();
                    ResetState();
                    return;
                }
            }
            else if (Time.time - _mysteryBoxClickTime >= 1f)
            {
                ProcessMysteryBoxWindow();
            }
        }
        
        private static void ProcessMysteryBoxWindow()
        {
            try
            {
                DebugManager.DebugLog("Обработка окна MysteryBox");
                
                GameObject mysteryBoxStoreButton = FindMysteryBoxStoreButton();
                if (mysteryBoxStoreButton != null)
                {
                    DebugManager.DebugLog($"Найдена кнопка: {mysteryBoxStoreButton.name}");
                    
                    var button = mysteryBoxStoreButton.GetComponent<UnityEngine.UI.Button>();
                    if (button != null)
                    {
                        DebugManager.DebugLog($"Компонент Button найден, interactable: {button.interactable}");
                        if (button.interactable)
                        {
                            button.onClick.Invoke();
                            MelonLogger.Msg("Нажата кнопка mysteryBox в магазине");
                        }
                        else
                        {
                            DebugManager.DebugLog("Кнопка mysteryBox неактивна");
                        }
                    }
                    else
                    {
                        DebugManager.DebugLog("Компонент Button не найден");
                    }
                }
                else
                {
                    DebugManager.DebugLog("Кнопка mysteryBox в магазине не найдена");
                }
                
                CloseMysteryBoxWindow();
                BotMain.NextFunction();
                ResetState();
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка обработки MysteryBox: {ex.Message}");
            }
        }
        
        private static GameObject FindMysteryBoxStoreButton()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "mysteryBox")
                    {
                        Transform current = obj.transform;
                        string path = "";
                        while (current != null)
                        {
                            path = current.name + "/" + path;
                            current = current.parent;
                        }
                        
                        if (path.Contains("Store/bg/submenus/valueBundle/"))
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
                MelonLogger.Error($"Ошибка поиска mysteryBox в магазине: {ex.Message}");
                return null;
            }
        }
        
        private static void CloseMysteryBoxWindow()
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
                                MelonLogger.Msg("Закрыто окно MysteryBox");
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
        
        private static GameObject FindMysteryBoxButton()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "MysteryBox")
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
                MelonLogger.Error($"Ошибка поиска кнопки MysteryBox: {ex.Message}");
                return null;
            }
        }
        
        private static void ResetState()
        {
            _mysteryBoxClicked = false;
            _mysteryBoxClickTime = 0f;
        }
    }
}