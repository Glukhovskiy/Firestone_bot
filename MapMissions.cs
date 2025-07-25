using MelonLoader;
using UnityEngine;
using UnityEngine.EventSystems;
using Il2CppSystem.Collections.Generic;
using System.Reflection;
using System.Collections;

namespace Firestone_bot
{
    public static class MapMissions
    {
        private enum State { OpenMap, CollectRewards, FindNewMissions, HandleMissionPreview, Complete }
        private static State _state = State.OpenMap;
        private static float _waitTimer = 0f;
        private static GameObject _currentMission = null;
        private static string _currentMissionType = "Unknown";

        private static int _activeMissionIndex = 0;
        private static System.Collections.Generic.List<string> _processedMissions = new System.Collections.Generic.List<string>();

        private static System.Collections.Generic.Dictionary<string, int> _missionPriorities = new System.Collections.Generic.Dictionary<string, int>();
        private static System.Collections.Generic.List<(GameObject mission, int priority)> _sortedMissions = new System.Collections.Generic.List<(GameObject, int)>();
        private static int _currentSortedIndex = 0;

        public static void ProcessMapMissions()
        {
            try
            {
                switch (_state)
                {
                    case State.OpenMap:
                        var mapButton = FindMapMissionButton();
                        DebugManager.DebugLog($"[MapMissions] Кнопка карты найдена: {mapButton != null}");
                        if (GameUtils.ClickButton(mapButton))
                        {
                            _waitTimer = Time.time + 2f;
                            _state = State.CollectRewards;
                            DebugManager.DebugLog("Карта миссий открыта");
                            DebugManager.DebugLog($"[MapMissions] Переход к сбору наград, ожидание до: {_waitTimer}");
                        }
                        else
                        {
                            DebugManager.DebugLog("[MapMissions] Кнопка карты не найдена, завершение модуля");
                            BotMain.NextFunction();
                            ResetState();
                        }
                        break;
                        
                    case State.CollectRewards:
                        if (Time.time >= _waitTimer)
                        {
                            DebugManager.DebugLog("[MapMissions] Начинаем сбор активных наград");
                            if (!CollectActiveRewards())
                            {
                                _state = State.FindNewMissions;
                                _currentMission = null;
                                DebugManager.DebugLog("Переход к поиску новых миссий");
                                DebugManager.DebugLog("[MapMissions] Активные награды собраны, ищем новые миссии");
                            }
                        }
                        break;
                        
                    case State.FindNewMissions:
                        if (_currentMission == null)
                        {
                            if (_sortedMissions.Count == 0)
                            {
                                LoadMissionPriorities();
                                _sortedMissions = FindAndSortMissions();
                                _currentSortedIndex = 0;
                            }
                            
                            if (_currentSortedIndex >= _sortedMissions.Count)
                            {
                                DebugManager.DebugLog("[MapMissions] Все миссии обработаны, завершение");
                                _state = State.Complete;
                                break;
                            }
                            
                            _currentMission = _sortedMissions[_currentSortedIndex].mission;
                            var parentMission = _currentMission.transform.parent?.gameObject;
                            _currentMissionType = parentMission != null ? DetectMissionType(parentMission) : "Unknown";
                            _waitTimer = Time.time + 0.1f;
                            var missionName = GetMissionName(_currentMission);
                            DebugManager.DebugLog($"Выбрана миссия с приоритетом {_sortedMissions[_currentSortedIndex].priority}: {missionName} ({_currentMissionType})");
                        }
                        else if (Time.time >= _waitTimer)
                        {
                            DebugManager.DebugLog("[MapMissions] Попытка взаимодействия с миссией");
                            TryInteractWithMission(_currentMission);
                            _state = State.HandleMissionPreview;
                            _waitTimer = Time.time + 0.1f;
                        }
                        break;
                        
                    case State.HandleMissionPreview:
                        if (Time.time >= _waitTimer)
                        {
                            HandleMissionPreviewWindow();
                            _currentSortedIndex++;
                            _state = State.FindNewMissions;
                            _currentMission = null;
                            _currentMissionType = "Unknown";
                            _currentMissionType = "Unknown";
                        }
                        break;
                        
                    case State.Complete:
                        DebugManager.DebugLog("[MapMissions] Завершение работы модуля");
                        var missionRewardsClose = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/popups/MissionRewards/bg/closeButton");
                        GameUtils.ClickButton(missionRewardsClose);
                        GameUtils.CloseWindow("menus/WorldMap");
                        BotMain.NextFunction();
                        ResetState();
                        break;
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка в ProcessMapMissions: {ex.Message}");
                DebugManager.DebugLog($"[MapMissions] Исключение: {ex.Message}");
            }
        }
        
        private static bool CollectActiveRewards()
        {
            var path = $"menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/WorldMap/submenus/mapMissionsSubmenu/activeMissionsCanvas/activeMissions/Viewport/grid/activeMission ({_activeMissionIndex})";
            var activeMission = GameUtils.FindByPath(path);
            
            DebugManager.DebugLog($"[MapMissions] Поиск активной миссии {_activeMissionIndex}: {activeMission != null}");
            
            if (activeMission != null)
            {
                if (GameUtils.ClickButton(activeMission))
                {
                    MelonLogger.Msg($"Собрана награда с активной миссии {_activeMissionIndex}");
                    _activeMissionIndex++;
                    return true;
                }
                else
                {
                    DebugManager.DebugLog($"[MapMissions] Не удалось кликнуть по активной миссии {_activeMissionIndex}");
                }
            }
            
            DebugManager.DebugLog($"[MapMissions] Активных миссий больше нет (проверено до индекса {_activeMissionIndex})");
            return false;
        }
        
        private static System.Collections.Generic.List<(GameObject mission, int priority)> FindAndSortMissions()
        {
            var missions = new System.Collections.Generic.List<(GameObject mission, int priority)>();
            var allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

            
            foreach (var obj in allObjects)
            {
                if (obj != null && obj.activeInHierarchy && obj.name == "missionBase")
                {
                    var path = GetObjectPath(obj);
                    
                    if (path.Contains("menusRoot/mapRoot/mapElements/missions") && !_processedMissions.Contains(path))
                    {

                        
                        var parentMission = obj.transform.parent?.gameObject;
                        var missionType = parentMission != null ? DetectMissionType(parentMission) : "Unknown";
                        var priority = _missionPriorities.ContainsKey(missionType.ToLower()) ? _missionPriorities[missionType.ToLower()] : 998;
                        
                        missions.Add((obj, priority));
                        _processedMissions.Add(path);
                        var missionKey = ExtractMissionKey(path);
                        DebugManager.DebugLog($"[MapMissions] Миссия: {missionKey}, тип: {missionType}, приоритет: {priority}");
                    }
                }
            }
            
            missions.Sort((a, b) => a.priority.CompareTo(b.priority));
            return missions;
        }
        

        
        private static void LoadMissionPriorities()
        {
            _missionPriorities = ConfigManager.Config.MissionPriorities;
            DebugManager.DebugLog($"[MapMissions] Загружено {_missionPriorities.Count} приоритетов миссий");
        }
        
        private static string ExtractMissionKey(string path)
        {
            var pathParts = path.Split('/');
            if (pathParts.Length >= 6)
            {
                return $"{pathParts[4]}/{pathParts[5]}"; // Location/MissionName
            }
            return "Unknown/Unknown";
        }
        
        private static string GetMissionName(GameObject mission)
        {
            var path = GetObjectPath(mission);
            var pathParts = path.Split('/');
            if (pathParts.Length >= 6)
            {
                return pathParts[5]; // MissionName
            }
            return "Unknown";
        }
        

        
        private static void HandleMissionPreviewWindow()
        {
            var startButton = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/popups/PreviewMission/bg/managementBg/startMissionButton");
            
            if (startButton != null)
            {
                var button = startButton.GetComponent<UnityEngine.UI.Button>();
                if (button != null && button.interactable)
                {
                    if (GameUtils.ClickButton(startButton))
                    {
                        var missionName = GetMissionName(_currentMission);
                        MelonLogger.Msg($"Миссия запущена: {missionName} ({_currentMissionType})");
                        return;
                    }
                }
            }
            
            GameUtils.CloseWindow("popups/PreviewMission");
            DebugManager.DebugLog("[MapMissions] Окно PreviewMission закрыто");
        }
        

        

        
        private static string GetObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform current = obj.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }
        
