using System.Collections;
using Fallen_LE_Mods.Shared;
using Fallen_LE_Mods.Shared.UI;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Factions;
using Il2CppRewired.Utils;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.EventSystems;
using static Fallen_LE_Mods.Shared.FallenUtils;

namespace Fallen_LE_Mods.Improved_Observatory
{
    public static class ObservatoryManager
    {
        public static Dictionary<ProphecyRegion, Il2CppSystem.Collections.Generic.IEnumerable<ConstellationStar>> AllRegionsStars { get; private set; } = new();
        public static ProphecyRegion ObservedRegion { get; private set; }
        public static string? CurrentSearchQuery { get; set; }


        [HarmonyPatch(typeof(Constellation), "Init")]
        public class ProphecyInitPatch
        {
            private static void Postfix(Constellation __instance, ObservatoryUI panel)
            {
                if (panel.IsNullOrDestroyed() || __instance.IsNullOrDestroyed()) return;

                //QOL faster refresh
                panel._constellationFadeInDuration = 0f;
                panel._constellationFadeOutDuration = 0f;
                panel._starFadeDuration = 0f;

                //Data sync point
                AllRegionsStars[__instance.region] = __instance.ActiveStars;

                //Epic quick buy button
                Transform? rerollParent = panel.gameObject?.transform.Find("Reroll");
                if (rerollParent != null)
                {
                    string btnName = "FallenQuickBuyButton";
                    Transform? existingBtn = rerollParent.parent.Find(btnName);
                    if (existingBtn == null)
                    {
                        GameObject quickBuyObj = UnityEngine.Object.Instantiate(rerollParent.gameObject, rerollParent.parent);
                        quickBuyObj.name = btnName;
                        quickBuyObj.GetChildByName("TextMeshPro Text").SmartDestroy();
                        RectTransform rect = quickBuyObj.GetComponent<RectTransform>();
                        rect.anchoredPosition += new Vector2(-220f, 0f);

                        var text = quickBuyObj.GetComponentInChildren<TextMeshProUGUI>();
                        if (text != null)
                        {
                            text.text = "Buy Match";
                            text.color = new Color(0.2f, 1f, 0.2f);
                        }

                        var btn = quickBuyObj.GetComponent<UnityEngine.UI.Button>();
                        if (btn != null)
                        {
                            //Remove the old single-click logic
                            btn.onClick.RemoveAllListeners();

                            var trigger = quickBuyObj.GetComponent<EventTrigger>() ?? quickBuyObj.AddComponent<EventTrigger>();
                            trigger.delegates.Clear();

                            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                            pointerDown.callback.AddListener(new Action<BaseEventData>((e) =>
                            {
                                //Always do one immediate click first
                                ObservatoryHelpers.BuyFirstMatchOrReroll(panel);
                                //Start the hold-to-spam routine...
                                MelonCoroutines.Start(ObservatoryHelpers.AutoSnipeRoutine(panel));
                            }));
                            trigger.delegates.Add(pointerDown);

                            //Pointer Up (Stop Spamming)
                            var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                            pointerUp.callback.AddListener(new Action<BaseEventData>((e) =>
                            {
                                ObservatoryHelpers.IsSniperHeld = false;
                            }));
                            trigger.delegates.Add(pointerUp);

                            //Pointer Exit (Stop spamming)
                            var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                            pointerExit.callback.AddListener(new Action<BaseEventData>((e) =>
                            {
                                ObservatoryHelpers.IsSniperHeld = false;
                            }));
                            trigger.delegates.Add(pointerExit);
                        }
                    }
                }


                //Search box shenanigans
                Transform? configParent = panel.gameObject?.transform.Find("ObservatoryConfig");
                if (configParent != null)
                {
                    var searchBox = FallenUI.CreateSearchBox(configParent, "FallenProphecySearch", new Action<string>(ObservatoryHelpers.FilterStars));
                    RectTransform? rect = searchBox?.GetComponent<RectTransform>();
                    if (VersionChecker.UpdateAvailable && !string.IsNullOrEmpty(VersionChecker.LatestVersion))
                    {
                        var updateNotice = FallenUI.CreateUpdateNotice(configParent, VersionChecker.LatestVersion);
                        RectTransform? updateRect = updateNotice?.GetComponent<RectTransform>();

                        if (updateRect != null)
                        {
                            updateRect.SetParent(panel.transform, false);

                            updateRect.anchorMin = new Vector2(1f, 0f);
                            updateRect.anchorMax = new Vector2(1f, 0f);
                            updateRect.pivot = new Vector2(1f, 0f);

                            updateRect.anchoredPosition = new Vector2(-20f, 20f);
                        }
                    }
                    if (rect != null)
                    {
                        rect.anchorMin = new Vector2(0.5f, 1f);
                        rect.anchorMax = new Vector2(0.5f, 1f);

                        rect.pivot = new Vector2(0.5f, 0f);

                        rect.anchoredPosition = new Vector2(0f, 5f);

                    }
                }
                ClearInput(panel);
            }

        }



        [HarmonyPatch(typeof(ConstellationStar), nameof(ConstellationStar.Init))]
        public class StarInitPatch
        {
            public static void Postfix(ConstellationStar __instance, ProphecyRegion region, Prophecy prophecy, int index, bool previewEnabled)
            {
                if (__instance.IsNullOrDestroyed() || __instance.currentProphecy == null) return;


                //ensure we have a reference to the latest constellation stars
                var constellation = __instance.GetComponentInParent<Constellation>();
                if (constellation != null)
                {
                    AllRegionsStars[region] = constellation.ActiveStars;
                }

                //filter with a 1-frame delay
                MelonCoroutines.Start(DelayedStarFilter(__instance));
            }
        }

        private static IEnumerator DelayedStarFilter(ConstellationStar star)
        {
            yield return null;

            if (star.IsNullOrDestroyed()) yield break;

            var searchBox = GameObject.Find("FallenProphecySearch");
            var input = searchBox?.GetComponentInChildren<TMP_InputField>();

            bool isMatch = ObservatoryHelpers.IsFuzzyMatch(input?.text ?? "", star);
            star.transform.localScale = isMatch ? Vector3.one : Vector3.zero;
        }



        [HarmonyPatch(typeof(ObservatoryUI), "Update")]
        public class ObservatoryUpdatePatch
        {
            public static void Postfix(ObservatoryUI __instance)
            {
                if (__instance.IsNullOrDestroyed()) return;

                if (__instance.CurrentRegion != ObservedRegion)
                {
                    ObservedRegion = __instance.CurrentRegion;
                    OnRegionChanged(__instance);
                }
            }
        }
        private static void ClearInput(ObservatoryUI panel)
        {
            var input = panel.gameObject.transform.Find("ObservatoryConfig/FallenProphecySearch")?.GetComponentInChildren<TMP_InputField>();
            if (input != null)
            {
                input.text = "";
                LogDebug($"[ObservatoryManager] Input Cleared");
            }
        }
        private static void OnRegionChanged(ObservatoryUI panel)
        {
            var input = panel.gameObject.transform.Find("ObservatoryConfig/FallenProphecySearch")?.GetComponentInChildren<TMP_InputField>();
            if (input != null)
            {
                input.text = "";
                LogDebug($"[ObservatoryManager] Region changed to {ObservedRegion}, clearing search.");
            }
        }


    }
}


