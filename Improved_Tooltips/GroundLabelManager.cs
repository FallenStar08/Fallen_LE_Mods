using System.Collections;
using Fallen_LE_Mods.Shared;
using Fallen_LE_Mods.Shared.UI;
using Il2Cpp;
using Il2CppRewired.Utils;
using Il2CppTMPro;
using MelonLoader;

namespace Fallen_LE_Mods.Improved_Tooltips
{
    public class GroundLabelManager
    {
        private static bool? _isKgImprovementsLoaded;
        public static bool IsKgImprovementsLoaded => _isKgImprovementsLoaded ??=
            MelonMod.RegisteredMelons.Any(m => m.Info.Name == "kg_LastEpoch_Improvements");
        public static MelonPreferences_Category? _category;
        public static MelonPreferences_Entry<bool>? _prefShowFullItemName;
        public static MelonPreferences_Entry<bool>? _prefShowLPOnGroundLabels;
        public static MelonPreferences_Entry<bool>? _prefShowLPComparison;
        private static bool running = false;
        public static void Initialize()
        {
            if (running) return;
            _category = MelonPreferences.CreateCategory("ImprovedTooltips", "Improved Tooltips");
            _category.SetFilePath("UserData/FallenImprovedTooltips.cfg");

            string description1 = IsKgImprovementsLoaded
                ? "Show Item's LP on Ground Labels (Disabled due to kg Improvements providing this info)"
                : "Show Item's LP on Ground Labels";

            string description2 = "Show LP Comparison to Items in Stash on Ground Labels";

            string description3 = IsKgImprovementsLoaded
                ? "Show Full Item Name on their ground label (Disabled due to kg Improvements providing this info)"
                : "Show Full Item Name on their ground label";

            _prefShowLPOnGroundLabels = _category.CreateEntry("ShowLPOnGroundLabels", true, description1);
            _prefShowLPComparison = _category.CreateEntry("ShowLPComparison", true, description2);
            _prefShowFullItemName = _category.CreateEntry("ShowFullItemName", true, description3);

            FallenUI.RegisterMenu((container) =>
            {
                var header = FallenUI.CreateHeader(container, $"Improved Tooltips v{BuildInfo.Version}", "GroundLabels");
                if (header == null) return;
                if (VersionChecker.UpdateAvailable)
                {
                    FallenUI.CreateUpdateNotice(container, VersionChecker.LatestVersion);
                }
                FallenUI.CreateToggle(container, "Show LP On Ground Labels", description1, _prefShowLPOnGroundLabels);
                FallenUI.CreateToggle(container, "Show LP Comparison On Ground Labels", description2, _prefShowLPComparison);
                FallenUI.CreateToggle(container, "Show Full Item Name On Ground Labels", description3, _prefShowFullItemName);
            });
            running = true;
        }

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
                    if (itemData.IsNullOrDestroyed() || !itemData.isUniqueSetOrLegendary()) yield break;
                }
                catch { yield break; }

                TextMeshProUGUI itemText = item.itemText;
                if (itemText.IsNullOrDestroyed()) yield break;

                string currentText = itemText.text;
                if (currentText.Contains(LabelMarker)) yield break;

                string baseName = currentText;
                string statsDisplay = "";
                string statusSuffix = "";

                bool isSet = itemData.isSet();

                if (!IsKgImprovementsLoaded)
                {
                    baseName = _prefShowFullItemName.Value ? itemData.FullName : currentText;

                    if (!isSet & _prefShowLPOnGroundLabels.Value == true)
                    {
                        statsDisplay = itemData.weaversWill > 0
                            ? $" <color=#5D3FD3>[WW:{itemData.weaversWill}]</color>"
                            : $" <color=#FF0000>[LP:{itemData.legendaryPotential}]</color>";
                    }
                }

                ItemDataUnpacked matchedInStash = FallenUtils.FindSimilarUniqueItemInStash(itemData, itemData.weaversWill > 0);

                if (matchedInStash != null)
                {
                    if (isSet)
                    {
                        statusSuffix = " <color=#00FF00>[OWNED]</color>";
                    }
                    else if (_prefShowLPComparison.Value == true)
                    {
                        int currentStat = (itemData.weaversWill > 0) ? itemData.weaversWill : itemData.legendaryPotential;
                        int stashStat = (itemData.weaversWill > 0) ? matchedInStash.weaversWill : matchedInStash.legendaryPotential;

                        statusSuffix = stashStat > currentStat ? " <color=#FF0000>↓</color>" :
                                       stashStat < currentStat ? " <color=#00FF00>↑</color>" :
                                                                 " <color=#0000FF>=</color>";
                    }
                }
                else
                {
                    statusSuffix = " <i><color=#FFD700>NEW</color></i>";
                }

                string newFullText = $"{baseName}{statsDisplay}{statusSuffix}{LabelMarker}";

                if (item.emphasized) newFullText = newFullText.ToUpper();

                itemText.text = newFullText;
                item.sceneFollower?.calculateDimensions();
            }
        }
    }
}