        private static GameObject FindMapMissionButton()
        {
            try
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "MapMissions")
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
                MelonLogger.Error($"Ошибка поиска кнопки миссий: {ex.Message}");
                return null;
            }
        }
        
        private static void TryInteractWithMission(GameObject mission)
        {
            try
            {
                var parent = mission.transform.parent?.gameObject;
                if (parent != null)
                {
                    var parentScreenPos = Camera.main.WorldToScreenPoint(parent.transform.position);
                    var parentEventData = new PointerEventData(EventSystem.current) { position = parentScreenPos };
                    ExecuteEvents.Execute(parent, parentEventData, ExecuteEvents.pointerClickHandler);
                    parent.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
                    parent.SendMessage("OnMouseUpAsButton", SendMessageOptions.DontRequireReceiver);
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка открытия миссии: {ex.Message}");
            }
        }
        
        private static void ResetState()
        {
            _state = State.OpenMap;
            _waitTimer = 0f;
            _currentMission = null;
            _currentMissionType = "Unknown";
            _activeMissionIndex = 0;
            _processedMissions.Clear();
            _sortedMissions.Clear();
            _currentSortedIndex = 0;
        }
        
        private static string DetectMissionType(GameObject missionParent)
        {
            try
            {
                var missionIcon = missionParent.transform.Find("missionBg/missionIcon");
                var spriteRenderer = missionIcon?.GetComponent<SpriteRenderer>();
                
                if (spriteRenderer?.sprite != null)
                {
                    var spriteName = spriteRenderer.sprite.name;
                    DebugManager.DebugLog($"[DetectMissionType] Имя спрайта: '{spriteName}'");
                    
                    // Убираем (Clone) если есть
                    if (spriteName.EndsWith("(Clone)"))
                    {
                        spriteName = spriteName.Substring(0, spriteName.Length - "(Clone)".Length);
                    }
                    
                    // Извлекаем тип из имени спрайта (mission[Type]Icon)
                    if (spriteName.StartsWith("mission") && spriteName.EndsWith("Icon"))
                    {
                        var typeStart = "mission".Length;
                        var typeEnd = spriteName.LastIndexOf("Icon");
                        
                        if (typeEnd > typeStart)
                        {
                            var extractedType = spriteName.Substring(typeStart, typeEnd - typeStart).ToLower();
                            DebugManager.DebugLog($"[DetectMissionType] Извлеченный тип: '{extractedType}'");
                            return extractedType;
                        }
                    }
                    else
                    {
                        DebugManager.DebugLog($"[DetectMissionType] Спрайт не соответствует формату mission[Type]Icon");
                    }
                }
                else
                {
                    DebugManager.DebugLog($"[DetectMissionType] SpriteRenderer или sprite не найден");
                }
                
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}