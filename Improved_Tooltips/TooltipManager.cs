using Fallen_LE_Mods.Shared;
using HarmonyLib;
using Il2Cpp;
using Il2CppItemFiltering;
using MelonLoader;
using UnityEngine;

namespace Fallen_LE_Mods.Improved_Tooltips


{

    public class TooltipManager : MelonMod
    {
        private const string LoreMarker = "\u200B\u200B\u200B"; // Invisible marker

        private static void HandleTooltipUpdate(ItemDataUnpacked item)
        {
            if (item == null) return;
            if (item.LoreText != null && item.LoreText.Contains(LoreMarker))
            {
                // Already modified
                int markerIndex = item.LoreText.IndexOf(LoreMarker);
                if (markerIndex >= 0)
                    item.LoreText = item.LoreText.Substring(0, markerIndex); // Strip prior additions
            }

            string additions = "";
            bool addedFilterText = false;

            Rule? match = FallenUtils.MatchFilterRule(item);
            if (match != null && (ItemList.isEquipment(item.itemType) || ItemList.isIdol(item.itemType)))
            {
                var description = match.GetRuleDescription();
                if (description != null)
                {
                    if (item.LoreText == "")
                        additions += $"FilterRule : {description}";
                    else
                        additions += $"\n\n</color>FilterRule : {description}";

                    addedFilterText = true;
                }
            }

            ItemDataUnpacked? matchedUniqueOrSet = FallenUtils.FindSimilarUniqueItemInStash(item);
            if (matchedUniqueOrSet != null)
            {
                int ownedLP = matchedUniqueOrSet.legendaryPotential;
                string LPdescription = ownedLP > item.legendaryPotential ? $"with higher LP : {ownedLP}" :
                                       ownedLP < item.legendaryPotential ? $"with lower LP : {ownedLP}" :
                                       matchedUniqueOrSet.Equals(item) ? "with similar LP (Self)" :
                                                                        "with similar LP (Duplicate)";
                string description = $"Already Owned {LPdescription}";

                additions += addedFilterText ? $"\n\n{description}" : $"\n\n</color>{description}";
            }
            else if (item.isUniqueSetOrLegendary())
            {
                string description = "Not Owned";
                additions += $"\n\n</color>{description}";
            }
            if (item.LoreText == null)
            {
                item.LoreText = "";
            }

            // Only modify if new text would be different
            string newLore = item.LoreText + additions + LoreMarker;
            if (item.LoreText != newLore)
                item.LoreText = newLore;
        }


        //Postfix(Il2Cpp.UITooltipItem __instance, Il2Cpp.UITooltipItem.ItemTooltipInfo __0, UnityEngine.Vector2 __1, UnityEngine.GameObject __2, Il2Cpp.ItemDataUnpacked __3, Il2Cpp.TooltipItemManager.SlotType __4)

        [HarmonyPatch(typeof(TooltipItemManager), "OpenItemTooltip", new Type[] { typeof(ItemDataUnpacked), typeof(TooltipItemManager.SlotType), typeof(Vector2), typeof(Vector3), typeof(GameObject), typeof(Vector2), })]
        public class TooltipItemManagerPatch
        {
            static void Prefix(
                    TooltipItemManager __instance,
                    ItemDataUnpacked data,
                    TooltipItemManager.SlotType type,
                    Vector2 _offset,
                    Vector3 position,
                    GameObject opener,
                    Vector2 openerSize)
            {
                if (data == null) return;



                TooltipManager.HandleTooltipUpdate(data);
            }



        }
    }
}