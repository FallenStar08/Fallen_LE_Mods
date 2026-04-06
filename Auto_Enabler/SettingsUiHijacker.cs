using Fallen_LE_Mods.Shared;
using HarmonyLib;
using Il2Cpp;
using Il2CppRewired.Utils;
using UnityEngine;
using UnityEngine.UI;
using static Fallen_LE_Mods.Shared.FallenUtils;

namespace Fallen_LE_Mods.Auto_Enabler
{
    [HarmonyPatch(typeof(SettingsUIManager), nameof(SettingsUIManager.EnableSocialTab))]
    public class SettingsPanel_SocialTab_Patch
    {
        public static void Postfix(SettingsUIManager __instance)
        {
            try
            {
                Transform social = __instance.transform.Find("Content/Social");
                Transform viewport = social.Find("Viewport");
                Transform socialContainer = viewport.Find("Social-container");
                if (social.IsNullOrDestroyed() || viewport.IsNullOrDestroyed() || socialContainer.IsNullOrDestroyed()) return;
                var scrollRect = social.GetComponent<ScrollRect>() ?? social.gameObject.AddComponent<ScrollRect>();
                scrollRect.scrollSensitivity = 20f;
                scrollRect.horizontal = false;
                scrollRect.content = socialContainer.GetComponent<RectTransform>();

                if (socialContainer.Find("FallenHeader_MainHeader") != null) return;
                //Main Header
                FallenUI.CreateHeader(socialContainer, "Fallen's Proximity Manager Settings", "MainHeader");

                //Toggle For Ring Visual
                FallenUI.CreateToggle(socialContainer, "Show Proximity Rings",
                             "Visual colored circles around shrines and chests.",
                             UniversalProximityManager._prefShowRings);

                //Slider for Activation Radius
                FallenUI.CreateSlider(socialContainer, "Activation Radius",
                             "Distance at which proximity activation occurs.", 1.0f, 10.0f,
                             UniversalProximityManager._prefDistance);

                //Sub-Header for Filters
                FallenUI.CreateHeader(socialContainer, "Auto-Activation Filters", "Filters");

                //Filter Toggles
                foreach (var entry in UniversalProximityManager.TypeToggles)
                {
                    FallenUI.CreateToggle(socialContainer, $"Auto-Activate {entry.Key}s",
                                 $"Enable or disable proximity activation for {entry.Key} objects.",
                                 entry.Value);
                }

                Log("[UI Hijack] Successfully injected all FallenMods settings! :3");
            }
            catch (Exception e) { Log($"[UI Hijack] Error: {e.Message}"); }
        }

    }
}