using System.Collections;
using Fallen_LE_Mods.Shared;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using static Fallen_LE_Mods.Shared.FallenUtils;

namespace Fallen_LE_Mods.Dev
{
    public class QuickShatter : MelonMod
    {
        private static ItemDataUnpacked? _hoveredItem;
        private static GameObject? _opener;

        public static void Initialize()
        {
            MelonCoroutines.Start(InputLoop());
        }

        private static IEnumerator InputLoop()
        {
            while (true)
            {
                if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.X) && UnityEngine.Input.GetMouseButtonDown(0))
                {
                    if (_hoveredItem != null)
                    {
                        QuickShatterItem(_hoveredItem);
                    }
                }
                yield return null;
            }
        }

        public static void QuickShatterItem(ItemData item)
        {
            if (item == null) return;
            LogDebug("Attempting to shatter item: " + item.getAsUnpacked().FormatForLog());
            if (CheckCanShatter(item))
            {
                LogDebug("Conditions met. Shattering item...");

                ShatterItem(item);

                DeducedRuneQuantity();

                LogDebug($"{_opener?.name} is the opener.");
                var container = _opener?.GetComponentInChildren<InventoryContainerUI>()?.container;
                LogDebug($"{container?.Type} is the type of opener's container");
                var content = container?.GetContent();
                LogDebug($"Container content count: {content?.Count}");
                //Only works for player inv since we're the opener.
                //Need to find a way to generalize to any container...
                if (container != null && content != null)
                {
                    foreach (var entry in content)
                    {
                        if (entry.data.Equals(item))
                        {
                            Log("Removing shattered item from inventory.");
                            container.TryRemoveItem(entry, 1, Context.CLEARING);

                            break;
                        }
                    }
                }
            }
        }

        public static bool CheckCanShatter(ItemData item)
        {
            if (item == null) return false;

            if (GetRuneQuantity() < 1)
            {
                MakeNotification($"Not enough Runes to shatter!");
                return false;
            }

            bool canShatter = item.isEquipment()
                && !item.isUniqueSetOrLegendary()
                && !item.isIdol()
                && !item.isIdolAltar()
                && !item.IsCorrupted();

            return canShatter;
        }

        public static void ShatterItem(ItemData item)
        {
            var manager = GameReferencesCache.CraftingManager.Value;
            if (manager != null && item != null)
            {
                manager.Shatter(item, out _);
            }
        }

        private static int GetRuneQuantity(int index = 0)
        {
            var container = GameReferencesCache.MaterialContainers.Value;
            int runeQuantity = container != null ? container.RuneContainers[index].GetQuantity() : 0;
            return runeQuantity;
        }

        private static int DeducedRuneQuantity(int quantity = 1, int index = 0)
        {
            var container = GameReferencesCache.MaterialContainers.Value;
            if (container != null)
            {
                var runeContainer = container.RuneContainers[index];
                int currentQuantity = runeContainer.GetQuantity();
                int newQuantity = Mathf.Max(0, currentQuantity - quantity);
                runeContainer.content.Quantity = newQuantity;
                return newQuantity;
            }
            return 0;
        }

        [HarmonyPatch(typeof(TooltipItemManager), "OpenItemTooltip", new Type[] { typeof(ItemDataUnpacked), typeof(TooltipItemManager.SlotType), typeof(Vector2), typeof(Vector3), typeof(GameObject), typeof(Vector2) })]
        public class TooltipItemManagerPatch
        {
            static void Postfix(ItemDataUnpacked data, TooltipItemManager.SlotType type, GameObject opener)
            {
                if (data == null) return;
                if (type != TooltipItemManager.SlotType.EQUIPPED)
                {
                    _opener = opener;
                    _hoveredItem = data;
                }
            }
        }

        [HarmonyPatch(typeof(TooltipItemManager), "CloseTooltip")]
        public class TooltipHidePatch
        {
            static void Postfix()
            {
                _hoveredItem = null;
                _opener = null;
            }
        }
    }
}