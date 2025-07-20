using MelonLoader;
using UnityEngine;

namespace FirestoneBot
{
    public static class Tanks
    {
        private enum State { OpenWarfront, ProcessClaim, OpenDailyMissions, WaitForDailyWindow, OpenDungeonMissions, WaitForDungeonWindow, ProcessDungeonFights, WaitForBattleResult, CloseDungeonMissions, WaitAfterDungeonClose, OpenLiberationMissions, WaitForLiberationWindow, ProcessLiberationFights, CloseWindow, Complete }
        private static State _state = State.OpenWarfront;
        private static float _waitTimer = 0f;
        private static int _currentMissionIndex = 0;
        
        public static void ProcessTanks()
        {
            switch (_state)
            {
                case State.OpenWarfront:
                    var warfrontButton = GameUtils.FindByPath("battleRoot/battleMain/battleCanvas/SafeArea/leftSideUI/notifications/Viewport/grid/WarfrontCampaign");
                    if (GameUtils.ClickButton(warfrontButton))
                    {
                        MelonLogger.Msg("Нажата кнопка WarfrontCampaign");
                        _waitTimer = Time.time + 0.2f;
                        _state = State.ProcessClaim;
                    }
                    else
                    {
                        _state = State.Complete;
                    }
                    break;
                    
                case State.ProcessClaim:
                    if (Time.time >= _waitTimer)
                    {
                        var claimButton = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/WorldMap/submenus/warfrontCampaignSubmenu/loot/claimButton");
                        if (GameUtils.ClickButton(claimButton))
                        {
                            MelonLogger.Msg("Награда за танки собрана");
                        }
                        _waitTimer = Time.time + 0.2f;
                        _state = State.OpenDailyMissions;
                    }
                    break;
                    
                case State.OpenDailyMissions:
                    if (Time.time >= _waitTimer)
                    {
                        var dailyMissionsButton = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/WorldMap/submenus/warfrontCampaignSubmenu/dailyMissionsButton");
                        if (GameUtils.ClickButton(dailyMissionsButton))
                        {
                            MelonLogger.Msg("Открыты ежедневные миссии");
                            _waitTimer = Time.time + 0.2f;
                            _state = State.WaitForDailyWindow;
                        }
                        else
                        {
                            _state = State.CloseWindow;
                        }
                    }
                    break;
                    
                case State.WaitForDailyWindow:
                    if (Time.time >= _waitTimer)
                        _state = State.OpenDungeonMissions;
                    break;
                    
                case State.OpenDungeonMissions:
                    var openButton = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/popups/WFDailyMissions/bg/dungeonMissions/openButton");
                    if (GameUtils.ClickButton(openButton))
                    {
                        MelonLogger.Msg("Открыты миссии подземелий");
                        _waitTimer = Time.time + 0.2f;
                        _state = State.WaitForDungeonWindow;
                    }
                    else
                    {
                        _state = State.CloseWindow;
                    }
                    break;
                    
                case State.WaitForDungeonWindow:
                    if (Time.time >= _waitTimer)
                        _state = State.ProcessDungeonFights;
                    break;
                    
                case State.ProcessDungeonFights:
                    if (!ProcessNextDungeonFight())
                    {
                        _state = State.CloseDungeonMissions;
                    }
                    break;
                    
                case State.WaitForBattleResult:
                    var battleWonClose = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/popups/WFBattleWon/bg/closeButton");
                    if (GameUtils.ClickButton(battleWonClose))
                    {
                        MelonLogger.Msg("Бой завершен, окно закрыто");
                        _state = _currentMissionIndex < 100 ? State.ProcessDungeonFights : State.ProcessLiberationFights;
                    }
                    break;
                    
                case State.CloseDungeonMissions:
                    var closeDungeonBtn = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/popups/WFDungeonMissions/bg/closeButton");
                    GameUtils.ClickButton(closeDungeonBtn);
                    MelonLogger.Msg("Подземелья закрыты");
                    _waitTimer = Time.time + 0.2f;
                    _currentMissionIndex = 0;
                    _state = State.WaitAfterDungeonClose;
                    break;
                    
                case State.WaitAfterDungeonClose:
                    if (Time.time >= _waitTimer)
                        _state = State.OpenLiberationMissions;
                    break;
                    
                case State.OpenLiberationMissions:
                    var liberationOpenBtn = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/popups/WFDailyMissions/bg/liberationMissions/openButton");
                    if (GameUtils.ClickButton(liberationOpenBtn))
                    {
                        MelonLogger.Msg("Открыты миссии освобождения");
                        _waitTimer = Time.time + 0.2f;
                        _state = State.WaitForLiberationWindow;
                    }
                    else
                    {
                        _state = State.CloseWindow;
                    }
                    break;
                    
                case State.WaitForLiberationWindow:
                    if (Time.time >= _waitTimer)
                        _state = State.ProcessLiberationFights;
                    break;
                    
                case State.ProcessLiberationFights:
                    if (!ProcessNextLiberationFight())
                    {
                        _state = State.CloseWindow;
                    }
                    break;
                    
                case State.CloseWindow:
                    GameUtils.CloseWindow("menus/WorldMap");
                    _state = State.Complete;
                    break;
                    
                case State.Complete:
                    BotMain.NextFunction();
                    _state = State.OpenWarfront;
                    _waitTimer = 0f;
                    _currentMissionIndex = 0;
                    break;
            }
        }
        
