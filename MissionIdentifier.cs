using MelonLoader;
using UnityEngine;
using System.IO;
using System;

namespace FirestoneBot
{
    public static class MissionIdentifier
    {
        private static readonly string _logPath = Path.Combine(Application.dataPath, "..", "Mods", "mission_types.txt");

        public static void IdentifyMission(GameObject currentMission)
        {
            try
            {
                var previewWindow = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/popups/PreviewMission");
                if (previewWindow == null) return;

                // Получаем map и mission из пути currentMission
                var missionInfo = ExtractMissionInfo(currentMission, previewWindow.transform);
                
                WriteToLogFile(missionInfo);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Ошибка в IdentifyMission: {ex.Message}");
            }
        }

        private static string ExtractMissionInfo(GameObject currentMission, Transform previewWindow)
        {
            string location = "";
            string missionName = "";
            string missionType = "";
            
            // Получаем map и mission из пути currentMission
            if (currentMission != null)
            {
                var path = GetObjectPath(currentMission);
                var pathParts = path.Split('/');
                
                // Пример: menusRoot/mapRoot/mapElements/missions/Riverlands/LakesTerror/missionBase
                if (pathParts.Length >= 6)
                {
                    location = pathParts[4]; // Riverlands
                    missionName = pathParts[5]; // LakesTerror
                }
            }
            
            // Получаем тип миссии из иконки
            missionType = FindMissionTypeIcon(previewWindow);
            
            return $"{location}/{missionName}={missionType}";
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
        
        private static string FindMissionTypeIcon(Transform parent)
        {
            var typeImg = FindChildByName(parent, "previewMissionTypeImg");
            if (typeImg != null)
            {
                var image = typeImg.GetComponent<UnityEngine.UI.Image>();
                if (image?.sprite != null)
                {
                    return image.sprite.name.Replace("(Clone)", "").Replace("mission", "").Replace("Icon", "");
                }
            }
            return "Unknown";
        }
        
        private static Transform FindChildByName(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            
            for (int i = 0; i < parent.childCount; i++)
            {
                var result = FindChildByName(parent.GetChild(i), name);
                if (result != null) return result;
            }
            return null;
        }

        private static void WriteToLogFile(string logEntry)
        {
            try
            {
                if (File.Exists(_logPath))
                {
                    var existingLines = File.ReadAllLines(_logPath);
                    if (Array.Exists(existingLines, line => line == logEntry))
                        return;
                }
                File.AppendAllText(_logPath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Ошибка записи в mission_types.txt: {ex.Message}");
            }
        }
    }
}