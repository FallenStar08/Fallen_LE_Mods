using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using static Fallen_LE_Mods.Shared.FallenUtils;

namespace Fallen_LE_Mods.Dev
{
    using HarmonyLib;
    using Il2Cpp;
    using MelonLoader;
    using UnityEngine;
    using System.Collections;

    namespace Fallen_LE_Mods.Dev
    {
        [HarmonyPatch(typeof(InventoryItemUI), nameof(InventoryItemUI.OnEnable))]
        public class InventoryAutoSortPatch
        {
            private static bool _isSorting = false;
            private static readonly float _debounceDelay = 0.25f;

            public static void Postfix()
            {
                if (_isSorting) return;

                MelonCoroutines.Start(DelayedSort());
            }

            private static IEnumerator DelayedSort()
            {
                _isSorting = true;

                yield return new WaitForSeconds(_debounceDelay);

                try
                {
                    var sortButton = GameObject.FindObjectOfType<SortInventoryButton>();

                    if (sortButton != null)
                    {
                        sortButton.OnPress();
                        Log("[Inventory] Auto-sorted inventory.");
                    }
                    else
                    {
                        Log("[Inventory] Sort button not found in UI.");
                    }
                }
                catch (System.Exception ex)
                {
                    Log($"[Inventory] Failed to auto-sort: {ex.Message}");
                }

                _isSorting = false;
            }
        }
    }
}
