using MelonLoader;
using UnityEngine;

namespace FirestoneBot
{
    public static class Tanks
    {
        private enum State { OpenWarfront, ProcessClaim, CloseWindow, Complete }
        private static State _state = State.OpenWarfront;
        private static float _waitTimer = 0f;
        
        public static void ProcessTanks()
        {
            switch (_state)
            {
                case State.OpenWarfront:
                    var warfrontButton = GameUtils.FindByPath("battleRoot/battleMain/battleCanvas/SafeArea/leftSideUI/notifications/Viewport/grid/WarfrontCampaign");
                    if (GameUtils.ClickButton(warfrontButton))
                    {
                        MelonLogger.Msg("Нажата кнопка WarfrontCampaign");
                        _waitTimer = Time.time + 0.5f;
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
                        _state = State.CloseWindow;
                    }
                    break;
                    
                case State.CloseWindow:
                    var closeButton = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/WorldMap/closeButton");
                    GameUtils.ClickButton(closeButton);
                    _state = State.Complete;
                    break;
                    
                case State.Complete:
                    BotMain.NextFunction();
                    _state = State.OpenWarfront;
                    _waitTimer = 0f;
                    break;
            }
        }
    }
}