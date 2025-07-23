using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;
using System.IO;
using System.Linq;

[assembly: MelonInfo(typeof(Firestone_bot.BotMain), "Firestone Bot", "0.13.0", "GentleDevil")]
[assembly: MelonGame("HolydayStudios", "Firestone")]

namespace Firestone_bot
{
    public class BotMain : MelonMod
    {
        private static BotController _botController;
        private static BotConfig _config;
        private static readonly string _configPath = "Mods/config.json";
        private static BotFunction _currentFunction = BotFunction.GuildExpeditions;
        
        private static readonly System.Collections.Generic.Dictionary<BotFunction, (System.Func<bool> IsEnabled, System.Action Process)> _modules = new()
        {
            { BotFunction.GuildExpeditions, (() => _config.GuildExpeditions, GuildExpeditions.ProcessExpeditions) },
            { BotFunction.MapMissions, (() => _config.MapMissions, MapMissions.ProcessMapMissions) },
            { BotFunction.DailyTasks, (() => _config.DailyTasks, DailyTasks.ProcessDailyTasks) },
            { BotFunction.MeteoriteResearch, (() => _config.MeteoriteResearch, MeteoriteResearch.ProcessMeteoriteResearch) },
            { BotFunction.FirestoneResearch, (() => _config.FirestoneResearch, FirestoneResearch.ProcessFirestoneResearch) },
            { BotFunction.MysteryBox, (() => _config.MysteryBox, MysteryBox.ProcessMysteryBox) },
            { BotFunction.CheckIn, (() => _config.CheckIn, CheckIn.ProcessCheckIn) },
            { BotFunction.OracleRituals, (() => _config.OracleRituals, OracleRituals.ProcessOracleRituals) },
            { BotFunction.OraclesGift, (() => _config.OraclesGift, OraclesGift.ProcessOraclesGift) },
            { BotFunction.OracleBlessings, (() => _config.OracleBlessings, OracleBlessings.ProcessOracleBlessings) },
            { BotFunction.Upgrades, (() => _config.Upgrades, Upgrades.ProcessUpgrades) },
            { BotFunction.Chests, (() => _config.Chests, Chests.ProcessChests) },
            { BotFunction.Tanks, (() => _config.Tanks, Tanks.ProcessTanks) },
            { BotFunction.Alchemy, (() => _config.Alchemy, Alchemy.ProcessAlchemy) },
            { BotFunction.Engineer, (() => _config.Engineer, Engineer.ProcessEngineer) },
            { BotFunction.CloseWindows, (() => _config.CloseWindows, CloseWindows.CloseAllWindows) }
        };

        [System.Obsolete("Переопределяет устаревший метод MelonBase.OnApplicationStart()")]
        public override void OnApplicationStart()
        {
            ClassInjector.RegisterTypeInIl2Cpp<BotController>();
            var botGO = new GameObject("BotController");
            _botController = botGO.AddComponent<BotController>();
            LoadConfig();
            LogModulesStatus();
            MelonLogger.Msg("Бот инициализирован!\nF5 - Включить/Выключить бота\nF7 - Включить/Выключить модуль Chests\nF8 - Включить/Выключить модуль Debug");
        }

        public override void OnUpdate()
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.F5))
                {
                    _config.Enabled = !_config.Enabled;
                    MelonLogger.Msg(_config.Enabled ? "Бот включен" : "Бот выключен");
                }
                
                if (Input.GetKeyDown(KeyCode.F7))
                {
                    _config.Chests = !_config.Chests;
                    MelonLogger.Msg(_config.Chests ? "Модуль Chests включен" : "Модуль Chests выключен");
                }

                if (_config.DebugEnabled)
                    DebugManager.CheckDebugToggle();
                
                if (!_config.Enabled) return;
                
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
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка загрузки конфига: {ex.Message}");
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
    
    public class BotConfig
    {
        public bool Enabled { get; set; } = true;
        public string LogLevel { get; set; } = "info";
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
        public bool Tanks { get; set; } = false;
        public bool Alchemy { get; set; } = true;
        public bool Engineer { get; set; } = true;
        public bool CloseWindows { get; set; } = false;
        public string MissionTypesPriority { get; set; } = "(Scout=1, Adventure=2, War=3)";
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