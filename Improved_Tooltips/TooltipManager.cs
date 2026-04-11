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
            bool hasLore = !string.IsNullOrEmpty(item.LoreText);

            void AppendAddition(string content)
            {
                if (string.IsNullOrEmpty(content)) return;

                if (string.IsNullOrEmpty(additions))
                {
                    additions += hasLore ? $"\n\n{content}" : content;
                }
                else
                {
                    additions += $"\n\n{content}";
                }
            }

            Rule? match = FallenUtils.MatchFilterRule(item);
            if (match != null && (ItemList.isEquipment(item.itemType) || ItemList.isIdol(item.itemType)))
            {
                var description = match.GetRuleDescription();
                if (!string.IsNullOrEmpty(description))
                {
                    AppendAddition($"<color=#E0E0E0>Filter Rule:</color> {description}");
                }
            }

            if (item.isUniqueSetOrLegendary())
            {
                string uniqueText = "";
                bool isWW = item.weaversWill > 0;
                ItemDataUnpacked? matchedInStash = FallenUtils.FindSimilarUniqueItemInStash(item, isWW);

                if (matchedInStash != null && matchedInStash.Pointer != item.Pointer)
                {
                    if (item.isSet())
                    {
                        uniqueText = "<color=#00FF00>[OWNED - IN STASH]</color>";
                    }
                    else
                    {
                        int currentVal = isWW ? item.weaversWill : item.getLegendaryPotentialTier();
                        int stashVal = isWW ? matchedInStash.weaversWill : matchedInStash.legendaryPotential;
                        string statColor = isWW ? "#5D3FD3" : "#FF0000";
                        string statName = isWW ? "WW" : "LP";

                        string comparison = stashVal > currentVal
                            ? $"<color=#FF0000>↓</color> (Stash has <color={statColor}>{statName}:{stashVal}</color>)"
                            : stashVal < currentVal
                            ? $"<color=#00FF00>↑</color> (Stash has <color={statColor}>{statName}:{stashVal}</color>)"
                            : $"<color=#0000FF>=</color> (Stash has <color={statColor}>{statName}:{stashVal}</color>)";

                        uniqueText = $"<color=#00FF00>[OWNED]</color> - <color={statColor}>[{statName}:{currentVal}]</color> {comparison}";
                    }
                }
                else if (matchedInStash != null && matchedInStash.Pointer == item.Pointer)
                {
                    if (item.isSet())
                    {
                        uniqueText = "<color=#00FF00>[OWNED - IN STASH (Self)]</color>";
                    }
                    else
                    {
                        string statName = isWW ? "WW" : "LP";
                        string diamond = "<rotate=45><voffset=0.2em><size=80%>■</size></voffset></rotate>";
                        uniqueText = $"<color=#FFD700>{diamond} BEST {statName} IN STASH {diamond}</color>";
                    }
                }
                else
                {
                    uniqueText = "<i><color=#FFD700>[NEW - NOT IN STASH]</color></i>";
                }

                AppendAddition(uniqueText);
            }

            if (!string.IsNullOrEmpty(additions))
            {
                item.LoreText = item.LoreText + LoreMarker + additions;
            }
        }

        [HarmonyPatch(typeof(TooltipItemManager), "CreateTooltipContent", new Type[] { typeof(TooltipItemManager.ItemTooltipParameters) })]
        public class TooltipItemManagerPatch
        {
            static void Prefix(TooltipItemManager.ItemTooltipParameters parameters)
            {
                if (parameters == null) return;
                HandleTooltipUpdate(parameters.Item);
            }
        }
    }
}