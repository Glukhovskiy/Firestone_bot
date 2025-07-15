using MelonLoader;
using UnityEngine;

namespace FirestoneBot
{
    public static class Upgrades
    {
        private static bool _upgradesButtonClicked = false;
        private static float _upgradesButtonClickTime = 0f;
        
        public static void ProcessUpgrades()
        {
            GameObject changeLevelUpModeButton = FindChangeLevelUpModeButton();
            if (changeLevelUpModeButton != null)
            {
                ProcessUpgradeButtons();
            }
            else if (!_upgradesButtonClicked)
            {
                GameObject upgradesButton = FindUpgradesButton();
                if (upgradesButton != null)
                {
                    var button = upgradesButton.GetComponent<UnityEngine.UI.Button>();
                    if (button != null && button.interactable)
                    {
                        button.onClick.Invoke();
                        _upgradesButtonClicked = true;
                        _upgradesButtonClickTime = Time.time;
                        return;
                    }
                }
                else
                {
                    BotMain.NextFunction();
                    ResetState();
                }
            }
            else if (Time.time - _upgradesButtonClickTime >= 1f)
            {
                GameObject changeLevelUpModeButtonAfter = FindChangeLevelUpModeButton();
                if (changeLevelUpModeButtonAfter != null)
                {
                    ProcessUpgradeButtons();
                }
                else
                {
                    BotMain.NextFunction();
                    ResetState();
                }
            }
        }
        
        private static void ProcessUpgradeButtons()
        {
            bool anyButtonPressed = false;
            
            for (int i = 0; i <= 4; i++)
            {
                GameObject lvlUpButton = FindLvlUpButton(i);
                if (lvlUpButton != null)
                {
                    var button = lvlUpButton.GetComponent<UnityEngine.UI.Button>();
                    if (button != null && button.interactable)
                    {
                        button.onClick.Invoke();
                        anyButtonPressed = true;
                    }
                }
            }
            
            GameObject guardianLvlUpButton = FindGuardianLvlUpButton();
            if (guardianLvlUpButton != null)
            {
                var button = guardianLvlUpButton.GetComponent<UnityEngine.UI.Button>();
                if (button != null && button.interactable)
                {
                    button.onClick.Invoke();
                    anyButtonPressed = true;
                }
            }
            
            GameObject buyButton = FindBuyButton();
            if (buyButton != null)
            {
                var button = buyButton.GetComponent<UnityEngine.UI.Button>();
                if (button != null && button.interactable)
                {
                    button.onClick.Invoke();
                    anyButtonPressed = true;
                }
            }
            
            if (!anyButtonPressed)
            {
                BotMain.NextFunction();
                ResetState();
            }
        }
        
        private static GameObject FindChangeLevelUpModeButton()
        {
            return FindObjectByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/Upgrades/changeLevelUpModeButton");
        }
        
        private static GameObject FindUpgradesButton()
        {
            return FindObjectByPath("battleRoot/battleMain/battleCanvas/SafeArea/bottomSideUIDesktop/menuButtons/upgradesButton");
        }
        
        private static GameObject FindLvlUpButton(int slotIndex)
        {
            return FindObjectByPath($"menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/Upgrades/upgradesList/heroSlot ({slotIndex})/lvlUpButton");
        }
        
        private static GameObject FindGuardianLvlUpButton()
        {
            return FindObjectByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/Upgrades/upgradesList/guardianSlot/lvlUpButton");
        }
        
        private static GameObject FindBuyButton()
        {
            return FindObjectByPath("menusRoot/menuCanvasParent/SafeArea/menuCanvas/menus/Upgrades/upgradesList/customUpgrade/buyButton");
        }
        
        private static GameObject FindObjectByPath(string targetPath)
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    Transform current = obj.transform;
                    string path = "";
                    while (current != null)
                    {
                        path = current.name + "/" + path;
                        current = current.parent;
                    }
                    path = path.TrimEnd('/');
                    
                    if (path == targetPath)
                    {
                        return obj;
                    }
                }
            }
            
            return null;
        }
        
        private static void ResetState()
        {
            _upgradesButtonClicked = false;
            _upgradesButtonClickTime = 0f;
        }
    }
}