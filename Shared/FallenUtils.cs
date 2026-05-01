using System.Diagnostics;
using System.Runtime.CompilerServices;
using Il2Cpp;
using Il2CppItemFiltering;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rule = Il2CppItemFiltering.Rule;

namespace Fallen_LE_Mods.Shared
{

    //Maybe this should be split, idk..
    public static class FallenUtils
    {
        public static HarmonyLib.Harmony? Harmony;
        public static void Warning(string msg)
        {
            Melon<MyMod>.Logger.Warning(msg);
        }
        public static void BigError(string msg)
        {
            Melon<MyMod>.Logger.BigError(msg);
        }
        public static void Error(string msg)
        {
            Melon<MyMod>.Logger.Error(msg);
        }
        public static void Log(string msg)
        {
            Melon<MyMod>.Logger.Msg(msg);
        }

        [Conditional("RELEASE")]
        public static void LogDebug(string msg,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "")
        {
            string className = System.IO.Path.GetFileNameWithoutExtension(filePath);

            Melon<MyMod>.Logger.Msg($"[{className}.{methodName}] {msg}");
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

            var filterManager = GameReferencesCache.ItemFilterManager.Value;
            if (filterManager == null || filterManager.Filter == null || filterManager.Filter.rules == null)
            {
                return null;
            }

            var rules = filterManager.Filter.rules;
            int level = (GameReferencesCache.ExpTracker.Value != null) ? GameReferencesCache.ExpTracker.Value.CurrentLevel : 0;

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

        public static ItemDataUnpacked? FindSimilarUniqueItemInStash(ItemDataUnpacked _item, bool preferWW)
        {
            if (!_item.isUniqueSetOrLegendary() || GameReferencesCache.PlayerStash.Value == null) return null;

            ItemDataUnpacked? bestMatch = null;
            int bestValue = -1;

            foreach (ItemContainer stashtab in GameReferencesCache.PlayerStash.Value)
            {
                foreach (ItemContainerEntry itemEntry in stashtab.content)
                {
                    var data = itemEntry.data;
                    if (data == null) continue;

                    if (data.isUniqueSetOrLegendary() && data.uniqueID == _item.uniqueID)
                    {
                        int currentValue = preferWW ? data.weaversWill : data.legendaryPotential;

                        if (bestMatch == null || currentValue > bestValue)
                        {
                            bestValue = currentValue;
                            bestMatch = data.getAsUnpacked();
                        }
                    }
                }
            }
            return bestMatch;
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

    public static class GameObjectExtensions
    {
        public static GameObject? GetChildByName(this GameObject parent, string name)
        {
            if (parent == null) return null;
            Transform t = parent.transform.Find(name);
            if (t != null) return t.gameObject;

            for (int i = 0; i < parent.transform.childCount; i++)
            {
                var childTransform = parent.transform.GetChild(i);
                if (childTransform.name == name) return childTransform.gameObject;
                var found = childTransform.gameObject.GetChildByName(name);
                if (found != null) return found;
            }

            return null;
        }

    }

    internal static class Scenes
    {
        private static readonly string[] SceneMenuNames = { "ClientSplash", "PersistentUI", "Login", "CharacterSelectScene" };

        public static bool IsGameScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            return scene.IsValid() && System.Array.IndexOf(SceneMenuNames, scene.name) < 0;
        }

    }
}
