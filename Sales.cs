using MelonLoader;
using UnityEngine;

namespace Firestone_bot
{
    public static class Sales
    {
        private enum State { OpenInventory, OpenItemsTab, CheckItems, OpenTown, OpenMerchant, OpenSellTab, CheckSellButtons, Complete }
        private static State _state = State.OpenInventory;
        private static float _waitTimer = 0f;
        private static float _lastCheckTime = 0f;
        private static bool _hasItemsToSell = false;
        
        public static void ProcessSales()
        {
            try
            {
                // Проверяем раз в 5 минут
                if (Time.time - _lastCheckTime < 30f)
                {
                    BotMain.NextFunction();
                    return;
                }
                
                switch (_state)
                {
                    case State.OpenInventory:
                        var inventoryButton = GameUtils.FindByPath("battleRoot/battleMain/battleCanvas/SafeArea/bottomSideUIDesktop/menuButtons/inventoryButton");
                        if (GameUtils.ClickButton(inventoryButton))
                        {
                            _waitTimer = Time.time + 1f;
                            _state = State.OpenItemsTab;
                            DebugManager.DebugLog("Инвентарь открыт");
                        }
                        else
                        {
                            _state = State.Complete;
                        }
                        break;
                        
                    case State.OpenItemsTab:
                        if (Time.time >= _waitTimer)
                        {
                            var itemsTab = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/Inventory/submenuButtons/inventoryItems");
                            if (GameUtils.ClickButton(itemsTab))
                            {
                                _waitTimer = Time.time + 1f;
                                _state = State.CheckItems;
                                DebugManager.DebugLog("Закладка предметов открыта");
                            }
                            else
                            {
                                _state = State.Complete;
                            }
                        }
                        break;
                        
                    case State.CheckItems:
                        if (Time.time >= _waitTimer)
                        {
                            CheckInventoryItems();
                            if (_hasItemsToSell)
                            {
                                GameUtils.CloseWindow("menus/Inventory");
                                _waitTimer = Time.time + 1f;
                                _state = State.OpenTown;
                            }
                            else
                            {
                                _state = State.Complete;
                            }
                        }
                        break;
                        
                    case State.OpenTown:
                        if (Time.time >= _waitTimer)
                        {
                            var townButton = GameUtils.FindByPath("battleRoot/battleMain/battleCanvas/SafeArea/rightSideUI/menuButtons/townButton");
                            if (GameUtils.ClickButton(townButton))
                            {
                                _waitTimer = Time.time + 1f;
                                _state = State.OpenMerchant;
                                DebugManager.DebugLog("Город открыт");
                            }
                            else
                            {
                                _state = State.Complete;
                            }
                        }
                        break;
                        
                    case State.OpenMerchant:
                        if (Time.time >= _waitTimer)
                        {
                            var merchantButton = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/TownIrongard/townBg/parent/exoticMerchant");
                            if (GameUtils.ClickButton(merchantButton))
                            {
                                _waitTimer = Time.time + 1f;
                                _state = State.OpenSellTab;
                                DebugManager.DebugLog("Магазин торговца открыт");
                            }
                            else
                            {
                                _state = State.Complete;
                            }
                        }
                        break;
                        
                    case State.OpenSellTab:
                        if (Time.time >= _waitTimer)
                        {
                            var sellTab = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/ExoticMerchant/submenus/submenuButtons/sellItems");
                            if (GameUtils.ClickButton(sellTab))
                            {
                                _waitTimer = Time.time + 1f;
                                _state = State.CheckSellButtons;
                                DebugManager.DebugLog("Вкладка продажи открыта");
                            }
                            else
                            {
                                _state = State.Complete;
                            }
                        }
                        break;
                        
                    case State.CheckSellButtons:
                        if (Time.time >= _waitTimer)
                        {
                            CheckSellButtons();
                            _state = State.Complete;
                        }
                        break;
                        
                    case State.Complete:
                        GameUtils.CloseWindow("menus/Inventory");
                        GameUtils.CloseWindow("menus/ExoticMerchant");
                        
                        var townCloseButton = GameUtils.FindByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/TownIrongard/closeButton");
                        GameUtils.ClickButton(townCloseButton);
                        
                        _lastCheckTime = Time.time;
                        BotMain.NextFunction();
                        ResetState();
                        break;
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Ошибка в ProcessSales: {ex.Message}");
                BotMain.NextFunction();
                ResetState();
            }
        }
        
        private static void CheckInventoryItems()
        {
            var contentPath = "menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/Inventory/submenus/items/ScrollView/Viewport/Content";
            var content = GameUtils.FindByPath(contentPath);
            
            if (content != null)
            {
                var itemsToSell = new System.Collections.Generic.List<GameObject>();
                
                for (int i = 0; i < content.transform.childCount; i++)
                {
                    var child = content.transform.GetChild(i);
                    if (child.gameObject.activeInHierarchy)
                    {
                        var itemName = child.name;
                        
                        if (ConfigManager.Config.SalesItems.ContainsKey(itemName) && 
                            ConfigManager.Config.SalesItems[itemName])
                        {
                            itemsToSell.Add(child.gameObject);
                        }
                    }
                }
                
                if (itemsToSell.Count > 0)
                {
                    MelonLogger.Msg($"Найдено {itemsToSell.Count} предметов для продажи:");
                    
                    foreach (var item in itemsToSell)
                    {
                        MelonLogger.Msg($"- {item.name}");
                    }
                    
                    _hasItemsToSell = true;
                }
                else
                {
                    DebugManager.DebugLog("Предметов для продажи не найдено");
                    _hasItemsToSell = false;
                }
            }
        }
        
        private static void CheckSellButtons()
        {
            var contentPath = "menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/ExoticMerchant/submenus/bg/sellItemsSubmenu/Scroll View/Viewport/Content/productGrid";
            var productGrid = GameUtils.FindByPath(contentPath);
            
            if (productGrid != null)
            {
                var soldCount = 0;
                
                for (int i = 0; i < productGrid.transform.childCount; i++)
                {
                    var child = productGrid.transform.GetChild(i);
                    if (child.gameObject.activeInHierarchy)
                    {
                        var sellButton = child.Find("sellButton");
                        if (sellButton != null)
                        {
                            var button = sellButton.GetComponent<UnityEngine.UI.Button>();
                            if (button != null && button.interactable)
                            {
                                var itemSprite = GetItemSprite(child);
                                if (!string.IsNullOrEmpty(itemSprite))
                                {
                                    var itemName = ConvertSpriteToItemName(itemSprite);
                                    
                                    if (ConfigManager.Config.SalesItems.ContainsKey(itemName) &&
                                        ConfigManager.Config.SalesItems[itemName])
                                    {
                                        var clickCount = 0;
                                        while (button.interactable && GameUtils.ClickButton(sellButton.gameObject))
                                        {
                                            clickCount++;
                                            System.Threading.Thread.Sleep(100); // Короткая задержка
                                        }

                                        if (clickCount > 0)
                                        {
                                            MelonLogger.Msg($"Продан: {itemName} (x{clickCount})");
                                            soldCount++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                if (soldCount > 0)
                {
                    MelonLogger.Msg($"Продано предметов: {soldCount}");
                }
                else
                {
                    DebugManager.DebugLog("Предметов для продажи не найдено");
                }
            }
        }
        
        private static string GetItemSprite(Transform itemTransform)
        {
            var images = itemTransform.GetComponentsInChildren<UnityEngine.UI.Image>();
            
            foreach (var image in images)
            {
                if (image.sprite != null && !string.IsNullOrEmpty(image.sprite.name))
                {
                    var spriteName = image.sprite.name;
                    
                    // Ищем спрайты предметов (не UI элементы)
                    if (!spriteName.Contains("Bg") && !spriteName.Contains("Frame") && 
                        !spriteName.Contains("Button") && !spriteName.Contains("Coin") &&
                        !spriteName.Contains("Outline") && !spriteName.Contains("Area"))
                    {
                        return spriteName;
                    }
                }
            }
            
            return null;
        }
        
        private static string ConvertSpriteToItemName(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName)) return spriteName;
            
            // Преобразуем camelCase в PascalCase
            return char.ToUpper(spriteName[0]) + spriteName.Substring(1);
        }
        
        private static void ResetState()
        {
            _state = State.OpenInventory;
            _waitTimer = 0f;
            _hasItemsToSell = false;
        }
    }
}