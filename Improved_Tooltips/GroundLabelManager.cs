using System.Collections;
using Fallen_LE_Mods.Shared;
using Il2Cpp;
using Il2CppRewired.Utils;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;

namespace Fallen_LE_Mods.Improved_Tooltips
{
    public class GroundLabelManager : MelonMod
    {
        private static bool? _isKgImprovementsLoaded;
        public static bool IsKgImprovementsLoaded => _isKgImprovementsLoaded ??=
            MelonMod.RegisteredMelons.Any(m => m.Info.Name == "kg_LastEpoch_Improvements");
        public static class GroundLabelPatch
        {
            public static void Postfix(GroundItemLabel __instance)
            {
                MelonCoroutines.Start(DelayRoutine(__instance));
            }

            private static IEnumerator DelayRoutine(GroundItemLabel item)
            {
                const string LabelMarker = "\u200B\u200B\u200B";
                yield return null;

                if (item.IsNullOrDestroyed()) yield break;

                ItemDataUnpacked itemData;
                try
                {
                    itemData = item.getItemData();
                    if (itemData.IsNullOrDestroyed()) yield break;
                }
                catch { yield break; }

                TextMeshProUGUI itemText = item.itemText;
                if (itemText.IsNullOrDestroyed()) yield break;

                string currentText = itemText.text;
                if (currentText.Contains(LabelMarker)) yield break;

                if (itemData.isUnique())
                {
                    string statsDisplay = "";
                    if (!IsKgImprovementsLoaded)
                    {
                        //WW>LP
                        if (itemData.weaversWill > 0)
                        {
                            statsDisplay = $" <color=#5D3FD3>[WW:{itemData.weaversWill}]</color>";
                        }
                        else
                        {
                            statsDisplay = $" <color=#FF0000>[LP:{itemData.legendaryPotential}]</color>";
                        }
                    }
                    string comparisonSymbol = "";

                    ItemDataUnpacked matchedInStash = FallenUtils.FindSimilarUniqueItemInStash(itemData);

                    if (matchedInStash != null)
                    {
                        int currentStat = (itemData.weaversWill > 0) ? itemData.weaversWill : itemData.legendaryPotential;
                        int stashStat = (itemData.weaversWill > 0) ? matchedInStash.weaversWill : matchedInStash.legendaryPotential;

                        comparisonSymbol = stashStat > currentStat ? " <color=#FF0000>↓</color>" :
                                           stashStat < currentStat ? " <color=#00FF00>↑</color>" :
                                                                     " <color=#0000FF>=</color>";
                    }
                    else if (itemData.isUniqueSetOrLegendary())
                    {
                        comparisonSymbol = " <i><color=#FFD700>NEW</color></i>";
                    }

                    string newFullText = $"{currentText}{statsDisplay}{comparisonSymbol}{LabelMarker}";

                    if (item.emphasized) newFullText = newFullText.ToUpper();

                    itemText.text = newFullText;
                    item.sceneFollower?.calculateDimensions();
                }
            }
        }
    }
}