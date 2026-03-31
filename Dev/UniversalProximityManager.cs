using System;
using System.Collections;
using System.Collections.Generic;
using Fallen_LE_Mods.Shared;
using Il2Cpp;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine;
using HarmonyLib;
using static Fallen_LE_Mods.Shared.FallenUtils;

namespace Fallen_LE_Mods.Dev
{
    public static class UniversalProximityManager
    {
        private struct TrackedObject
        {
            public IntPtr Ptr;
            public Transform Trans;
            public WorldObjectClickListener Listener;
            public string Name;
        }

        private static readonly List<TrackedObject> activeObjects = new();
        private static readonly HashSet<IntPtr> knownPtrs = new();

        private static Transform? playerTrans;
        private static bool running = false;
        private const float SQUARED_DIST_LIMIT = 25f;

        public static void Initialize()
        {
            if (running) return;
            MelonCoroutines.Start(UpdateLoop());
            running = true;
        }

        public static void PerformFullSceneSweep()
        {
            Log("[Proximity Manager] Zone Change Detected. Performing Deep Sweep UwU...");

            activeObjects.Clear();
            knownPtrs.Clear();

            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            var all = GameObject.FindObjectsOfType<WorldObjectClickListener>(true);

            for (int i = 0; i < all.Length; i++)
            {
                var click = all[i];
                if (click == null) continue;

                GameObject go = click.gameObject;
                if (go.scene.name != activeScene.name) continue;
                if (IsTarget(go))
                {
                    Register(click, go);
                }
            }
            Log($"[Proximity Manager] Sweep complete. {(activeObjects.Count > 0 ? $"Found {activeObjects.Count} targets in [{activeScene.name}]:" : $"No valid targets found in [{activeScene.name}].")}");
            foreach (var obj in activeObjects)
            {
                Log($" -> Tracked: {obj.Name}");
            }
        }

        private static readonly HashSet<string> TargetKeywords = new()
        {
            "Shrine Placement Manager",
            "Chest Placement Manager",
            "Tomb Reward Chest",
            "Monolith Reward Chest",
            "Cache Click Listener"
        };

        private static bool IsTarget(GameObject go)
        {
            Transform? current = go.transform;

            while (current != null)
            {
                string name = current.name;

                foreach (var keyword in TargetKeywords)
                {
                    if (name.Contains(keyword))
                    {
                        return true;
                    }
                }

                current = current.parent;
            }

            return false;
        }

        private static void Register(WorldObjectClickListener listener, GameObject go)
        {
            IntPtr ptr = go.Pointer;
            if (knownPtrs.Contains(ptr)) return;

            knownPtrs.Add(ptr);
            activeObjects.Add(new TrackedObject
            {
                Ptr = ptr,
                Trans = go.transform,
                Listener = listener,
                Name = go.name
            });
        }

        private static IEnumerator UpdateLoop()
        {
            var wait = new WaitForSeconds(0.4f);
            while (true)
            {
                if (playerTrans == null || playerTrans.Pointer == IntPtr.Zero)
                {
                    if (GameReferencesCache.player != null)
                        playerTrans = GameReferencesCache.player.gameObject.transform;
                }

                if (playerTrans != null && activeObjects.Count > 0)
                {
                    Vector3 pPos = playerTrans.position;

                    for (int i = activeObjects.Count - 1; i >= 0; i--)
                    {
                        var obj = activeObjects[i];

                        if (obj.Ptr == IntPtr.Zero || obj.Trans == null)
                        {
                            activeObjects.RemoveAt(i);
                            continue;
                        }

                        Vector3 diff = pPos - obj.Trans.position;
                        float sqrMag = diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;

                        if (sqrMag <= SQUARED_DIST_LIMIT)
                        {
                            obj.Listener.ObjectClick(obj.Trans.gameObject, true);
                            Log($"[Proximity Manager] [Auto-Activate] Success: {obj.Trans.gameObject.name}");

                            activeObjects.RemoveAt(i);
                        }
                    }
                }
                yield return wait;
            }
        }
    }


    [HarmonyPatch(typeof(ClientSceneService), "OnActiveSceneChanged")]
    public class SceneChangePatch
    {
        public static void Postfix()
        {
            MelonCoroutines.Start(DelayedSweep());
        }
        private static IEnumerator DelayedSweep()
        {

            yield return new WaitForSeconds(0.5f);

            UniversalProximityManager.PerformFullSceneSweep();
        }
    }
}