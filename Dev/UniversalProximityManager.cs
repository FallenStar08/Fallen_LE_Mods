using System;
using System.Collections;
using System.Collections.Generic;
using Fallen_LE_Mods.Shared;
using Il2Cpp;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine;
using static Fallen_LE_Mods.Shared.FallenUtils;

namespace Fallen_LE_Mods.Dev
{
    public static class UniversalProximityManager
    {
        private static readonly List<GameObject> activeObjects = new();
        private static readonly HashSet<IntPtr> knownPtrs = new();

        private static GameObject? player;
        private static bool running = false;

        public static void Initialize()
        {
            if (running) return;

            Log("[UniversalManager] Initializing Global Proximity System...");
            MelonCoroutines.Start(UpdateLoop());
            MelonCoroutines.Start(ScannerLoop(5f));
            running = true;
        }

        public static void Register(GameObject go)
        {
            if (go == null) return;
            IntPtr ptr = go.Pointer;

            if (!knownPtrs.Contains(ptr))
            {
                knownPtrs.Add(ptr);
                activeObjects.Add(go);
            }
        }

        private static IEnumerator ScannerLoop(float interval)
        {
            while (true)
            {
                string[] managers = { "Shrine Placement Manager", "Chest Placement Manager" };

                foreach (var managerName in managers)
                {
                    GameObject mgr = GameObject.Find(managerName);
                    if (mgr == null) continue;

                    var clickables = mgr.GetComponentsInChildren<WorldObjectClickListener>(true);
                    foreach (var click in clickables)
                    {
                        Register(click.gameObject);
                    }
                }
                yield return new WaitForSeconds(interval);
            }
        }

        private static IEnumerator UpdateLoop()
        {
            while (true)
            {
                if (player == null || player.Pointer == IntPtr.Zero || !player.activeInHierarchy)
                {
                    if (GameReferencesCache.player != null)
                    {
                        player = GameReferencesCache.player.gameObject;
                        Log("[UniversalManager] Re-acquired Player Reference.");
                    }
                }

                if (player != null && activeObjects.Count > 0)
                {
                    Vector3 playerPos = player.transform.position;

                    for (int i = activeObjects.Count - 1; i >= 0; i--)
                    {
                        GameObject obj = activeObjects[i];

                        if (obj == null || obj.Pointer == IntPtr.Zero)
                        {
                            activeObjects.RemoveAt(i);
                            continue;
                        }

                        float dist = Vector3.Distance(playerPos, obj.transform.position);

                        if (dist <= 5.0f)
                        {
                            TriggerObject(obj);
                            knownPtrs.Remove(obj.Pointer);
                            activeObjects.RemoveAt(i);
                        }
                    }
                }
                yield return new WaitForSeconds(0.50f);
            }
        }

        private static void TriggerObject(GameObject go)
        {
            var listener = go.GetComponent<WorldObjectClickListener>();
            if (listener != null)
            {
                listener.ObjectClick(go, true);
                Log($"[UniversalManager] Auto-Activated: {go.name}");
            }
        }
    }
}