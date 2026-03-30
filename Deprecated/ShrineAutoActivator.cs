using System.Collections;
using Fallen_LE_Mods.MonoScripts;
using Fallen_LE_Mods.Shared;
using Il2Cpp;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine;

namespace Fallen_LE_Mods.Dev
{
    public class ShrineAutoActivator
    {

        public static void OnSceneWasLoadedHandler(int buildIndex, string sceneName)
        {
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            FallenUtils.Log($"[Melon] Scene Loaded: {sceneName}");
            ShrineProximityManager.Initialize();

            GameObject shrineManager = GameObject.Find("Shrine Placement Manager");

            if (shrineManager == null)
            {
                FallenUtils.Log("[Warning] Shrine Placement Manager not found in scene.");
                return;
            }

            var shrines = shrineManager.GetComponentsInChildren<WorldObjectClickListener>(true);

            foreach (var shrine in shrines)
            {
                if (shrine == null)
                    continue;

                var go = shrine.gameObject;

                if (!go.name.ToLower().Contains("shrine"))
                    continue;

                if (go.GetComponent(Il2CppType.Of<ShrineAutoRegister>()) == null)
                {
                    go.AddComponent(Il2CppType.Of<ShrineAutoRegister>());
                    FallenUtils.Log($"[Register] Shrine found and registered: {go.name}");
                }
            }
        }


    }

    public static class ShrineProximityManager
    {
        private static readonly List<GameObject> shrines = new();
        private static GameObject player;
        private static bool running = false;

        public static void RegisterShrine(GameObject shrine)
        {
            if (!shrines.Contains(shrine))
            {
                shrines.Add(shrine);
                FallenUtils.Log($"[Manager] Shrine registered: {shrine.name}");
            }
        }

        public static void UnregisterShrine(GameObject shrine)
        {
            if (shrines.Contains(shrine))
            {
                shrines.Remove(shrine);
                FallenUtils.Log($"[Manager] Shrine unregistered: {shrine.name}");
            }
        }

        public static void Initialize()
        {
            if (!running)
            {
                FallenUtils.Log("[Manager] Initializing Shrine Proximity Checker...");
                MelonCoroutines.Start(ProximityCheckLoop());
                MelonCoroutines.Start(DynamicShrineScanner(2f));
                running = true;
            }
        }

        private static HashSet<IntPtr> knownShrinePtrs = new();

        private static IEnumerator DynamicShrineScanner(float period = 2f)
        {
            while (true)
            {
                GameObject shrineManager = GameObject.Find("Shrine Placement Manager");
                if (shrineManager != null)
                {
                    var shrines = shrineManager.GetComponentsInChildren<WorldObjectClickListener>(true);
                    foreach (var shrine in shrines)
                    {
                        if (shrine == null)
                            continue;

                        var go = shrine.gameObject;
                        IntPtr ptr = go.Pointer;

                        if (!knownShrinePtrs.Contains(ptr))
                        {
                            knownShrinePtrs.Add(ptr);

                            if (go.GetComponent(Il2CppType.Of<ShrineAutoRegister>()) == null)
                            {
                                go.AddComponent(Il2CppType.Of<ShrineAutoRegister>());
                                FallenUtils.Log($"[DynamicScan] New shrine registered: {go.name}");
                            }
                        }
                    }
                }

                yield return new WaitForSeconds(period);
            }
        }


        private static IEnumerator ProximityCheckLoop(float period = 0.5f)
        {
            while (true)
            {
                if (player == null)
                {
                    if (GameReferencesCache.player != null)
                        player = GameReferencesCache.player.gameObject;
                    if (player != null)
                    {
                        FallenUtils.Log("[Manager] Player found.");
                    }
                }

                if (player != null && player.transform != null)
                {
                    for (int i = shrines.Count - 1; i >= 0; i--)
                    {
                        GameObject shrine = shrines[i];
                        if (shrine == null)
                        {
                            shrines.RemoveAt(i);
                            continue;
                        }

                        float distance = Vector3.Distance(player.transform.position, shrine.transform.position);
                        if (distance <= 5f)
                        {
                            var comp = shrine.gameObject.GetComponent(Il2CppType.Of<WorldObjectClickListener>());
                            var listener = comp?.TryCast<WorldObjectClickListener>();


                            if (listener != null)
                            {
                                listener.ObjectClick(shrine, true);
                                FallenUtils.Log($"[Trigger] Activated shrine: {shrine.name} at distance {distance:F2}");
                            }
                            else
                            {
                                MelonLogger.Warning($"[Error] Shrine {shrine.name} missing WorldObjectClickListener.");
                            }

                            shrines.RemoveAt(i);
                        }
                    }
                }

                yield return new WaitForSeconds(period);
            }
        }
    }
}