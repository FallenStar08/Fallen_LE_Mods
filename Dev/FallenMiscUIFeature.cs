using Fallen_LE_Mods.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Fallen_LE_Mods.Dev
{
    internal class FallenMiscUIFeature : IFallenFeature
    {
        private GameObject? _uiContainer;

        public void OnMelonSceneLoaded(string sceneName)
        {
            // 1. Setup or find the persistent container
            if (_uiContainer == null)
            {
                _uiContainer = GameObject.Find("Fallen_Persistent_UI");
                if (_uiContainer == null)
                {
                    _uiContainer = new GameObject("Fallen_Persistent_UI");
                    UnityEngine.Object.DontDestroyOnLoad(_uiContainer);

                    // Add a Canvas so UI elements actually render
                    var canvas = _uiContainer.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    _uiContainer.AddComponent<CanvasScaler>();
                    _uiContainer.AddComponent<GraphicRaycaster>();
                }
            }

            // 2. Define the paths to find the original source objects
            string[] paths = {
                "GUI/Panel System/Panel Stacks/Right Panel Stack/InventoryPanel(Clone)/InventoryHolder/Tab Contents/Items Tab/Inventory Tab Footer Base/Currencies/Gold",
                "GUI/Panel System/Panel Stacks/Right Panel Stack/InventoryPanel(Clone)/InventoryHolder/Tab Contents/Items Tab/Inventory Tab Footer Base/Currencies/Faction Favor",
                "GUI/Panel System/Panel Stacks/Right Panel Stack/InventoryPanel(Clone)/InventoryHolder/Tab Contents/Items Tab/Inventory Tab Footer Base/Currencies/Ancient Bones"
            };

            for (int i = 0; i < paths.Length; i++)
            {
                CreatePersistentElement(paths[i], i);
            }
        }

        private void CreatePersistentElement(string path, int index)
        {
            string name = "Persistent_" + index;

            // Check if we already cloned this specific element
            if (_uiContainer.transform.Find(name) != null) return;

            GameObject source = GameObject.Find(path);
            if (source == null) return;

            GameObject clone = UnityEngine.Object.Instantiate(source, _uiContainer.transform);
            clone.name = name;

            RectTransform rect = clone.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Align to Bottom Right
                rect.anchorMin = new Vector2(1, 0);
                rect.anchorMax = new Vector2(1, 0);
                rect.pivot = new Vector2(1, 0);

                // Stack them vertically (adjust -50 * index for spacing)
                rect.anchoredPosition = new Vector2(-20, 20 + (index * 40));
            }
        }
    }
}