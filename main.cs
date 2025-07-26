using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;

[assembly: MelonInfo(typeof(Firestone_bot.BotMain), "Firestone Bot", "0.13.1", "GentleDevil")]
[assembly: MelonGame("HolydayStudios", "Firestone")]

namespace Firestone_bot
{
    public class BotMain : MelonMod
    {
        private static BotController _botController;
        private static BotFunction _currentFunction = BotFunction.GuildExpeditions;
        
        private static readonly System.Collections.Generic.Dictionary<BotFunction, (System.Func<bool> IsEnabled, System.Action Process)> _modules = new()
        {
            { BotFunction.GuildExpeditions, (() => ConfigManager.Config.Bot.GuildExpeditions, GuildExpeditions.ProcessExpeditions) },
            { BotFunction.MapMissions, (() => ConfigManager.Config.Bot.MapMissions, MapMissions.ProcessMapMissions) },
            { BotFunction.DailyTasks, (() => ConfigManager.Config.Bot.DailyTasks, DailyTasks.ProcessDailyTasks) },
            { BotFunction.MeteoriteResearch, (() => ConfigManager.Config.Bot.MeteoriteResearch, MeteoriteResearch.ProcessMeteoriteResearch) },
            { BotFunction.FirestoneResearch, (() => ConfigManager.Config.Bot.FirestoneResearch, FirestoneResearch.ProcessFirestoneResearch) },
            { BotFunction.MysteryBox, (() => ConfigManager.Config.Bot.MysteryBox, MysteryBox.ProcessMysteryBox) },
            { BotFunction.CheckIn, (() => ConfigManager.Config.Bot.CheckIn, CheckIn.ProcessCheckIn) },
            { BotFunction.OracleRituals, (() => ConfigManager.Config.Bot.OracleRituals, OracleRituals.ProcessOracleRituals) },
            { BotFunction.OraclesGift, (() => ConfigManager.Config.Bot.OraclesGift, OraclesGift.ProcessOraclesGift) },
            { BotFunction.OracleBlessings, (() => ConfigManager.Config.Bot.OracleBlessings, OracleBlessings.ProcessOracleBlessings) },
            { BotFunction.Upgrades, (() => ConfigManager.Config.Bot.Upgrades, Upgrades.ProcessUpgrades) },
            { BotFunction.Chests, (() => ConfigManager.Config.Bot.Chests, Chests.ProcessChests) },
            { BotFunction.Tanks, (() => ConfigManager.Config.Bot.Tanks, Tanks.ProcessTanks) },
            { BotFunction.Alchemy, (() => ConfigManager.Config.Bot.Alchemy, Alchemy.ProcessAlchemy) },
            { BotFunction.Engineer, (() => ConfigManager.Config.Bot.Engineer, Engineer.ProcessEngineer) },
            { BotFunction.CloseWindows, (() => ConfigManager.Config.Bot.CloseWindows, CloseWindows.CloseAllWindows) }
        };

        [System.Obsolete("Переопределяет устаревший метод MelonBase.OnApplicationStart()")]
        public override void OnApplicationStart()
        {
            ClassInjector.RegisterTypeInIl2Cpp<BotController>();
            var botGO = new GameObject("BotController");
            _botController = botGO.AddComponent<BotController>();
            LogModulesStatus();
            MelonLogger.Msg("Бот инициализирован!\nF5 - Включить/Выключить бота\nF7 - Включить/Выключить модуль Chests\nF8 - Включить/Выключить модуль Debug");
        }

