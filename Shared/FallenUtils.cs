using Il2Cpp;
using Il2CppItemFiltering;
using MelonLoader;
using UnityEngine;
using Rule = Il2CppItemFiltering.Rule;

namespace Fallen_LE_Mods.Shared
{
    //Maybe this should be split, idk..
    public static class FallenUtils
    {

        public static void Log(string msg)
        {
            Melon<MyMod>.Logger.Msg(msg);
        }

        public static void MakeNotification(string msg)
        {
            Il2Cpp.Notifications.ShowGenericNotification(msg, 1, 1, 1);
        }

        public static void IncrementOrInitialize(Dictionary<string, int> dict, string key)
        {
            // Try to get the value for the key
            if (dict.TryGetValue(key, out int value))
            {
                // Key exists, increment the value
                dict[key]++;
            }
            else
            {
                // Key doesn't exist, initialize it to 1
                dict[key] = 1;
            }
        }

        /// <summary>
        /// find ItemFilterManager
        /// </summary>
        /// <returns>ItemFilterManager</returns>
        public static ItemFilterManager GetFilterManager
        {
            get
            {
                WorldOverlayCanvas WOC = WorldOverlayCanvas.instance;
                GameObject WOcanvas = WOC.gameObject;
                ItemFilterManager myManager = WOcanvas.GetComponent<ItemFilterManager>();
                return myManager;
            }
        }

        public static Rule? MatchFilterRule(ItemDataUnpacked _item, bool GetHighest = true)
        {
            if (_item == null) return null;

            var filterManager = GameReferencesCache.itemFilterManager;
            if (filterManager == null || filterManager.Filter == null || filterManager.Filter.rules == null)
            {
                return null;
            }

            var rules = filterManager.Filter.rules;
            int level = (GameReferencesCache.expTracker != null) ? GameReferencesCache.expTracker.CurrentLevel : 0;

            if (!GetHighest)
            {
                for (int i = 0; i < rules.Count; i++)
                {
                    Rule rule = rules[i];
                    if (rule != null && rule.isEnabled && rule.type.ToString() != "HIDE")
                    {
                        if (rule.Match(_item, level)) return rule;
                    }
                }
            }
            else
            {
                for (int i = rules.Count - 1; i >= 0; i--)
                {
                    Rule rule = rules[i];
                    if (rule != null && rule.isEnabled && rule.type.ToString() != "HIDE")
                    {
                        if (rule.Match(_item, level)) return rule;
                    }
                }
            }

            return null;
        }

        public static ItemDataUnpacked? FindSimilarUniqueItemInStash(ItemDataUnpacked _item)
        {
            if (!_item.isUniqueSetOrLegendary()) { return null; }
            ;
            if (GameReferencesCache.playerStash == null) { return null; }
            ItemDataUnpacked? highestLPmatch = null;
            foreach (ItemContainer stashtab in GameReferencesCache.playerStash)
            {
                foreach (ItemContainerEntry itemEntry in stashtab.content)
                {
                    //uniqueID 0 for non unique/sets
                    var data = itemEntry.data;
                    if (data.isUniqueSetOrLegendary() && _item.uniqueID == data.uniqueID)
                    {
                        if (highestLPmatch == null)
                        {
                            highestLPmatch = data.getAsUnpacked();

                        }
                        else
                        {
                            if (data.legendaryPotential > highestLPmatch.legendaryPotential)
                            {
                                highestLPmatch = data.getAsUnpacked();
                            }

                        }
                    }
                }

            }
            //FallenUtils.Log($"Returned highestLPmatch {highestLPmatch}");
            return highestLPmatch;
        }

        /// <summary>
        /// Make a rectransform start from top left
        /// </summary>
        /// <param name="rectTransform"></param>
        public static void SetDefaultRectTransformProperties(RectTransform rectTransform)
        {
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = Vector2.zero;
        }

    }

}
