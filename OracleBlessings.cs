using MelonLoader;
using UnityEngine;
using System.IO;
using System.Linq;

namespace Firestone_bot
{
    public static class OracleBlessings
    {
        private static bool _oracleBlessingsClicked = false;
        private static float _oracleBlessingsClickTime = 0f;
        private static GameObject[] _blessingsList = null;
        private static int _currentBlessingIndex = 0;
        private static bool _blessingWindowOpened = false;
        private static float _blessingOpenTime = 0f;

        
        private static readonly System.Collections.Generic.Dictionary<string, string> _blessingNames = new()
        {
            { "0", "Raining gold" },
            { "1", "Mana heroes" },
            { "2", "Rage heroes" },
            { "3", "Energy heroes" },
            { "4", "Tank specialization" },
            { "5", "Healer specialization" },
            { "6", "Damage spexialization" },
            { "7", "Fist fight" },
            { "8", "Precision" },
            { "9", "Magic spells" },
            { "10", "Guardian power" },
            { "11", "Prestigious" },
            { "12", "Fate" }
        };
        
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
                            string displayName = GetBlessingDisplayName(currentBlessing.name);
                            MelonLogger.Msg($"Открыто {displayName}");
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
                            string blessingName = _blessingsList[_currentBlessingIndex].name;
                            string displayName = GetBlessingDisplayName(blessingName);
                            button.onClick.Invoke();
                            MelonLogger.Msg($"Куплено благословение: {displayName}");
                        }
                        else
                        {
                            string blessingName = _blessingsList[_currentBlessingIndex].name;
                            string displayName = GetBlessingDisplayName(blessingName);
                            DebugManager.DebugLog($"Недостаточно ресурсов для: {displayName}");
                        }
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
            System.Collections.Generic.List<(GameObject obj, int priority)> blessings = new();
            
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
                    
                    if (path.Contains("Oracle/submenus/bg/blessingsSubmenu/blessings/"))
                    {
                        var button = obj.GetComponent<UnityEngine.UI.Button>();
                        if (button != null && button.interactable)
                        {
                            int priority = GetBlessingPriority(obj.name);
                            string displayName = GetBlessingDisplayName(obj.name);
                            DebugManager.DebugLog($"Обработка благословения: {obj.name} -> приоритет: {priority}");
                            if (priority < 999)
                            {
                                blessings.Add((obj, priority));
                                MelonLogger.Msg($"Найдено благословение: {displayName} (приоритет: {priority})");
                            }
                            else
                            {
                                DebugManager.DebugLog($"Благословение пропущено (приоритет 999): {displayName}");
                            }
                        }
                    }
                }
            }
            
            var sortedBlessings = blessings.OrderBy(x => x.priority).Select(x => x.obj).ToArray();
            DebugManager.DebugLog($"Найдено {sortedBlessings.Length} активных благословений");
            return sortedBlessings;
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
        

        
        private static int GetBlessingPriority(string blessingName)
        {
            if (blessingName.StartsWith("blessing (") && blessingName.EndsWith(")"))
            {
                string index = blessingName.Substring(10, blessingName.Length - 11);
                return ConfigManager.Config.OracleBlessings.TryGetValue(index, out int priority) ? priority : 998;
            }
            return ConfigManager.Config.OracleBlessings.TryGetValue(blessingName, out int p) ? p : 998;
        }
        
        private static string GetBlessingDisplayName(string blessingName)
        {
            if (blessingName.StartsWith("blessing (") && blessingName.EndsWith(")"))
            {
                string index = blessingName.Substring(10, blessingName.Length - 11);
                if (_blessingNames.TryGetValue(index, out string displayName))
                    return $"blessing ({index}) - {displayName}";
            }
            return blessingName;
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