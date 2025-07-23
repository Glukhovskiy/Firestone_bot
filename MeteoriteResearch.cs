using MelonLoader;
using UnityEngine;

namespace Firestone_bot
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
                        if (buyButton == null)
                        {
                            buyButton = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/popups/ResearchPreview/bg/researchButton");
                        }
                        
                        DebugManager.DebugLog($"Кнопка покупки найдена: {buyButton != null}");
                        
                        if (GameUtils.ClickButton(buyButton))
                        {
                            MelonLogger.Msg($"Исследование {_currentResearchIndex} куплено");
                        }
                        else
                        {
                            DebugManager.DebugLog($"Не удалось купить исследование {_currentResearchIndex}");
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
            // Ищем исследования в разных деревьях
            GameObject researchButton = null;
            for (int tree = 1; tree <= 10; tree++)
            {
                researchButton = GameUtils.FindByPath($"menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/Library/submenus/meteoriteResearch/submenus/tree{tree}/research{_currentResearchIndex}");
                if (researchButton != null)
                {
                    DebugManager.DebugLog($"Найдено исследование {_currentResearchIndex} в дереве {tree}");
                    break;
                }
            }
            
            if (researchButton != null && GameUtils.ClickButton(researchButton))
            {
                MelonLogger.Msg($"Открыто исследование {_currentResearchIndex}");
                _waitTimer = Time.time + 0.5f;
                _state = State.WaitForResearchWindow;
                return true;
            }
            
            DebugManager.DebugLog($"Исследование {_currentResearchIndex} не найдено или недоступно");
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