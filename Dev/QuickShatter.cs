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
            Log("Attempting to shatter item: " + (item != null ? item.getAsUnpacked().FormatForLog() : "null"));
            if (item == null) return;
            if (CheckCanShatter(item))
            {
                Log("We Can shatter " + (item != null ? item.getAsUnpacked().FormatForLog() : "null"));
                ShatterItem(item);
                var container = _opener.GetComponentInChildren<InventoryContainerUI>()?.container;
                var content = container?.GetContent();

                if (container != null)
                {

                    Log("Item container content found");
                    foreach (var entry in content)
                    {
                        if (entry.data.Equals(item))
                        {
                            Log("Trying to delete matching Item");
                            container.TryRemoveItem(entry, 1, Context.CLEARING);
                            return;
                        }
                    }
                }
            }
        }

        public static bool CheckCanShatter(ItemData item)
        {
            if (item == null) return false;
            bool canShatter = item.isEquipment() && !item.isUniqueSetOrLegendary() && !item.isIdol() && !item.isIdolAltar() && !item.IsCorrupted();
            return canShatter;
        }

        public static void ShatterItem(ItemData item)
        {
            var manager = GameReferencesCache.craftingManager;
            if (manager == null || item == null) return;


            manager.Shatter(item, out _);

        }

        [HarmonyPatch(typeof(TooltipItemManager), "OpenItemTooltip", new Type[] { typeof(ItemDataUnpacked), typeof(TooltipItemManager.SlotType), typeof(Vector2), typeof(Vector3), typeof(GameObject), typeof(Vector2), })]
        public class TooltipItemManagerPatch
        {
            static void Prefix(ItemDataUnpacked data, TooltipItemManager.SlotType type, Vector2 _offset, Vector3 position, GameObject opener, Vector2 openerSize = default(Vector2))
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
