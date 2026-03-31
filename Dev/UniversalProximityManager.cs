using System;
using System.Collections;
using System.Collections.Generic;
using Fallen_LE_Mods.Shared;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine;
using static Fallen_LE_Mods.Shared.FallenUtils;

namespace Fallen_LE_Mods.Dev
{
    public static class UniversalProximityManager
    {
        private struct TrackedObject
        {
            public long PtrAddr;
            public Transform Trans;
            public WorldObjectClickListener Listener;
            public string Name;
            public string Type;
        }

        private static readonly List<TrackedObject> activeObjects = new();
        private static readonly HashSet<long> knownPtrs = new();
        private static Transform? playerTrans;
        private static bool running = false;
        private const float SQUARED_DIST_LIMIT = 25f;

        private static readonly Dictionary<string, string> TargetKeywords = new()
        {
            { "Shrine Placement Manager", "Shrine" },
            { "Chest Placement Manager", "Chest" },
            { "Tomb Reward Chest", "Cemetery Chest" },
            { "Monolith Reward Chest", "Monolith Chest" },
            { "Cache Click Listener", "Cache" }
        };

        public static void Initialize()
        {
            if (running) return;
            MelonCoroutines.Start(UpdateLoop());
            running = true;
            Log("[Proximity Manager] Initialized");
        }

        [HarmonyPatch(typeof(WorldObjectClickListener), nameof(WorldObjectClickListener.OnEnable))]
        public class ListenerOnEnablePatch
        {
            public static void Postfix(WorldObjectClickListener __instance)
            {
                if (__instance == null || __instance.Pointer == IntPtr.Zero) return;

                long addr = __instance.Pointer.ToInt64();
                if (knownPtrs.Contains(addr)) return;

                GameObject go = __instance.gameObject;
                if (go == null) return;

                if (TryGetTargetType(go, out string type))
                {
                    Register(__instance, go, type, addr);
                }
            }
        }

        private static bool TryGetTargetType(GameObject go, out string type)
        {
            type = "Unknown";
            Transform? current = go.transform;
            while (current != null)
            {
                string n = current.name;
                foreach (var entry in TargetKeywords)
                {
                    if (n.Contains(entry.Key))
                    {
                        type = entry.Value;
                        return true;
                    }
                }
                current = current.parent;
            }
            return false;
        }

        private static void Register(WorldObjectClickListener listener, GameObject go, string type, long addr)
        {
            knownPtrs.Add(addr);
            activeObjects.Add(new TrackedObject
            {
                PtrAddr = addr,
                Trans = go.transform,
                Listener = listener,
                Name = go.name ?? "Unknown Object",
                Type = type,
            });

            Log($"[Proximity Manager] +Tracked [{type}]: {go.name}");
        }

        private static IEnumerator UpdateLoop()
        {
            var wait = new WaitForSeconds(0.4f);
            while (true)
            {
                knownPtrs.RemoveWhere(p => p == 0);

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


                        if (obj.PtrAddr == 0 || obj.Trans == null || obj.Trans.Pointer == IntPtr.Zero)
                        {
                            Log($"[Proximity Manager] -Unregistered (Destroyed): {obj.Name} [{obj.Type}]");
                            activeObjects.RemoveAt(i);
                            continue;
                        }

                        try
                        {
                            if (!obj.Trans.gameObject.activeInHierarchy)
                            {
                                Log($"[Proximity Manager] -Unregistered (Disabled): {obj.Name} [{obj.Type}]");
                                activeObjects.RemoveAt(i);
                                continue;
                            }

                            Vector3 diff = pPos - obj.Trans.position;
                            float sqrMag = diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;

                            if (sqrMag <= SQUARED_DIST_LIMIT)
                            {
                                obj.Listener.ObjectClick(obj.Trans.gameObject, true);
                                Log($"[Proximity Manager] [Auto-Activate] Success: {obj.Name} ({obj.Type})");
                                activeObjects.RemoveAt(i);
                            }
                        }
                        catch (Exception e)
                        {
                            Log($"[Proximity Manager] -Unregistered (Error): {obj.Name} | {e.Message}");
                            activeObjects.RemoveAt(i);
                        }
                    }
                }
                yield return wait;
            }
        }
    }
}