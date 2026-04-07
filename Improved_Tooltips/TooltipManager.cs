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
        public static MelonPreferences_Category? _category;
        public static MelonPreferences_Entry<bool>? _prefShowFullItemName;

        private static void HandleTooltipUpdate(ItemDataUnpacked item)
        {
            if (item == null) return;
            item.LoreText ??= "";

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
                    additions += $"\n\n<color=#E0E0E0>Filter Rule:</color> {description}";
                }
            }

            if (item.isUniqueSetOrLegendary())
            {
                bool isWW = item.weaversWill > 0;
                ItemDataUnpacked? matchedInStash = FallenUtils.FindSimilarUniqueItemInStash(item, isWW);

                if (matchedInStash != null && matchedInStash.Pointer != item.Pointer)
                {
                    if (item.isSet())
                    {
                        additions += "\n\n<color=#00FF00>[OWNED - IN STASH]</color>";
                    }
                    else
                    {
                        int currentVal = isWW ? item.weaversWill : item.legendaryPotential;
                        int stashVal = isWW ? matchedInStash.weaversWill : matchedInStash.legendaryPotential;

                        string statColor = isWW ? "#5D3FD3" : "#FF0000";
                        string statName = isWW ? "WW" : "LP";

                        string comparison = stashVal > currentVal
                            ? $"<color=#FF0000>↓</color> (Stash has <color={statColor}>{statName}:{stashVal}</color>)"
                            : stashVal < currentVal
                            ? $"<color=#00FF00>↑</color> (Stash has <color={statColor}>{statName}:{stashVal}</color>)"
                            : $"<color=#0000FF>=</color> (Stash has <color={statColor}>{statName}:{stashVal}</color>)";
                        additions += $"\n\n<color=#00FF00>[OWNED]</color> - <color={statColor}>[{statName}:{currentVal}]</color> {comparison}";
                    }
                }
                else
                {
                    if (matchedInStash != null && matchedInStash.Pointer == item.Pointer)
                    {
                        if (item.isSet())
                        {
                            additions += "\n\n<color=#00FF00>[OWNED - IN STASH (Self)]</color>";
                        }
                        else
                        {
                            string statName = isWW ? "WW" : "LP";
                            string diamond = "<rotate=45><voffset=0.2em><size=80%>■</size></voffset></rotate>";
                            additions += $"\n\n<color=#FFD700>{diamond} BEST {statName} IN STASH {diamond}</color>";
                        }
                    }
                    else
                    {
                        additions += "\n\n<i><color=#FFD700>[NEW - NOT IN STASH]</color></i>";
                    }
                }
            }

            if (!string.IsNullOrEmpty(additions))
            {
                item.LoreText = item.LoreText + LoreMarker + additions;
            }
        }

        [HarmonyPatch(typeof(TooltipItemManager), "OpenItemTooltip", new Type[] { typeof(ItemDataUnpacked), typeof(TooltipItemManager.SlotType), typeof(Vector2), typeof(Vector3), typeof(GameObject), typeof(Vector2), })]
        public class TooltipItemManagerPatch
        {
            static void Prefix(ItemDataUnpacked data)
            {
                if (data == null) return;
                HandleTooltipUpdate(data);
            }
        }
    }
}