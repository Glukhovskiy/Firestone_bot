using UnityEngine;
using UnityEngine.EventSystems;
using MelonLoader;

namespace FirestoneBot
{
    public static class GameUtils
    {
        public static GameObject FindByPath(string path)
        {
            var parts = path.Split('/');
            GameObject current = null;
            
            foreach (var part in parts)
            {
                current = current == null ? GameObject.Find(part) : current.transform.Find(part)?.gameObject;
                if (current == null) return null;
            }
            
            return current?.activeInHierarchy == true ? current : null;
        }
        
        public static bool ClickButton(GameObject obj)
        {
            if (obj == null) return false;
            
            var button = obj.GetComponent<UnityEngine.UI.Button>();
            if (button?.interactable == true)
            {
                button.onClick.Invoke();
                return true;
            }
            return false;
        }
        
        public static GameObject FindButton(string name)
        {
            var obj = GameObject.Find(name);
            return obj?.GetComponent<UnityEngine.UI.Button>()?.interactable == true ? obj : null;
        }
        
        public static void CloseWindow(string windowPath)
        {
            var closeBtn = FindByPath($"{windowPath}/closeButton");
            ClickButton(closeBtn);
        }
        
        public static bool ClickHandler(GameObject obj)
        {
            if (obj == null) return false;
            
            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(null, obj.transform.position)
            };
            
            ExecuteEvents.Execute(obj, pointerData, ExecuteEvents.pointerClickHandler);
            return true;
        }
    }
}