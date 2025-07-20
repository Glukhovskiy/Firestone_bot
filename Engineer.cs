using UnityEngine;

namespace FirestoneBot
{
    public static class Engineer
    {
        private enum State { CheckNotification, WaitForWindow, ClaimTools, WaitBeforeClose, CloseWindow, Complete }
        private static State _state = State.CheckNotification;
        private static float _waitTimer = 0f;
        
        public static void ProcessEngineer()
        {
            
            switch (_state)
            {
                case State.CheckNotification:
                    var engineerNotification = GameUtils.FindByPath("battleRoot/battleMain/battleCanvas/SafeArea/leftSideUI/notifications/Viewport/grid/Engineer");
                    if (GameUtils.ClickButton(engineerNotification))
                    {
                        _waitTimer = Time.time + 0.2f;
                        _state = State.WaitForWindow;
                    }
                    else
                    {
                        BotMain.NextFunction();
                        _state = State.CheckNotification;
                    }
                    break;
                    
                case State.WaitForWindow:
                    if (Time.time >= _waitTimer)
                        _state = State.ClaimTools;
                    break;
                    
                case State.ClaimTools:
                    var claimToolsBtn = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/Engineer/submenus/bg/engineerSubmenu/toolsProductionSection/claimToolsButton");
                    GameUtils.ClickButton(claimToolsBtn);
                    _waitTimer = Time.time + 0.2f;
                    _state = State.WaitBeforeClose;
                    break;
                    
                case State.WaitBeforeClose:
                    if (Time.time >= _waitTimer)
                        _state = State.CloseWindow;
                    break;
                    
                case State.CloseWindow:
                    GameUtils.CloseWindow("menus/Engineer");
                    _state = State.Complete;
                    break;
                    
                case State.Complete:
                    BotMain.NextFunction();
                    _state = State.CheckNotification;
                    _waitTimer = 0f;
                    break;
            }
        }
    }
}