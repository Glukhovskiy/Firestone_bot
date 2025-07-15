using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;
using System.IO;

[assembly: MelonInfo(typeof(FirestoneBot.BotMain), "Firestone Bot", "1.0.0", "YourName")]
[assembly: MelonGame("HolydayStudios", "Firestone")]

namespace FirestoneBot
{
    public class BotMain : MelonMod
    {
        private static BotController _botController;
        private static BotConfig _config;
        private static string _configPath = "Mods/config.json";
        private static BotFunction _currentFunction = BotFunction.GuildExpeditions;

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

                switch (_currentFunction)
                {
                    case BotFunction.GuildExpeditions:
                        if (_config.guildExpeditions)
                            GuildExpeditions.ProcessExpeditions();
                        else
                            NextFunction();
                        break;
                    case BotFunction.MapMissions:
                        if (_config.mapMissions)
                            MapMissions.ProcessMapMissions();
                        else
                            NextFunction();
                        break;
                    case BotFunction.DailyTasks:
                        if (_config.dailyTasks)
                            DailyTasks.ProcessDailyTasks();
                        else
                            NextFunction();
                        break;
                    case BotFunction.MeteoriteResearch:
                        if (_config.meteoriteResearch)
                            MeteoriteResearch.ProcessMeteoriteResearch();
                        else
                            NextFunction();
                        break;
                    case BotFunction.FirestoneResearch:
                        if (_config.firestoneResearch)
                            FirestoneResearch.ProcessFirestoneResearch();
                        else
                            NextFunction();
                        break;
                    case BotFunction.MysteryBox:
                        if (_config.mysteryBox)
                            MysteryBox.ProcessMysteryBox();
                        else
                            NextFunction();
                        break;
                    case BotFunction.CheckIn:
                        if (_config.checkIn)
                            CheckIn.ProcessCheckIn();
                        else
                            NextFunction();
                        break;
                    case BotFunction.OracleRituals:
                        if (_config.oracleRituals)
                            OracleRituals.ProcessOracleRituals();
                        else
                            NextFunction();
                        break;
                    case BotFunction.OraclesGift:
                        if (_config.oraclesGift)
                            OraclesGift.ProcessOraclesGift();
                        else
                            NextFunction();
                        break;
                    case BotFunction.OracleBlessings:
                        if (_config.oracleBlessings)
                            OracleBlessings.ProcessOracleBlessings();
                        else
                            NextFunction();
                        break;
                    case BotFunction.Upgrades:
                        if (_config.upgrades)
                            Upgrades.ProcessUpgrades();
                        else
                            NextFunction();
                        break;
                    case BotFunction.Chests:
                        if (_config.chests)
                            Chests.ProcessChests();
                        else
                            NextFunction();
                        break;
                    case BotFunction.Tanks:
                        if (_config.tanks)
                            Tanks.ProcessTanks();
                        else
                            NextFunction();
                        break;
                    case BotFunction.Alchemy:
                        if (_config.alchemy)
                            Alchemy.ProcessAlchemy();
                        else
                            NextFunction();
                        break;
                    case BotFunction.Engineer:
                        if (_config.engineer)
                            Engineer.ProcessEngineer();
                        else
                            NextFunction();
                        break;
                    case BotFunction.CloseWindows:
                        if (_config.closeWindows)
                            CloseWindows.CloseAllWindows();
                        else
                            NextFunction();
                        break;
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
                if (File.Exists(_configPath))
                {
                    var lines = File.ReadAllLines(_configPath);
                    _config = new BotConfig();
                    
                    foreach (var line in lines)
                    {
                        var parts = line.Split('=');
                        if (parts.Length != 2) continue;
                        
                        var key = parts[0];
                        var value = parts[1] == "true";
                        
                        switch (key)
                        {
                            case "enabled": _config.enabled = value; break;
                            case "debugEnabled": _config.debugEnabled = value; break;
                            case "guildExpeditions": _config.guildExpeditions = value; break;
                            case "mapMissions": _config.mapMissions = value; break;
                            case "dailyTasks": _config.dailyTasks = value; break;
                            case "meteoriteResearch": _config.meteoriteResearch = value; break;
                            case "firestoneResearch": _config.firestoneResearch = value; break;
                            case "mysteryBox": _config.mysteryBox = value; break;
                            case "checkIn": _config.checkIn = value; break;
                            case "oracleRituals": _config.oracleRituals = value; break;
                            case "oraclesGift": _config.oraclesGift = value; break;
                            case "oracleBlessings": _config.oracleBlessings = value; break;
                            case "upgrades": _config.upgrades = value; break;
                            case "chests": _config.chests = value; break;
                            case "tanks": _config.tanks = value; break;
                            case "alchemy": _config.alchemy = value; break;
                            case "engineer": _config.engineer = value; break;
                            case "closeWindows": _config.closeWindows = value; break;
                            case "logLevel": _config.logLevel = parts[1]; break;
                        }
                    }
                }
                else
                {
                    _config = new BotConfig();
                    SaveConfig();
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
                string[] lines = {
                    $"enabled={_config.enabled.ToString().ToLower()}",
                    $"logLevel={_config.logLevel}",
                    $"debugEnabled={_config.debugEnabled.ToString().ToLower()}",
                    $"guildExpeditions={_config.guildExpeditions.ToString().ToLower()}",
                    $"mapMissions={_config.mapMissions.ToString().ToLower()}",
                    $"dailyTasks={_config.dailyTasks.ToString().ToLower()}",
                    $"meteoriteResearch={_config.meteoriteResearch.ToString().ToLower()}",
                    $"firestoneResearch={_config.firestoneResearch.ToString().ToLower()}",
                    $"mysteryBox={_config.mysteryBox.ToString().ToLower()}",
                    $"checkIn={_config.checkIn.ToString().ToLower()}",
                    $"oracleRituals={_config.oracleRituals.ToString().ToLower()}",
                    $"oraclesGift={_config.oraclesGift.ToString().ToLower()}",
                    $"oracleBlessings={_config.oracleBlessings.ToString().ToLower()}",
                    $"upgrades={_config.upgrades.ToString().ToLower()}",
                    $"chests={_config.chests.ToString().ToLower()}",
                    $"tanks={_config.tanks.ToString().ToLower()}",
                    $"alchemy={_config.alchemy.ToString().ToLower()}",
                    $"engineer={_config.engineer.ToString().ToLower()}",
                    $"closeWindows={_config.closeWindows.ToString().ToLower()}"
                };
                File.WriteAllLines(_configPath, lines);
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка сохранения конфига: {ex.Message}");
            }
        }

        private static bool _collectButtonSeen = false;

        private static bool IsGameReady()
        {
            try
            {
                var collectButton = GameObject.Find("collectButton");
                
                if (collectButton != null)
                {
                    _collectButtonSeen = true;
                    var button = collectButton.GetComponent<UnityEngine.UI.Button>();
                    if (button != null)
                    {
                        button.onClick.Invoke();
                    }
                    return false;
                }
                
                return _collectButtonSeen;
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
        public bool mapMissions { get; set; } = false;
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