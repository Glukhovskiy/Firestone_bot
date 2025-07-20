using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;
using System.IO;
using System.Linq;

[assembly: MelonInfo(typeof(FirestoneBot.BotMain), "Firestone Bot", "0.12.3", "GenleDevil")]
[assembly: MelonGame("HolydayStudios", "Firestone")]

namespace FirestoneBot
{
    public class BotMain : MelonMod
    {
        private static BotController _botController;
        private static BotConfig _config;
        private static string _configPath = "Mods/config.json";
        private static BotFunction _currentFunction = BotFunction.GuildExpeditions;
        
        private static readonly System.Collections.Generic.Dictionary<BotFunction, (System.Func<bool> IsEnabled, System.Action Process)> _modules = new()
        {
            { BotFunction.GuildExpeditions, (() => _config.guildExpeditions, GuildExpeditions.ProcessExpeditions) },
            { BotFunction.MapMissions, (() => _config.mapMissions, MapMissions.ProcessMapMissions) },
            { BotFunction.DailyTasks, (() => _config.dailyTasks, DailyTasks.ProcessDailyTasks) },
            { BotFunction.MeteoriteResearch, (() => _config.meteoriteResearch, MeteoriteResearch.ProcessMeteoriteResearch) },
            { BotFunction.FirestoneResearch, (() => _config.firestoneResearch, FirestoneResearch.ProcessFirestoneResearch) },
            { BotFunction.MysteryBox, (() => _config.mysteryBox, MysteryBox.ProcessMysteryBox) },
            { BotFunction.CheckIn, (() => _config.checkIn, CheckIn.ProcessCheckIn) },
            { BotFunction.OracleRituals, (() => _config.oracleRituals, OracleRituals.ProcessOracleRituals) },
            { BotFunction.OraclesGift, (() => _config.oraclesGift, OraclesGift.ProcessOraclesGift) },
            { BotFunction.OracleBlessings, (() => _config.oracleBlessings, OracleBlessings.ProcessOracleBlessings) },
            { BotFunction.Upgrades, (() => _config.upgrades, Upgrades.ProcessUpgrades) },
            { BotFunction.Chests, (() => _config.chests, Chests.ProcessChests) },
            { BotFunction.Tanks, (() => _config.tanks, Tanks.ProcessTanks) },
            { BotFunction.Alchemy, (() => _config.alchemy, Alchemy.ProcessAlchemy) },
            { BotFunction.Engineer, (() => _config.engineer, Engineer.ProcessEngineer) },
            { BotFunction.CloseWindows, (() => _config.closeWindows, CloseWindows.CloseAllWindows) }
        };

        [System.Obsolete]
        public override void OnApplicationStart()
        {
            ClassInjector.RegisterTypeInIl2Cpp<BotController>();
            var botGO = new GameObject("BotController");
            _botController = botGO.AddComponent<BotController>();
            LoadConfig();
            MelonLogger.Msg("Бот инициализирован!\nF5 - Включить/Выключить бота\nF7 - Включить/Выключить модуль Chests\nF8 - Включить/Выключить модуль Debug");
        }

        public override void OnUpdate()
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.F5))
                {
                    _config.enabled = !_config.enabled;
                    MelonLogger.Msg(_config.enabled ? "Бот включен" : "Бот выключен");
                }
                
                if (Input.GetKeyDown(KeyCode.F7))
                {
                    _config.chests = !_config.chests;
                    MelonLogger.Msg(_config.chests ? "Модуль Chests включен" : "Модуль Chests выключен");
                }

                if (_config.debugEnabled)
                    DebugManager.CheckDebugToggle();
                
                if (!_config.enabled) return;
                
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



        static void LoadConfig()
        {
            try
            {
                _config = new BotConfig();
                if (!File.Exists(_configPath))
                {
                    SaveConfig();
                    return;
                }
                
                var lines = File.ReadAllLines(_configPath);
                var properties = typeof(BotConfig).GetProperties();
                
                foreach (var line in lines)
                {
                    var parts = line.Split('=');
                    if (parts.Length != 2) continue;
                    
                    var prop = properties.FirstOrDefault(p => p.Name.Equals(parts[0], System.StringComparison.OrdinalIgnoreCase));
                    if (prop != null)
                    {
                        var value = prop.PropertyType == typeof(bool) ? parts[1] == "true" : (object)parts[1];
                        prop.SetValue(_config, value);
                    }
                }
            }
            catch
            {
                _config = new BotConfig();
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

        static void SaveConfig()
        {
            try
            {
                var properties = typeof(BotConfig).GetProperties();
                var lines = properties.Select(prop => 
                {
                    var value = prop.GetValue(_config);
                    return $"{prop.Name}={value?.ToString().ToLower()}";
                }).ToArray();
                
                File.WriteAllLines(_configPath, lines);
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка сохранения конфига: {ex.Message}");
            }
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
                    MelonLogger.Msg("Нажата кнопка collectButton");
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
            catch
            {
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
    
    public class BotConfig
    {
        public bool enabled { get; set; } = true;
        public string logLevel { get; set; } = "info";
        public bool debugEnabled { get; set; } = true;
        public bool guildExpeditions { get; set; } = true;
        public bool mapMissions { get; set; } = true;
        public bool dailyTasks { get; set; } = true;
        public bool meteoriteResearch { get; set; } = true;
        public bool firestoneResearch { get; set; } = true;
        public bool mysteryBox { get; set; } = true;
        public bool checkIn { get; set; } = true;
        public bool oracleRituals { get; set; } = true;
        public bool oraclesGift { get; set; } = true;
        public bool oracleBlessings { get; set; } = true;
        public bool upgrades { get; set; } = true;
        public bool chests { get; set; } = true;
        public bool tanks { get; set; } = false;
        public bool alchemy { get; set; } = true;
        public bool engineer { get; set; } = true;
        public bool closeWindows { get; set; } = false;
    }

    public class BotController : MonoBehaviour
    {
        void Start()
        {
            UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
            MelonLogger.Msg("BotController запущен");
        }
    }
}