using MelonLoader;
using UnityEngine;

namespace FirestoneBot
{
    public static class OracleBlessings
    {
        private static bool _oracleBlessingsClicked = false;
        private static float _oracleBlessingsClickTime = 0f;
        private static GameObject[] _blessingsList = null;
        private static int _currentBlessingIndex = 0;
        private static bool _blessingWindowOpened = false;
        private static float _blessingOpenTime = 0f;
        
        public static void ProcessOracleBlessings()
        {
            if (!_oracleBlessingsClicked)
            {
                GameObject oracleBlessingsButton = FindOracleBlessingsButton();
                if (oracleBlessingsButton != null)
                {
                    DebugManager.DebugLog("Кнопка OracleBlessings найдена");
                    var button = oracleBlessingsButton.GetComponent<UnityEngine.UI.Button>();
                    if (button != null && button.interactable)
                    {
                        button.onClick.Invoke();
                        _oracleBlessingsClicked = true;
                        _oracleBlessingsClickTime = Time.time;
                        MelonLogger.Msg("Нажата кнопка OracleBlessings");
                        return;
                    }
                }
                else
                {
                    DebugManager.DebugLog("Кнопка OracleBlessings не найдена");
                    BotMain.NextFunction();
                    ResetState();
                    return;
                }
            }
            else if (Time.time - _oracleBlessingsClickTime >= 1f)
            {
                ProcessOracleBlessingsWindow();
            }
        }
        
        private static void ProcessOracleBlessingsWindow()
        {
            try
            {
                if (_blessingsList == null)
                {
                    DebugManager.DebugLog("Поиск списка blessing");
                    _blessingsList = FindBlessings();
                    _currentBlessingIndex = 0;
                }
                
                if (!_blessingWindowOpened)
                {
                    if (_currentBlessingIndex >= _blessingsList.Length)
                    {
                        DebugManager.DebugLog("Все blessing обработаны");
                        CloseOracleBlessingsWindow();
                        BotMain.NextFunction();
                        ResetState();
                        return;
                    }
                    
                    GameObject currentBlessing = _blessingsList[_currentBlessingIndex];
                    if (currentBlessing != null)
                    {
                        var button = currentBlessing.GetComponent<UnityEngine.UI.Button>();
                        if (button != null && button.interactable)
                        {
                            button.onClick.Invoke();
                            _blessingWindowOpened = true;
                            _blessingOpenTime = Time.time;
                            MelonLogger.Msg($"Открыто blessing {_currentBlessingIndex + 1}");
                        }
                        else
                        {
                            _currentBlessingIndex++;
                        }
                    }
                    else
                    {
                        _currentBlessingIndex++;
                    }
                }
                else if (Time.time - _blessingOpenTime >= 0.5f)
                {
                    GameObject buyButton = FindBuyUpgradeButton();
                    if (buyButton != null)
                    {
                        var button = buyButton.GetComponent<UnityEngine.UI.Button>();
                        if (button != null && button.interactable)
                        {
                            button.onClick.Invoke();
                            MelonLogger.Msg("Куплено улучшение blessing");
                        }
                        else
                        {
                            DebugManager.DebugLog("Кнопка покупки неактивна");
                        }
                    }
                    else
                    {
                        DebugManager.DebugLog("Кнопка покупки не найдена");
                    }
                    
                    CloseCurrentBlessingWindow();
                    _blessingWindowOpened = false;
                    _currentBlessingIndex++;
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка обработки OracleBlessings: {ex.Message}");
            }
        }
        
        private static GameObject FindOracleBlessingsButton()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "OracleBlessings")
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
                MelonLogger.Error($"Ошибка поиска кнопки OracleBlessings: {ex.Message}");
                return null;
            }
        }
        
        private static GameObject[] FindBlessings()
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            System.Collections.Generic.List<GameObject> blessings = new System.Collections.Generic.List<GameObject>();
            
            DebugManager.DebugLog($"Проверяем {allObjects.Length} объектов");
            
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    Transform current = obj.transform;
                    string path = "";
                    while (current != null)
                    {
                        path = current.name + "/" + path;
                        current = current.parent;
                    }
                    
                    // Показываем все объекты в пути Oracle
                    if (path.Contains("Oracle/submenus/bg/blessingsSubmenu/blessings/"))
                    {
                        DebugManager.DebugLog($"Объект в пути blessings: {obj.name} - {path}");
                        
                        var button = obj.GetComponent<UnityEngine.UI.Button>();
                        if (button != null)
                        {
                            DebugManager.DebugLog($"Объект {obj.name} имеет Button, interactable: {button.interactable}");
                            if (button.interactable)
                            {
                                blessings.Add(obj);
                            }
                        }
                    }
                }
            }
            
            DebugManager.DebugLog($"Найдено {blessings.Count} blessing");
            return blessings.ToArray();
        }
        
        private static GameObject FindBuyUpgradeButton()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "buyUpgradeButton")
                    {
                        Transform current = obj.transform;
                        string path = "";
                        while (current != null)
                        {
                            path = current.name + "/" + path;
                            current = current.parent;
                        }
                        
                        if (path.Contains("OracleBlessingPreview/bg/darkBlueFrame/buttonRect/"))
                        {
                            return obj;
                        }
                    }
                }
                
                return null;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка поиска buyUpgradeButton: {ex.Message}");
                return null;
            }
        }
        
        private static void CloseCurrentBlessingWindow()
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
                        
                        if (path.Contains("OracleBlessingPreview/"))
                        {
                            var button = obj.GetComponent<UnityEngine.UI.Button>();
                            if (button != null && button.interactable)
                            {
                                button.onClick.Invoke();
                                DebugManager.DebugLog("Закрыто окно blessing");
                                break;
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка закрытия окна blessing: {ex.Message}");
            }
        }
        
        private static void CloseOracleBlessingsWindow()
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
                        
                        if (path.Contains("Oracle/"))
                        {
                            var button = obj.GetComponent<UnityEngine.UI.Button>();
                            if (button != null && button.interactable)
                            {
                                button.onClick.Invoke();
                                MelonLogger.Msg("Закрыто окно OracleBlessings");
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
            _oracleBlessingsClicked = false;
            _oracleBlessingsClickTime = 0f;
            _blessingsList = null;
            _currentBlessingIndex = 0;
            _blessingWindowOpened = false;
            _blessingOpenTime = 0f;
        }
    }
}