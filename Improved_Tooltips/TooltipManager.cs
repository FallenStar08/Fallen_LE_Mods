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
            if (item.LoreText == null) item.LoreText = "";

            if (item.LoreText.Contains(LoreMarker))
            {
                int markerIndex = item.LoreText.IndexOf(LoreMarker);
                item.LoreText = item.LoreText.Substring(0, markerIndex).TrimEnd('\n', '\r', ' ');
            }

            string additions = "";

            Rule? match = FallenUtils.MatchFilterRule(item);
            if (match != null && (ItemList.isEquipment(item.itemType) || ItemList.isIdol(item.itemType)))
            {
                var description = match.GetRuleDescription();
                if (!string.IsNullOrEmpty(description))
                {
                    additions += $"\n\nFilterRule : {description}";
                }
            }

            ItemDataUnpacked? matchedUniqueOrSet = FallenUtils.FindSimilarUniqueItemInStash(item);
            if (matchedUniqueOrSet != null)
            {
                int ownedLP = matchedUniqueOrSet.legendaryPotential;
                string LPdesc = ownedLP > item.legendaryPotential ? $"higher LP : {ownedLP}" :
                                ownedLP < item.legendaryPotential ? $"lower LP : {ownedLP}" :
                                matchedUniqueOrSet.Equals(item) ? "Self" : $"same LP ({ownedLP})";
                additions += $"\n\nAlready Owned with {LPdesc}";
            }
            else if (item.isUniqueSetOrLegendary())
            {
                additions += "\n\nNot Owned";
            }

            if (!string.IsNullOrEmpty(additions))
            {
                item.LoreText = item.LoreText + LoreMarker + additions;
            }
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