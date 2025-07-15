using UnityEngine;

namespace FirestoneBot
{
    public static class Engineer
    {
        private enum State { CheckNotification, ClaimTools, CloseWindow, Complete }
        private static State _state = State.CheckNotification;
        
        public static void ProcessEngineer()
        {
            
            switch (_state)
            {
                case State.CheckNotification:
                    var engineerNotification = GameUtils.FindByPath("battleRoot/battleMain/battleCanvas/SafeArea/leftSideUI/notifications/Viewport/grid/Engineer");
                    if (GameUtils.ClickButton(engineerNotification))
                        _state = State.ClaimTools;
                    else
                    {
                        BotMain.NextFunction();
                        _state = State.CheckNotification;
                    }
                    break;
                    
                case State.ClaimTools:
                    var claimToolsBtn = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/Engineer/submenus/bg/engineerSubmenu/toolsProductionSection/claimToolsButton");
                    if (GameUtils.ClickHandler(claimToolsBtn))
                        _state = State.CloseWindow;
                    else
                        _state = State.CloseWindow;
                    break;
                    
                case State.CloseWindow:
                    var closeBtn = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/Engineer/closeButton");
                    GameUtils.ClickButton(closeBtn);
                    _state = State.Complete;
                    break;
                    
                case State.Complete:
                    BotMain.NextFunction();
                    _state = State.CheckNotification;
                    break;
            }
        }
    }
}