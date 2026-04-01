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
                    int currentLP = itemData.legendaryPotential;
                    string lpDisplay = !IsKgImprovementsLoaded
                        ? $" <color=#FF0000>[LP:{itemData.legendaryPotential}]</color>"
                        : "";
                    string wwDisplay = !IsKgImprovementsLoaded
                        ? $" <color=#5D3FD3>[WW:{itemData.weaversWill}]</color>"
                        : "";
                    string comparisonSymbol = "";

                    ItemDataUnpacked matchedInStash = FallenUtils.FindSimilarUniqueItemInStash(itemData);

                    if (matchedInStash != null)
                    {
                        int stashLP = matchedInStash.legendaryPotential;

                        comparisonSymbol = stashLP > currentLP ? " <color=#FF0000>↓</color>" :
                                         stashLP < currentLP ? " <color=#00FF00>↑</color>" :
                                                               " <color=#0000FF>=</color>";
                    }
                    else if (itemData.isUniqueSetOrLegendary())
                    {
                        comparisonSymbol = " <i><color=#FFD700>NEW</color></i>";
                    }

                    string newFullText = $"{currentText}{wwDisplay}{lpDisplay}{comparisonSymbol}{LabelMarker}";

                    if (item.emphasized) newFullText = newFullText.ToUpper();

                    itemText.text = newFullText;
                    item.sceneFollower?.calculateDimensions();
                }
            }
        }
    }
}