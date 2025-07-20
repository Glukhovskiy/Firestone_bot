using UnityEngine;
using MelonLoader;

namespace FirestoneBot
{
    public static class CloseWindows
    {
        private static float _lastRunTime = 0f;
        private const float RUN_INTERVAL = 600f; // 10 минут в секундах
        
        public static void CloseAllWindows()
        {
            if (Time.time - _lastRunTime < RUN_INTERVAL)
            {
                BotMain.NextFunction();
                return;
            }
            
            var allObjects = UnityEngine.Object.FindObjectsOfType<UnityEngine.UI.Button>();
            var closeButtons = new System.Collections.Generic.List<UnityEngine.UI.Button>();
            
            foreach (var button in allObjects)
            {
                if (button.gameObject.activeInHierarchy && 
                    button.name == "closeButton" && 
                    button.interactable)
                {
                    closeButtons.Add(button);
                }
            }
            
            MelonLogger.Msg($"Найдено активных кнопок закрытия: {closeButtons.Count}");
            
            foreach (var closeButton in closeButtons)
            {
                GameUtils.ClickButton(closeButton.gameObject);
                MelonLogger.Msg($"Закрыто окно: {GetObjectPath(closeButton.gameObject)}");
            }
            
            _lastRunTime = Time.time;
            BotMain.NextFunction();
        }
        
        private static string GetObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform current = obj.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }
    }
}