        private static bool ProcessNextDungeonFight()
        {
            var allObjects = UnityEngine.Object.FindObjectsOfType<UnityEngine.UI.Button>();
            var fightButtons = new System.Collections.Generic.List<UnityEngine.UI.Button>();
            
            foreach (var button in allObjects)
            {
                if (button.gameObject.activeInHierarchy && 
                    button.name == "fightButton" && 
                    button.interactable &&
                    GetObjectPath(button.gameObject).Contains("WFDungeonMissions"))
                {
                    fightButtons.Add(button);
                }
            }
            
            MelonLogger.Msg($"[DEBUG] Найдено активных кнопок боев в подземельях: {fightButtons.Count}");
            
            if (fightButtons.Count > 0)
            {
                var fightButton = fightButtons[0];
                MelonLogger.Msg($"[DEBUG] Нажимаем кнопку боя: {GetObjectPath(fightButton.gameObject)}");
                
                if (GameUtils.ClickButton(fightButton.gameObject))
                {
                    MelonLogger.Msg("Запущен бой в подземелье");
                    _state = State.WaitForBattleResult;
                    return true;
                }
            }
            
            MelonLogger.Msg("[DEBUG] Активных боев в подземельях не найдено");
            return false;
        }
        
        private static bool ProcessNextLiberationFight()
        {
            var allObjects = UnityEngine.Object.FindObjectsOfType<UnityEngine.UI.Button>();
            var fightButtons = new System.Collections.Generic.List<UnityEngine.UI.Button>();
            
            foreach (var button in allObjects)
            {
                if (button.gameObject.activeInHierarchy && 
                    button.name == "fightButton" && 
                    button.interactable &&
                    GetObjectPath(button.gameObject).Contains("WFLiberationMissions"))
                {
                    fightButtons.Add(button);
                }
            }
            
            MelonLogger.Msg($"[DEBUG] Найдено активных кнопок боев освобождения: {fightButtons.Count}");
            
            if (fightButtons.Count > 0)
            {
                var fightButton = fightButtons[0];
                MelonLogger.Msg($"[DEBUG] Нажимаем кнопку боя: {GetObjectPath(fightButton.gameObject)}");
                
                if (GameUtils.ClickButton(fightButton.gameObject))
                {
                    MelonLogger.Msg("Запущен бой освобождения");
                    _state = State.WaitForBattleResult;
                    return true;
                }
            }
            
            MelonLogger.Msg("[DEBUG] Активных боев освобождения не найдено");
            return false;
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
    }
}