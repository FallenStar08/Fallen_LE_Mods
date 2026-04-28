using System.Collections;
using Fallen_LE_Mods.Shared;
using Il2CppLE.Factions;
using Il2CppRewired.Utils;
using UnityEngine;
using static Fallen_LE_Mods.Shared.FallenUtils;

namespace Fallen_LE_Mods.Improved_Observatory
{
    public static class ObservatoryHelpers
    {
        public static bool IsSniperHeld = false;
        private const float AutoSnipeInitialDelay = 0.5f;
        private const float AutoSnipeInterval = 0.2f;

        public static IEnumerator AutoSnipeRoutine(ObservatoryUI panel)
        {
            IsSniperHeld = true;

            yield return new WaitForSeconds(AutoSnipeInitialDelay);

            while (IsSniperHeld)
            {
                if (panel.IsNullOrDestroyed() || !panel.gameObject.activeInHierarchy) break;

                BuyFirstMatchOrReroll(panel);

                yield return new WaitForSeconds(AutoSnipeInterval);
            }
        }
        public static void BuyFirstMatchOrReroll(ObservatoryUI panel)
        {
            if (panel.IsNullOrDestroyed()) return;

            string currentQuery = ObservatoryManager.CurrentSearchQuery;

            if (ObservatoryManager.AllRegionsStars.TryGetValue(panel.CurrentRegion, out var stars) && stars != null)
            {
                var enumerator = stars.GetEnumerator();
                var baseEnumerator = enumerator.Cast<Il2CppSystem.Collections.IEnumerator>();

                while (baseEnumerator.MoveNext())
                {
                    var star = baseEnumerator.Current?.TryCast<ConstellationStar>();
                    if (star != null && star.button != null)
                    {

                        bool isMatch = IsFuzzyMatch(currentQuery, star);

                        if (isMatch)
                        {

                            if (int.Parse(star.favorText.text) > GameReferencesCache.faction.Favor)
                            {
                                MakeNotification($"Too poor to buy {getStarReward(star)}");
                                LogDebug($"[ObservatoryHelpers] Too poor to buy {getStarReward(star)}");
                                return;
                            }
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

            var keywords = System.Text.RegularExpressions.Regex.Matches(query.ToLower(), @"(\w+):\""(.*?)""|quantity:(\d+)|\""(.*?)\""|(\S+)")
                .Cast<System.Text.RegularExpressions.Match>()
                .Select(m => m.Value)
                .ToList();

            if (keywords.Count == 0) return true;

            string reward = (getStarReward(star) ?? "").ToLower().Trim();
            string condition = (getStarCondition(star) ?? "").ToLower().Trim();
            string combinedData = $"{reward} {condition}";

            foreach (var word in keywords)
            {
                //Quantity Search (e.g. quantity:5)
                if (word.StartsWith("quantity:") && word.Length > 9)
                {
                    string targetQty = word.Substring(9);
                    var match = System.Text.RegularExpressions.Regex.Match(reward, @"x(\d+)$");

                    if (!match.Success || match.Groups[1].Value != targetQty) return false;
                }
                //Exact Reward Match
                else if (word.StartsWith("reward:\"") && word.EndsWith("\"") && word.Length >= 9)
                {
                    string exactValue = word.Substring(8, word.Length - 9);
                    string pattern = $@"^{System.Text.RegularExpressions.Regex.Escape(exactValue)}(\s+x\d+)?$";
                    if (!System.Text.RegularExpressions.Regex.IsMatch(reward, pattern)) return false;
                }
                //Exact Match (General) probably useless
                else if (word.StartsWith("\"") && word.EndsWith("\"") && word.Length >= 2)
                {
                    string exactWord = word.Substring(1, word.Length - 2);
                    string pattern = $@"\b{System.Text.RegularExpressions.Regex.Escape(exactWord)}\b";
                    if (!System.Text.RegularExpressions.Regex.IsMatch(combinedData, pattern)) return false;
                }
                //NOT
                else if (word.StartsWith("-") && word.Length > 1)
                {
                    string excludeWord = word.Substring(1);
                    if (combinedData.Contains(excludeWord)) return false;
                }
                //OR
                else if (word.Contains('|'))
                {
                    string[] orParts = word.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (!orParts.Any(part => combinedData.Contains(part))) return false;
                }
                //AND
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

            ObservatoryManager.CurrentSearchQuery = query;
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
