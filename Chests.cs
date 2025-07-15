using MelonLoader;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FirestoneBot
{
    public static class Chests
    {
        private enum State { OpenInventory, OpenChestsTab, ProcessChests, WaitForChestWindow, WaitForOpenResult, Complete }
        private static State _state = State.OpenInventory;
        private static int _currentChestIndex = 0;
        private static float _waitTimer = 0f;
        private static float _lastRunTime = 0f;
        
        public static void ProcessChests()
        {
            if (Time.time - _lastRunTime < 60f && _state == State.OpenInventory)
            {
                BotMain.NextFunction();
                return;
            }
            
            switch (_state)
            {
                case State.OpenInventory:
                    var inventoryBtn = GameUtils.FindByPath("battleRoot/battleMain/battleCanvas/SafeArea/bottomSideUIDesktop/menuButtons/inventoryButton");
                    if (GameUtils.ClickButton(inventoryBtn))
                        _state = State.OpenChestsTab;
                    else
                    {
                        BotMain.NextFunction();
                        _state = State.OpenInventory;
                    }
                    break;
                    
                case State.OpenChestsTab:
                    var chestsTab = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/Inventory/submenuButtons/chests");
                    if (GameUtils.ClickButton(chestsTab))
                        _state = State.ProcessChests;
                    break;
                    
                case State.ProcessChests:
                    if (!ProcessNextChest())
                    {
                        GameUtils.CloseWindow("menus/Inventory");
                        _state = State.Complete;
                    }
                    break;
                    
                case State.WaitForChestWindow:
                    if (Time.time >= _waitTimer)
                    {
                        ProcessChestWindow();
                    }
                    break;
                    
                case State.WaitForOpenResult:
                    if (Time.time >= _waitTimer)
                    {
                        var closeBtn = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/ChestOpening/closeButton");
                        if (closeBtn != null)
                        {
                            GameUtils.ClickButton(closeBtn);
                            _currentChestIndex++;
                            _state = State.ProcessChests;
                        }
                    }
                    break;
                    
                case State.Complete:
                    _lastRunTime = Time.time;
                    BotMain.NextFunction();
                    _state = State.OpenInventory;
                    _currentChestIndex = 0;
                    break;
            }
        }
        
        private static bool ProcessNextChest()
        {
            var chests = FindAllChests();
            
            if (_currentChestIndex >= chests.Length)
                return false;
            
            var chest = chests[_currentChestIndex];
            if (GameUtils.ClickHandler(chest))
            {
                _waitTimer = Time.time + 0.5f;
                _state = State.WaitForChestWindow;
            }
            else
            {
                _currentChestIndex++;
            }
            return true;
        }
        
        private static void ProcessChestWindow()
        {
            string[] windowTypes = { "OraclesGiftOpenPreview", "MysteryBoxOpenPreview", "ChestOpenPreview" };
            
            foreach (var windowType in windowTypes)
            {
                var basePath = $"menusRoot/menuCanvasParent/SafeArea/menuCanvas/popups/{windowType}/bg/openingOptions/";
                if (GameUtils.ClickButton(GameUtils.FindByPath(basePath + "openxMax")) || 
                    GameUtils.ClickButton(GameUtils.FindByPath(basePath + "openx1")))
                    break;
            }
            
            _waitTimer = Time.time + 0.5f;
            _state = State.WaitForOpenResult;
        }
        

        
        private static readonly string[] ChestTypes = { "Common", "Uncommon", "Rare", "Epic", "Legendary", "Wooden", "Iron", "Golden", "Comet", "Lunar", "Solar", "oraclesGift", "mysteryBox" };
        

        
        private static GameObject[] FindAllChests()
        {
            var chests = new System.Collections.Generic.List<GameObject>();
            var allButtons = UnityEngine.Object.FindObjectsOfType<UnityEngine.UI.Button>();
            
            foreach (var button in allButtons)
            {
                if (button.gameObject.activeInHierarchy && 
                    System.Array.Exists(ChestTypes, type => button.name.Contains(type)))
                {
                    chests.Add(button.gameObject);
                }
            }
            
            return chests.ToArray();
        }
        

    }
}