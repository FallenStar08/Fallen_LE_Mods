using System.Collections;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.UI.Minimap;
using MelonLoader;
using UnityEngine;
using static Fallen_LE_Mods.Shared.FallenUtils;

namespace Fallen_LE_Mods.Dev.Visuals
{
    public static class VisualFixes
    {
        private static bool _isEnabled = true;
        [HarmonyPatch(typeof(Minimap), "OnInitializeFoW")]
        public class MinimapPatch
        {
            public static void Postfix(Minimap __instance)
            {
                if (!_isEnabled) return;

                __instance.RevealRadius = 5000f;

                IEnumerator DelayReset()
                {
                    yield return null;
                    yield return null;
                    if (__instance != null) __instance.RevealRadius = 40f;
                }

                MelonCoroutines.Start(DelayReset());
            }
        }

        [HarmonyPatch(typeof(ClientSceneService), "OnActiveSceneChanged")]
        public class RainScenePatch
        {
            public static void Postfix()
            {
                if (!_isEnabled) return;
                MelonCoroutines.Start(DelayedRainCheck());
            }
        }

        private static IEnumerator DelayedRainCheck()
        {
            yield return new WaitForSeconds(0.5f);

            GameObject rainObj = GameObject.Find("SceneObject/Visuals/Lighting/Rain");

            if (rainObj == null)
            {
                GameObject lighting = GameObject.Find("Lighting");
                if (lighting != null)
                {
                    var child = lighting.transform.Find("Rain");
                    if (child != null) rainObj = child.gameObject;
                }
            }

            if (rainObj != null)
            {

                rainObj.SetActive(false);
                Log("[Visuals] Rain taken out back and shot ☔ -> 🌞");
            }
        }

        public static void ToggleRain(bool state)
        {
            _isEnabled = state;
            GameObject rain = GameObject.Find("Rain");
            rain?.SetActive(state);
        }
    }
}