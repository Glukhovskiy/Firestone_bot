using UnityEngine;

namespace Firestone_bot
{
    public static class Alchemy
    {
        private enum State { CheckNotification, WaitForWindow, ProcessExperiments, WaitAfterClaim, StartExperiment, CloseWindow, Complete }
        private static State _state = State.CheckNotification;
        private static float _waitTimer = 0f;
        
        public static void ProcessAlchemy()
        {
            switch (_state)
            {
                case State.CheckNotification:
                    var experimentsBtn = GameUtils.FindByPath("battleRoot/battleMain/battleCanvas/SafeArea/leftSideUI/notifications/Viewport/grid/Experiments");
                    if (GameUtils.ClickButton(experimentsBtn))
                    {
                        _waitTimer = Time.time + 0.5f;
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
                        _state = State.ProcessExperiments;
                    break;
                    
                case State.ProcessExperiments:
                    var claimBtn = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/Alchemist/submenus/bg/experimentsSubmenu/experiments/alchExperimentSlot0/claimButton");
                    if (GameUtils.ClickButton(claimBtn))
                    {
                        MelonLoader.MelonLogger.Msg("Награда за эксперимент собрана");
                        _waitTimer = Time.time + 0.2f;
                        _state = State.WaitAfterClaim;
                    }
                    else
                    {
                        _state = State.StartExperiment;
                    }
                    break;
                    
                case State.WaitAfterClaim:
                    if (Time.time >= _waitTimer)
                    {
                        _state = State.StartExperiment;
                    }
                    break;
                    
                case State.StartExperiment:
                    var startBtn = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/Alchemist/submenus/bg/experimentsSubmenu/experiments/alchExperimentType0/startExperiment");
                    if (GameUtils.ClickButton(startBtn))
                    {
                        MelonLoader.MelonLogger.Msg("Новый эксперимент запущен");
                    }
                    _state = State.CloseWindow;
                    break;
                    
                case State.CloseWindow:
                    var closeBtn = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/Alchemist/closeButton");
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