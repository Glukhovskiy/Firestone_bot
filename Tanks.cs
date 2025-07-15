using MelonLoader;
using UnityEngine;

namespace FirestoneBot
{
    public static class Tanks
    {
        public static void ProcessTanks()
        {
            try
            {
                GameObject warfrontButton = FindWarfrontButton();
                if (warfrontButton != null)
                {
                    var button = warfrontButton.GetComponent<UnityEngine.UI.Button>();
                    if (button != null && button.interactable)
                    {
                        button.onClick.Invoke();
                        MelonLogger.Msg("Нажата кнопка WarfrontCampaign");
                    }
                }
                
                BotMain.NextFunction();
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка в ProcessTanks: {ex.Message}");
            }
        }
        
        private static GameObject FindWarfrontButton()
        {
            return FindObjectByPath("battleRoot/battleMain/battleCanvas/SafeArea/leftSideUI/notifications/Viewport/grid/WarfrontCampaign");
        }
        
        private static GameObject FindObjectByPath(string targetPath)
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    string path = GetObjectPath(obj);
                    if (path == targetPath)
                    {
                        return obj;
                    }
                }
            }
            
            return null;
        }
        
        private static string GetObjectPath(GameObject obj)
        {
            Transform current = obj.transform;
            string path = obj.name;
            
            while (current.parent != null)
            {
                current = current.parent;
                path = current.name + "/" + path;
            }
            
            return path;
        }
    }
}