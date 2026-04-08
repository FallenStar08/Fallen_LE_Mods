using Il2CppLE.Factions;
using Il2CppRewired.Utils;
using UnityEngine;
using static Fallen_LE_Mods.Shared.FallenUtils;

namespace Fallen_LE_Mods.Improved_Observatory
{
    public static class ObservatoryHelpers
    {
        public static void BuyFirstMatchOrReroll(ObservatoryUI panel)
        {
            if (panel.IsNullOrDestroyed()) return;


            if (ObservatoryManager.AllRegionsStars.TryGetValue(panel.CurrentRegion, out var stars) && stars != null)
            {
                var enumerator = stars.GetEnumerator();
                var baseEnumerator = enumerator.Cast<Il2CppSystem.Collections.IEnumerator>();

                while (baseEnumerator.MoveNext())
                {
                    var star = baseEnumerator.Current?.TryCast<ConstellationStar>();
                    //Check if we have a match currently visible
                    if (star != null && star.transform.localScale.x > 0.1f)
                    {
                        if (star.button != null)
                        {
                            LogDebug($"[ObservatoryHelpers] Match found: {getStarReward(star)}. Buying...");
                            star.button.Press();
                            return;
                        }
                    }
                }
            }

            //Fallback -> no match was found, trigger a manual Reroll
            LogDebug("[ObservatoryHelpers] No match found. Rerolling region...");
            panel.RerollCurrentRegion();
        }

        public static bool IsFuzzyMatch(string query, ConstellationStar star)
        {
            if (string.IsNullOrWhiteSpace(query)) return true;
            if (star == null || star.Pointer == IntPtr.Zero) return false;

            //Split by space for AND logic
            string[] keywords = query.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (keywords.Length == 0) return true;

            string reward = (getStarReward(star) ?? "").ToLower();
            string condition = (getStarCondition(star) ?? "").ToLower();
            string combinedData = $"{reward} {condition}";

            foreach (var word in keywords)
            {
                //NOT Logic (e.g. -word)
                if (word.StartsWith("-") && word.Length > 1)
                {
                    string excludeWord = word.Substring(1);
                    if (combinedData.Contains(excludeWord)) return false;
                }
                //OR Logic (e.g. word1|word2)
                else if (word.Contains("|"))
                {
                    string[] orParts = word.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    bool anyMatch = false;
                    foreach (var part in orParts)
                    {
                        if (combinedData.Contains(part))
                        {
                            anyMatch = true;
                            break;
                        }
                    }
                    if (!anyMatch) return false;
                }
                //AND Logic (default, e.g. word)
                else
                {
                    if (!combinedData.Contains(word)) return false;
                }
            }

            return true;
        }

        public static void FilterStars(string query)
        {
            if (!ObservatoryManager.AllRegionsStars.TryGetValue(ObservatoryManager.ObservedRegion, out var stars) || stars == null) return;

            var enumerator = stars.GetEnumerator();
            var baseEnumerator = enumerator.Cast<Il2CppSystem.Collections.IEnumerator>();

            while (baseEnumerator.MoveNext())
            {
                var star = baseEnumerator.Current?.TryCast<ConstellationStar>();
                if (star == null || star.Pointer == IntPtr.Zero || star.gameObject == null) continue;

                bool isMatch = IsFuzzyMatch(query, star);
                star.transform.localScale = isMatch ? Vector3.one : Vector3.zero;
            }
        }

        public static string getStarReward(ConstellationStar star)
        {
            return star?.currentProphecy?.Reward?.generatedRewardString ?? "Unknown Reward";
        }

        public static string getStarCondition(ConstellationStar star)
        {
            return star?.currentProphecy?.Target?.name ?? "Unknown Condition";
        }
    }
}
