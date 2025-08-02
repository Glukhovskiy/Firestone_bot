using MelonLoader;
using UnityEngine;

namespace Firestone_bot
{
    public static class Guardian
    {
        private static float _lastCheckTime = 0f;
        private static float _waitForWindowTime = 0f;
        private static bool _waitingForWindow = false;
        private const float CHECK_INTERVAL = 300f; // 5 минут
        
        public static void ProcessGuardian()
        {
            if (_waitingForWindow)
            {
                if (Time.time >= _waitForWindowTime)
                {
                    CheckTrainButton();
                    _waitingForWindow = false;
                    return;
                }
            }
            else if (Time.time - _lastCheckTime >= CHECK_INTERVAL)
            {
                CheckGuardianTraining();
                _lastCheckTime = Time.time;
            }
            
            BotMain.NextFunction();
        }
        
        private static void CheckGuardianTraining()
        {
            var guardianBtn = GameUtils.FindByPath("battleRoot/battleMain/battleCanvas/SafeArea/leftSideUI/notifications/Viewport/grid/GuardianTraining");
            
            if (guardianBtn != null && GameUtils.ClickButton(guardianBtn))
            {
                MelonLogger.Msg("Найдена и нажата кнопка GuardianTraining");
                _waitForWindowTime = Time.time + 0.5f;
                _waitingForWindow = true;
            }
        }
        
        private static void CheckTrainButton()
        {
            // Сначала выбираем нужных стражей
            SelectGuardians();
            
            var trainBtn = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/MagicQuarters/submenus/bg/infoSubmenu/activities/unlocked/train/trainButton");
            
            if (trainBtn != null)
            {
                var button = trainBtn.GetComponent<UnityEngine.UI.Button>();
                if (button != null && button.interactable)
                {
                    if (GameUtils.ClickButton(trainBtn))
                    {
                        MelonLogger.Msg("Кнопка trainButton нажата - запущена тренировка стражей");
                    }
                }
            }
            
            if (ConfigManager.Config.GuardianSettings.Enlightenment)
            {
                CheckEnlightenmentButton();
            }
        }
        
        private static void SelectGuardians()
        {
            var settings = ConfigManager.Config.GuardianSettings;
            
            if (settings.Angel)
                SelectGuardian(0, "Angel");
            if (settings.Dragon)
                SelectGuardian(1, "Dragon");
            if (settings.Fenix)
                SelectGuardian(2, "Fenix");
            if (settings.Jinn)
                SelectGuardian(3, "Jinn");
        }
        
        private static void SelectGuardian(int index, string name)
        {
            var guardianBtn = GameUtils.FindByPath($"menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/MagicQuarters/guardianList/guardian ({index})");
            
            if (GameUtils.ClickButton(guardianBtn))
            {
                MelonLogger.Msg($"Выбран страж: {name}");
            }
        }
        
        private static void CheckEnlightenmentButton()
        {
            var enlightenmentBtn = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/MagicQuarters/submenus/bg/infoSubmenu/activities/unlocked/enlightenment/enlightenmentButton");
            
            if (enlightenmentBtn != null)
            {
                var button = enlightenmentBtn.GetComponent<UnityEngine.UI.Button>();
                int clickCount = 0;
                
                while (button != null && button.interactable)
                {
                    if (GameUtils.ClickButton(enlightenmentBtn))
                    {
                        clickCount++;
                        MelonLogger.Msg($"Кнопка enlightenmentButton нажата ({clickCount} раз)");
                    }
                    else
                    {
                        break;
                    }
                }
                
                if (clickCount > 0)
                {
                    MelonLogger.Msg($"Просвещение стража завершено. Всего нажатий: {clickCount}");
                }
            }
            
            CloseWindow();
        }
        
        private static void CloseWindow()
        {
            var closeBtn = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/MagicQuarters/closeButton");
            
            if (GameUtils.ClickButton(closeBtn))
            {
                MelonLogger.Msg("Окно MagicQuarters закрыто");
            }
            
            BotMain.NextFunction();
        }
    }
}