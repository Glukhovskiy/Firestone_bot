using MelonLoader;
using UnityEngine;
using UnityEngine.EventSystems;
using Il2CppSystem.Collections.Generic;
using System.Reflection;
using System.Collections;

namespace FirestoneBot
{
    public static class MapMissions
    {
        private enum State { OpenMap, CollectRewards, FindNewMissions, HandleMissionPreview, Complete }
        private static State _state = State.OpenMap;
        private static float _waitTimer = 0f;
        private static GameObject _currentMission = null;
        private static int _interactionMethod = 0;
        private static int _activeMissionIndex = 0;
        private static System.Collections.Generic.List<string> _processedMissions = new System.Collections.Generic.List<string>();

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
                            MelonLogger.Msg("Карта миссий открыта");
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
                                _interactionMethod = 0;
                                MelonLogger.Msg("Переход к поиску новых миссий");
                                DebugManager.DebugLog("[MapMissions] Активные награды собраны, ищем новые миссии");
                            }
                        }
                        break;
                        
                    case State.FindNewMissions:
                        if (_currentMission == null)
                        {
                            DebugManager.DebugLog("[MapMissions] Поиск новой миссии на карте");
                            _currentMission = FindNewMissionOnMap();
                            if (_currentMission == null)
                            {
                                DebugManager.DebugLog("[MapMissions] Новые миссии не найдены, завершение");
                                _state = State.Complete;
                                break;
                            }
                            _waitTimer = Time.time + 0.1f;
                            MelonLogger.Msg($"Найдена новая миссия: {_currentMission.name}");
                        }
                        else if (Time.time >= _waitTimer)
                        {
                            DebugManager.DebugLog($"[MapMissions] Попытка взаимодействия методом {_interactionMethod}");
                            TryInteractWithNewMission();
                            _state = State.HandleMissionPreview;
                            _waitTimer = Time.time + 0.1f;
                        }
                        break;
                        
                    case State.HandleMissionPreview:
                        if (Time.time >= _waitTimer)
                        {
                            HandleMissionPreviewWindow();
                            _state = State.FindNewMissions;
                            _currentMission = null;
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
        
        private static GameObject FindNewMissionOnMap()
        {
            DebugManager.DebugLog("[MapMissions] Поиск missionBase объектов на карте");
            
            var allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            
            foreach (var obj in allObjects)
            {
                if (obj != null && obj.activeInHierarchy && obj.name == "missionBase")
                {
                    var path = GetObjectPath(obj);
                    
                    if (path.Contains("menusRoot/mapRoot/mapElements/missions"))
                    {
                        // Проверяем, не обрабатывали ли мы уже эту миссию
                        if (!_processedMissions.Contains(path))
                        {
                            DebugManager.DebugLog($"[MapMissions] Найдена новая миссия: {path}");
                            _processedMissions.Add(path);
                            return obj;
                        }
                        else
                        {
                            DebugManager.DebugLog($"[MapMissions] Миссия уже обработана: {path}");
                        }
                    }
                }
            }
            
            DebugManager.DebugLog("[MapMissions] Новые missionBase объекты не найдены");
            return null;
        }
        
        private static void TryInteractWithNewMission()
        {
            TryInteractWithMission(_currentMission);
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
                        MelonLogger.Msg("Миссия запущена");
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
                    MelonLogger.Msg($"Открыто окно миссии: {parent.name}");
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
            _interactionMethod = 0;
            _activeMissionIndex = 0;
            _processedMissions.Clear();
        }
    }
}