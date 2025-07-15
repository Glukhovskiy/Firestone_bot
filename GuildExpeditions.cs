using MelonLoader;
using UnityEngine;

namespace FirestoneBot
{
    public static class GuildExpeditions
    {
        private enum State { FindButton, ProcessWindow, Complete }
        private static State _state = State.FindButton;
        
        public static void ProcessExpeditions()
        {
            switch (_state)
            {
                case State.FindButton:
                    var expeditionBtn = GameUtils.FindButton("Expeditions");
                    if (expeditionBtn != null)
                    {
                        GameUtils.ClickButton(expeditionBtn);
                        _state = State.ProcessWindow;
                    }
                    else
                    {
                        BotMain.NextFunction();
                        _state = State.FindButton;
                    }
                    break;
                    
                case State.ProcessWindow:
                    if (IsExpeditionWindowOpen())
                    {
                        ProcessExpeditionWindow();
                        _state = State.Complete;
                    }
                    break;
                    
                case State.Complete:
                    BotMain.NextFunction();
                    _state = State.FindButton;
                    break;
            }
        }
        
        private static void ProcessExpeditionWindow()
        {
            var claimBtn = GameUtils.FindButton("claimButton");
            if (claimBtn != null)
            {
                GameUtils.ClickButton(claimBtn);
            }
            else
            {
                var startBtn = GameUtils.FindButton("startButton");
                GameUtils.ClickButton(startBtn);
            }
            
            GameUtils.CloseWindow("popups/Expeditions");
        }
        
        private static bool IsExpeditionWindowOpen()
        {
            return GameUtils.FindByPath("popups/Expeditions") != null;
        }
        

        

        

        

        


    }
}