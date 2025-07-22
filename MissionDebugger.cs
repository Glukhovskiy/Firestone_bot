using MelonLoader;
using UnityEngine;
using System.IO;
using System;

namespace FirestoneBot
{
    public static class MissionDebugger
    {
        private static readonly string _logPath = Path.Combine(Application.dataPath, "..", "Mods", "missions.log");
        private static System.Collections.Generic.HashSet<string> _loggedMissions = new System.Collections.Generic.HashSet<string>();

        public static void LogAllMissions()
        {
            try
            {
                var allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (var obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && obj.name == "missionBase")
                    {
                        var path = GetObjectPath(obj);
                        
                        if (path.Contains("menusRoot/mapRoot/mapElements/missions"))
                        {
                            if (!_loggedMissions.Contains(path))
                            {
                                LogMissionInfo(obj);
                                _loggedMissions.Add(path);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Ошибка в LogAllMissions: {ex.Message}");
            }
        }

        private static void LogMissionInfo(GameObject missionBase)
        {
            try
            {
                var image = missionBase.GetComponent<UnityEngine.UI.Image>();
                var spriteName = image?.sprite?.name ?? "no_sprite";
                var textureName = image?.sprite?.texture?.name ?? "no_texture";
                var path = GetObjectPath(missionBase);
                
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Миссия: sprite={spriteName}, texture={textureName}, path={path}";
                
                // Логируем в консоль
                DebugManager.DebugLog($"[MissionDebugger] {logEntry}");
                
                // Записываем в файл
                WriteToLogFile(logEntry);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Ошибка логирования миссии: {ex.Message}");
            }
        }

        private static void WriteToLogFile(string logEntry)
        {
            try
            {
                File.AppendAllText(_logPath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Ошибка записи в missions.log: {ex.Message}");
            }
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

        public static void ClearLoggedMissions()
        {
            _loggedMissions.Clear();
        }
    }
}