        public override void OnUpdate()
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.F5))
                {
                    ConfigManager.Config.Bot.Enabled = !ConfigManager.Config.Bot.Enabled;
                    MelonLogger.Msg(ConfigManager.Config.Bot.Enabled ? "Бот включен" : "Бот выключен");
                }
                
                if (Input.GetKeyDown(KeyCode.F7))
                {
                    ConfigManager.Config.Bot.Chests = !ConfigManager.Config.Bot.Chests;
                    MelonLogger.Msg(ConfigManager.Config.Bot.Chests ? "Модуль Chests включен" : "Модуль Chests выключен");
                }

                if (ConfigManager.Config.Bot.DebugEnabled)
                    DebugManager.CheckDebugToggle();
                
                if (!ConfigManager.Config.Bot.Enabled) return;
                
                // Проверяем, что игра готова
                if (!IsGameReady()) return;

                if (_modules.TryGetValue(_currentFunction, out var module))
                {
                    if (module.IsEnabled())
                        module.Process();
                    else
                        NextFunction();
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка в OnUpdate: {ex.Message}");
            }
        }





        public static void NextFunction()
        {
            _currentFunction = _currentFunction switch
            {
                BotFunction.GuildExpeditions => BotFunction.MapMissions,
                BotFunction.MapMissions => BotFunction.DailyTasks,
                BotFunction.DailyTasks => BotFunction.MeteoriteResearch,
                BotFunction.MeteoriteResearch => BotFunction.FirestoneResearch,
                BotFunction.FirestoneResearch => BotFunction.MysteryBox,
                BotFunction.MysteryBox => BotFunction.CheckIn,
                BotFunction.CheckIn => BotFunction.OracleRituals,
                BotFunction.OracleRituals => BotFunction.OraclesGift,
                BotFunction.OraclesGift => BotFunction.OracleBlessings,
                BotFunction.OracleBlessings => BotFunction.Upgrades,
                BotFunction.Upgrades => BotFunction.Chests,
                BotFunction.Chests => BotFunction.Tanks,
                BotFunction.Tanks => BotFunction.Alchemy,
                BotFunction.Alchemy => BotFunction.Engineer,
                BotFunction.Engineer => BotFunction.CloseWindows,
                BotFunction.CloseWindows => BotFunction.GuildExpeditions,
                _ => BotFunction.GuildExpeditions
            };
        }

        static void LogModulesStatus()
        {
            var enabled = new System.Collections.Generic.List<string>();
            var disabled = new System.Collections.Generic.List<string>();
            
            foreach (var module in _modules)
            {
                if (module.Value.IsEnabled())
                    enabled.Add(module.Key.ToString());
                else
                    disabled.Add(module.Key.ToString());
            }
            
            MelonLogger.Msg($"Включено ({enabled.Count}): {string.Join("\n ", enabled)}");
            if (disabled.Count > 0)
                MelonLogger.Msg($"Отключено ({disabled.Count}): {string.Join("\n ", disabled)}");
        }



        private static bool _collectButtonSeen = false;
        private static float _collectButtonTime = 0f;

        private static bool IsGameReady()
        {
            try
            {
                var collectButton = GameObject.Find("collectButton");
                
                if (collectButton != null)
                {
                    _collectButtonSeen = true;
                    GameUtils.ClickButton(collectButton);
                    _collectButtonTime = Time.time + 0.2f;
                    return false;
                }
                
                // Обработка событий после задержки
                if (_collectButtonTime > 0f && Time.time >= _collectButtonTime)
                {
                    var eventsContainer = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/events");
                    
                    if (eventsContainer != null && eventsContainer.transform.childCount > 0)
                    {
                        MelonLogger.Msg($"Обнаружено событий: {eventsContainer.transform.childCount}");
                        
                        for (int i = 0; i < eventsContainer.transform.childCount; i++)
                        {
                            var child = eventsContainer.transform.GetChild(i);
                            
                            if (child.gameObject.activeInHierarchy)
                            {
                                var closeBtn = child.Find("bg/closeButton")?.gameObject;
                                
                                if (GameUtils.ClickButton(closeBtn))
                                {
                                    MelonLogger.Msg($"Закрыто событие: {child.name}");
                                    break;
                                }
                            }
                        }
                    }
                    
                    _collectButtonTime = 0f;
                    return false;
                }
                
                // Игра готова к работе бота
                return _collectButtonSeen && _collectButtonTime == 0f;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка проверки готовности игры: {ex.Message}");
                return false;
            }
        }
        

    }

    public enum BotFunction
    {
        GuildExpeditions,
        MapMissions,
        DailyTasks,
        MeteoriteResearch,
        FirestoneResearch,
        MysteryBox,
        CheckIn,
        OracleRituals,
        OraclesGift,
        OracleBlessings,
        Upgrades,
        Chests,
        Tanks,
        Alchemy,
        Engineer,
        CloseWindows
    }
    


    public class BotController : MonoBehaviour
    {
        void Start()
        {
            UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
            MelonLogger.Msg("BotController запущен");
        }
    }

    public class UnifiedConfig
    {
        public BotSettings Bot { get; set; } = new();
        public Dictionary<string, int> FirestoneResearch { get; set; } = new();
        public Dictionary<string, int> OracleBlessings { get; set; } = new();
        public Dictionary<string, int> MissionPriorities { get; set; } = new();
        public Dictionary<string, string> ResearchNames { get; set; } = new();
        public Dictionary<string, string> BlessingNames { get; set; } = new();
    }

    public class BotSettings
    {
        public bool Enabled { get; set; } = true;
        public bool DebugEnabled { get; set; } = true;
        public bool GuildExpeditions { get; set; } = true;
        public bool MapMissions { get; set; } = true;
        public bool DailyTasks { get; set; } = true;
        public bool MeteoriteResearch { get; set; } = true;
        public bool FirestoneResearch { get; set; } = true;
        public bool MysteryBox { get; set; } = true;
        public bool CheckIn { get; set; } = true;
        public bool OracleRituals { get; set; } = true;
        public bool OraclesGift { get; set; } = true;
        public bool OracleBlessings { get; set; } = true;
        public bool Upgrades { get; set; } = true;
        public bool Chests { get; set; } = true;
        public bool Tanks { get; set; } = true;
        public bool Alchemy { get; set; } = true;
        public bool Engineer { get; set; } = true;
        public bool CloseWindows { get; set; } = true;
    }

    public static class ConfigManager
    {
        private static UnifiedConfig _config;
        private static readonly string _configPath = "Mods/unified_config.json";

        public static UnifiedConfig Config => _config ??= LoadConfig();

        private static UnifiedConfig LoadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    return JsonSerializer.Deserialize<UnifiedConfig>(json);
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка загрузки конфига: {ex.Message}");
            }

            var defaultConfig = CreateDefaultConfig();
            SaveConfig(defaultConfig);
            return defaultConfig;
        }

        public static void SaveConfig(UnifiedConfig config = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(config ?? _config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка сохранения конфига: {ex.Message}");
            }
        }

        private static UnifiedConfig CreateDefaultConfig()
        {
            return new UnifiedConfig
            {
                FirestoneResearch = new Dictionary<string, int>
                {
                    { "1/12", 1 }, { "2/2", 1 }, { "1/6", 2 }, { "2/5", 2 }, { "3/14", 2 },
                    { "2/1", 3 }, { "3/15", 4 }, { "1/0", 7 }, { "2/7", 8 }, { "3/10", 9 },
                    { "3/6", 10 }, { "2/13", 11 }, { "1/3", 12 }, { "2/14", 12 }, { "1/4", 13 },
                    { "2/15", 13 }, { "2/9", 14 }, { "3/0", 15 }, { "1/5", 15 }, { "3/2", 16 },
                    { "3/3", 17 }, { "3/4", 18 }, { "1/7", 18 }, { "1/8", 19 }, { "3/5", 19 },
                    { "1/9", 20 }, { "3/7", 20 }, { "1/10", 21 }, { "3/8", 21 }, { "1/11", 22 },
                    { "3/9", 22 }, { "3/11", 23 }, { "1/13", 24 }, { "1/14", 25 }, { "1/15", 26 },
                    { "2/0", 27 }, { "2/3", 29 }, { "2/4", 30 }, { "2/6", 31 }, { "2/8", 32 },
                    { "2/10", 33 }, { "3/1", 34 }, { "1/1", 997 }, { "1/2", 997 }, { "2/11", 997 },
                    { "2/12", 997 }, { "3/12", 997 }, { "3/13", 997 }
                },
                OracleBlessings = new Dictionary<string, int>
                {
                    { "11", 1 }, { "0", 2 }, { "6", 3 }, { "8", 4 },
                    { "3", 5 }, { "2", 6 }, { "7", 7 }, { "1", 8 }
                },
                MissionPriorities = new Dictionary<string, int>
                {
                    { "mysterybox", 0 }, { "scout", 1 }, { "adventure", 2 }, 
                    { "war", 3 }, { "dragon", 4 }, { "monster", 5 }, { "naval", 6 }
                }
            };
        }
    }
}