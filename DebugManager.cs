using MelonLoader;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Firestone_bot
{
    public static class DebugManager
    {
        private static bool _debugMode = false;

        public static void CheckDebugToggle()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                _debugMode = !_debugMode;
                MelonLogger.Msg(_debugMode ? "Debug ON" : "Debug OFF");
            }
            
            if (_debugMode && Input.GetMouseButtonDown(0))
            {
                CheckObjectClick();
            }
        }
        
        private static void CheckObjectClick()
        {
            var mousePos = Input.mousePosition;
            
            // Поиск UI элементов
            var uiObject = FindUIAtPosition(mousePos);
            if (uiObject != null)
            {
                MelonLogger.Msg($"[DEBUG] UI: {GetObjectPath(uiObject)}");
                LogComponentInfo(uiObject);
            }
            
            // Поиск всех объектов под курсором через Raycast
            FindAllObjectsAtPosition(mousePos);
            
            // Поиск 2D коллайдеров
            var worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            var colliderObject = FindCollider2DAtPosition(worldPos);
            if (colliderObject != null)
            {
                MelonLogger.Msg($"[DEBUG] Collider2D: {GetObjectPath(colliderObject)}");
                LogCollider2DInfo(colliderObject);
            }
        }
        
        private static GameObject FindUIAtPosition(Vector2 screenPos)
        {
            var allObjects = UnityEngine.Object.FindObjectsOfType<RectTransform>();
            GameObject topMostObject = null;
            int highestSiblingIndex = -1;
            
            foreach (var rect in allObjects)
            {
                if (rect.gameObject.activeInHierarchy && 
                    RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos, Camera.main) &&
                    IsInteractiveElement(rect.gameObject))
                {
                    if (rect.GetSiblingIndex() > highestSiblingIndex)
                    {
                        highestSiblingIndex = rect.GetSiblingIndex();
                        topMostObject = rect.gameObject;
                    }
                }
            }
            return topMostObject;
        }
        
        private static void FindAllObjectsAtPosition(Vector2 screenPos)
        {
            // Поиск через UI Raycast
            if (EventSystem.current != null)
            {
                var pointerData = new PointerEventData(EventSystem.current) { position = screenPos };
                var results = new Il2CppSystem.Collections.Generic.List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);
                
                foreach (var result in results)
                {
                    MelonLogger.Msg($"[DEBUG] UI Raycast: {GetObjectPath(result.gameObject)}");
                    LogAllComponents(result.gameObject);
                }
            }
            
            // Поиск через 3D Raycast
            var ray = Camera.main.ScreenPointToRay(screenPos);
            var hits = Physics.RaycastAll(ray);
            foreach (var hit in hits)
            {
                MelonLogger.Msg($"[DEBUG] 3D Raycast: {GetObjectPath(hit.collider.gameObject)}");
                LogAllComponents(hit.collider.gameObject);
            }
        }
        
        private static GameObject FindCollider2DAtPosition(Vector3 worldPos)
        {
            var hit = Physics2D.OverlapPoint(worldPos);
            return hit?.gameObject;
        }
        
        private static bool IsInteractiveElement(GameObject obj)
        {
            return obj.GetComponent<UnityEngine.UI.Button>() != null ||
                   obj.GetComponent<UnityEngine.UI.Toggle>() != null ||
                   obj.GetComponent<UnityEngine.UI.Slider>() != null ||
                   obj.GetComponent<UnityEngine.UI.Dropdown>() != null ||
                   obj.GetComponent<UnityEngine.UI.InputField>() != null ||
                   obj.GetComponent<UnityEngine.UI.Scrollbar>() != null ||
                   obj.GetComponent<UnityEngine.UI.ScrollRect>() != null;
        }
        

        private static string GetObjectPath(GameObject obj)
        {
            var path = obj.name;
            var current = obj.transform.parent;
            
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            
            return path;
        }

        private static void LogComponentInfo(GameObject obj)
        {
            var button = obj.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
                MelonLogger.Msg($"[DEBUG] Button: interactable={button.interactable}");
                
            var toggle = obj.GetComponent<UnityEngine.UI.Toggle>();
            if (toggle != null)
                MelonLogger.Msg($"[DEBUG] Toggle: isOn={toggle.isOn}, interactable={toggle.interactable}");
                
            var slider = obj.GetComponent<UnityEngine.UI.Slider>();
            if (slider != null)
                MelonLogger.Msg($"[DEBUG] Slider: value={slider.value}, interactable={slider.interactable}");
                
            var dropdown = obj.GetComponent<UnityEngine.UI.Dropdown>();
            if (dropdown != null)
                MelonLogger.Msg($"[DEBUG] Dropdown: value={dropdown.value}, interactable={dropdown.interactable}");
                
            var inputField = obj.GetComponent<UnityEngine.UI.InputField>();
            if (inputField != null)
                MelonLogger.Msg($"[DEBUG] InputField: text='{inputField.text}', interactable={inputField.interactable}");
        }
        
        private static void LogAllComponents(GameObject obj)
        {
            var components = obj.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component != null)
                    MelonLogger.Msg($"[DEBUG] Component: {component.GetType().Name}");
            }
        }
        
        private static void LogCollider2DInfo(GameObject obj)
        {
            var collider2D = obj.GetComponent<Collider2D>();
            if (collider2D != null)
            {
                MelonLogger.Msg($"[DEBUG] Collider2D: enabled={collider2D.enabled}, isTrigger={collider2D.isTrigger}");
                MelonLogger.Msg($"[DEBUG] Collider2D type: {collider2D.GetType().Name}");
            }
        }
        
        public static void DebugLog(string message)
        {
            if (_debugMode)
                MelonLogger.Msg($"[DEBUG] {message}");
        }
    }
}