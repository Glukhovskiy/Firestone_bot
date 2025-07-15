using UnityEngine;

namespace FirestoneBot
{
    public static class CloseWindows
    {
        public static void CloseAllWindows()
        {
            // Закрываем основные окна игры
            GameUtils.CloseWindow("menus/Inventory");
            GameUtils.CloseWindow("menus/Shop");
            GameUtils.CloseWindow("menus/Collection");
            GameUtils.CloseWindow("menus/Arena");
            GameUtils.CloseWindow("menus/Tavern");
            GameUtils.CloseWindow("menus/Adventures");
            GameUtils.CloseWindow("menus/Battlegrounds");
            GameUtils.CloseWindow("menus/Duels");
            
            // Закрываем попапы
            GameUtils.CloseWindow("popups/ChestOpenPreview");
            GameUtils.CloseWindow("popups/PackOpenPreview");
            GameUtils.CloseWindow("menus/ChestOpening");
            GameUtils.CloseWindow("menus/PackOpening");
            
            BotMain.NextFunction();
        }
    }
}