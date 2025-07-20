using MelonLoader;
using UnityEngine;

namespace FirestoneBot
{
    public static class MeteoriteResearch
    {
        private enum State { OpenLibrary, ProcessResearch, WaitForResearchWindow, Complete }
        private static State _state = State.OpenLibrary;
        private static float _waitTimer = 0f;
        private static int _currentResearchIndex = 0;
        
        public static void ProcessMeteoriteResearch()
        {
            switch (_state)
            {
                case State.OpenLibrary:
                    var meteoriteButton = GameUtils.FindByPath("battleRoot/battleMain/battleCanvas/SafeArea/leftSideUI/notifications/Viewport/grid/MeteoriteResearch");
                    if (GameUtils.ClickButton(meteoriteButton))
                    {
                        _waitTimer = Time.time + 1f;
                        _state = State.ProcessResearch;
                        MelonLogger.Msg("Библиотека метеоритов открыта");
                    }
                    else
                    {
                        BotMain.NextFunction();
                        ResetState();
                    }
                    break;
                    
                case State.ProcessResearch:
                    if (Time.time >= _waitTimer)
                    {
                        if (!ProcessNextResearch())
                        {
                            _state = State.Complete;
                        }
                    }
                    break;
                    
                case State.WaitForResearchWindow:
                    if (Time.time >= _waitTimer)
                    {
                        var buyButton = GameUtils.FindButton("researchButton");
                        if (GameUtils.ClickButton(buyButton))
                        {
                            MelonLogger.Msg("Исследование куплено");
                        }
                        
                        GameUtils.CloseWindow("popups/ResearchPreview");
                        _currentResearchIndex++;
                        _state = State.ProcessResearch;
                    }
                    break;
                    
                case State.Complete:
                    GameUtils.CloseWindow("Library");
                    BotMain.NextFunction();
                    ResetState();
                    break;
            }
        }
        
        private static bool ProcessNextResearch()
        {
            var researchButton = GameUtils.FindByPath($"menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/Library/submenus/meteoriteResearch/tree/research ({_currentResearchIndex})");
            
            if (researchButton != null && GameUtils.ClickButton(researchButton))
            {
                MelonLogger.Msg($"Открыто исследование {_currentResearchIndex}");
                _waitTimer = Time.time + 0.1f;
                _state = State.WaitForResearchWindow;
                return true;
            }
            
            return false;
        }

        
        private static void ResetState()
        {
            _state = State.OpenLibrary;
            _waitTimer = 0f;
            _currentResearchIndex = 0;
        }
    }
}