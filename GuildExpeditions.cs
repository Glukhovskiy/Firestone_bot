using MelonLoader;
using UnityEngine;

namespace FirestoneBot
{
    public static class GuildExpeditions
    {
        private enum State { FindButton, ProcessWindow, WaitAfterClaim, StartExpedition, WaitBeforeClose, Complete }
        private static State _state = State.FindButton;
        private static float _waitTimer = 0f;
        
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
                        var claimBtn = GameUtils.FindButton("claimButton");
                        if (claimBtn != null)
                        {
                            GameUtils.ClickButton(claimBtn);
                            MelonLogger.Msg("Награда за экспедицию собрана");
                            _waitTimer = Time.time + 0.1f;
                            _state = State.WaitAfterClaim;
                        }
                        else
                        {
                            _state = State.StartExpedition;
                        }
                    }
                    break;
                    
                case State.WaitAfterClaim:
                    if (Time.time >= _waitTimer)
                    {
                        _state = State.StartExpedition;
                    }
                    break;
                    
                case State.StartExpedition:
                    var startBtn = GameUtils.FindButton("startButton");
                    if (GameUtils.ClickButton(startBtn))
                    {
                        MelonLogger.Msg("Новая экспедиция запущена");
                        _waitTimer = Time.time + 0.2f;
                        _state = State.WaitBeforeClose;
                    }
                    else
                    {
                        _state = State.WaitBeforeClose;
                    }
                    break;
                    
                case State.WaitBeforeClose:
                    if (Time.time >= _waitTimer)
                    {
                        GameUtils.CloseWindow("popups/Expeditions/bg");
                        _state = State.Complete;
                    }
                    break;
                    
                case State.Complete:
                    BotMain.NextFunction();
                    _state = State.FindButton;
                    _waitTimer = 0f;
                    break;
            }
        }
        

        
        private static bool IsExpeditionWindowOpen()
        {
            return GameUtils.FindByPath("popups/Expeditions") != null;
        }
        

        

        

        

        


    }
}