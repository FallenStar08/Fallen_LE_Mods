using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;
using MelonLoader;
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
                Transform viewport = __instance.transform.Find("Content/Social/Viewport");
                if (viewport == null) return;
                Transform socialList = viewport.Find("Social-container");
                if (socialList == null) return;

                if (socialList.Find("FallenHeader_MainHeader") != null) return;

                //Main Header
                CreateHeader(socialList, "Fallen's Proximity Manager Settings", "MainHeader");

                //Toggle For Ring Visual
                CreateToggle(socialList, "Show Proximity Rings",
                             "Visual colored circles around shrines and chests.",
                             UniversalProximityManager._prefShowRings);

                //Sub-Header for Filters
                CreateHeader(socialList, "Auto-Activation Filters", "Filters");

                //Filter Toggles
                foreach (var entry in UniversalProximityManager.TypeToggles)
                {
                    CreateToggle(socialList, $"Auto-Activate {entry.Key}s",
                                 $"Enable or disable proximity activation for {entry.Key} objects.",
                                 entry.Value);
                }

                Log("[UI Hijack] Successfully injected all FallenMods settings! :3");
            }
            catch (Exception e) { Log($"[UI Hijack] Error: {e.Message}"); }
        }

        private static void CreateHeader(Transform parent, string title, string objectName)
        {
            Transform original = parent.Find("Header-Social");
            if (original == null) return;

            GameObject header = UnityEngine.Object.Instantiate(original.gameObject, parent);
            header.name = $"FallenHeader_{objectName}";

            //Cleanup Localization
            var textObj = header.GetComponentInChildren<TextMeshProUGUI>()?.gameObject;
            if (textObj != null)
            {
                foreach (var comp in textObj.GetComponents<MonoBehaviour>())
                    if (comp.GetIl2CppType().FullName.Contains("Localize")) UnityEngine.Object.Destroy(comp);

                var textComp = textObj.GetComponent<TextMeshProUGUI>();
                textComp.text = title.ToUpper();
                textComp.color = new Color(0.1f, 0.8f, 1f, 1f);
            }
        }

        private static void CreateToggle(Transform parent, string labelText, string sublabelText, MelonPreferences_Entry<bool> pref)
        {
            //EHG can't spell "Toggle" consistently in their UI :P
            //Surely this one toggle won't change its name across updates :copium:
            Transform original = parent.Find("Toogle - Profanity Filter");
            if (original == null) return;

            GameObject toggleGo = UnityEngine.Object.Instantiate(original.gameObject, parent);
            toggleGo.name = $"FallenToggle_{pref.Identifier}";

            foreach (var script in toggleGo.GetComponentsInChildren<MonoBehaviour>(true))
            {
                string fName = script.GetIl2CppType().FullName;
                if (fName.Contains("Settings") || fName.Contains("Localize")) UnityEngine.Object.Destroy(script);
            }

            var label = toggleGo.transform.Find("Input Labels/Label")?.GetComponent<TextMeshProUGUI>();
            var subLabel = toggleGo.transform.Find("Input Labels/Sublabel")?.GetComponent<TextMeshProUGUI>();
            if (label != null) label.text = labelText;
            if (subLabel != null) subLabel.text = sublabelText;

            var toggleComp = toggleGo.GetComponentInChildren<Toggle>();
            if (toggleComp != null)
            {
                toggleComp.onValueChanged.RemoveAllListeners();
                toggleComp.isOn = pref.Value;

                toggleComp.onValueChanged.AddListener(new Action<bool>((val) =>
                {
                    pref.Value = toggleComp.isOn;
                    UniversalProximityManager._category.SaveToFile();
                    Log($"[UI] {pref.Identifier} set to: {pref.Value}");
                }));
            }
        }
    }
}