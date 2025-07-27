using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;

[assembly: MelonInfo(typeof(Firestone_bot.BotMain), "Firestone Bot", "0.14.0", "GentleDevil")]
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
            { BotFunction.Sales, (() => ConfigManager.Config.Bot.Sales, Sales.ProcessSales) },
            { BotFunction.CloseWindows, (() => ConfigManager.Config.Bot.CloseWindows, CloseWindows.CloseAllWindows) }
        };

        [System.Obsolete("Переопределяет устаревший метод MelonBase.OnApplicationStart()")]
        public override void OnApplicationStart()
        {
            ClassInjector.RegisterTypeInIl2Cpp<BotController>();
            var botGO = new GameObject("BotController");
            _botController = botGO.AddComponent<BotController>();
            LogModulesStatus();
            MelonLogger.Msg("Бот инициализирован!\nF4 - Перезагрузить конфиг\nF5 - Включить/Выключить бота\nF7 - Включить/Выключить модуль Chests\nF8 - Включить/Выключить модуль Debug");
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
                
                if (Input.GetKeyDown(KeyCode.F4))
                {
                    ConfigManager.ReloadConfig();
                    ResetAllModules();
                    MelonLogger.Msg("Конфигурация перезагружена, состояния модулей сброшены");
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
        
        private static void ResetAllModules()
        {
            // Сбрасываем текущую функцию к начальной
            _currentFunction = BotFunction.GuildExpeditions;
            
            // Здесь можно добавить сброс состояний конкретных модулей
            // Например: MapMissions.ResetState(), Sales.ResetState() и т.д.
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
                BotFunction.Engineer => BotFunction.Sales,
                BotFunction.Sales => BotFunction.CloseWindows,
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
        Sales,
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
        public Dictionary<string, bool> SalesItems { get; set; } = new();
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
        public bool Sales { get; set; } = false;
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
        
        public static void ReloadConfig()
        {
            _config = null; // Сбрасываем кеш
            _config = LoadConfig(); // Перезагружаем
        }

        private static UnifiedConfig CreateDefaultConfig()
        {
            return new UnifiedConfig
            {
                FirestoneResearch = new Dictionary<string, int>
                {
                    { "Prestigious", 1 }, { "Raining Gold", 2 }, { "All Main Attributes", 3 },
                    { "Attribute Damage", 7 }, { "Damage Specialization", 9 }, { "Precision", 10 },
                    { "Rage Heroes", 11 }, { "Fist Fight", 12 }, { "Mana Heroes", 12 },
                    { "Guardian Power", 13 }, { "Energy Heroes", 13 }, { "Projectiles", 15 },
                    { "Critical Loot Bonus", 18 }, { "Critical Loot Chance", 19 }, { "Magic Spells", 20 },
                    { "Weaklings", 20 }, { "Expose Weakness", 21 }, { "Tank Specialization", 21 },
                    { "Medal Of Honor", 22 }, { "Healer Specialization", 22 }, { "Trainer Skills", 24 },
                    { "Skip Wave", 25 }, { "Expeditioner", 26 }, { "Skip Stage", 27 },
                    { "Powerless Enemy", 29 }, { "Powerless Boss", 30 }, { "Meteorite Hunter", 31 },
                    { "Firestone Effect", 34 }, { "Attribute Health", 997 }, { "Attribute Armor", 997 }
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
                },
                SalesItems = new Dictionary<string, bool>
                {
                    { "MidasTouch", true }, { "PouchOfGold", false }, { "BucketOfGold", false },
                    { "CrateOfGold", false }, { "PileOfGold", false }, { "DrumsOfWar", true },
                    { "DragonArmor", true }, { "GuardiansRune", true }, { "TotemOfAgony", true },
                    { "TotemOfAnnihilation", true }
                }
            };
        }
    }
}