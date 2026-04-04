using System.Collections;
using Fallen_LE_Mods.Shared;
using HarmonyLib;
using Il2Cpp;
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
            public GameObject? VisualRing;
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
            { "Cache Click Listener", "Cache" },
            { "void portal", "Void Portal" }
        };

        public static void Initialize()
        {
            if (running) return;
            CreateTemplate();
            MelonCoroutines.Start(UpdateLoop());
            MelonCoroutines.Start(InitialStartupSweep());
            running = true;
            Log("[Proximity Manager] Initialized");
        }

        //Sanity check
        private static IEnumerator InitialStartupSweep()
        {
            yield return new WaitForSeconds(1f);
            var all = GameObject.FindObjectsOfType<WorldObjectClickListener>(true);
            foreach (var listener in all)
            {
                ProcessPotentialTarget(listener);
            }
        }
        //Objects Birth
        [HarmonyPatch(typeof(InteractableListener), nameof(InteractableListener.Awake))]
        public class ListenerAwakePatch
        {
            public static void Postfix(InteractableListener __instance)
            {
                if (__instance == null) return;
                var listener = __instance.TryCast<WorldObjectClickListener>();
                if (listener != null) ProcessPotentialTarget(listener);
            }
        }
        //Pooled objects?
        [HarmonyPatch(typeof(WorldObjectClickListener), nameof(WorldObjectClickListener.OnEnable))]
        public class ListenerOnEnablePatch
        {
            public static void Postfix(WorldObjectClickListener __instance)
            {
                ProcessPotentialTarget(__instance);
            }
        }

        private static void ProcessPotentialTarget(WorldObjectClickListener listener)
        {
            if (listener == null || listener.Pointer == IntPtr.Zero) return;

            long addr = listener.Pointer.ToInt64();
            if (knownPtrs.Contains(addr)) return;

            GameObject go = listener.gameObject;
            if (go == null) return;

            if (TryGetTargetType(go, out string type))
            {
                Register(listener, go, type, addr);
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

            GameObject? ring = CreateProximityRing(go);

            activeObjects.Add(new TrackedObject
            {
                PtrAddr = addr,
                Trans = go.transform,
                Listener = listener,
                VisualRing = ring,
                Name = go.name ?? "Unknown Object",
                Type = type,
            });

            Log($"[Proximity Manager] +Tracked [{type}]: {go.name}");
        }
        private static GameObject? _ringTemplate;

        //Template object to reduce GC and Draw Calls when creating proximity rings. The circle mesh is pre-built and shared
        private static void CreateTemplate()
        {
            if (_ringTemplate != null) return;

            _ringTemplate = new GameObject("ProximityRing_Template");
            _ringTemplate.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(_ringTemplate);

            LineRenderer lr = _ringTemplate.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.widthMultiplier = 0.12f;
            lr.positionCount = 32;
            lr.loop = true;

            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.material.color = new Color(0.1f, 0.8f, 1f, 0.5f);

            float radius = Mathf.Sqrt(SQUARED_DIST_LIMIT);
            float deltaTheta = (2f * Mathf.PI) / 31;
            for (int i = 0; i < 32; i++)
            {
                float x = radius * Mathf.Cos(deltaTheta * i);
                float z = radius * Mathf.Sin(deltaTheta * i);
                lr.SetPosition(i, new Vector3(x, z, 0));
            }
        }

        private static GameObject? CreateProximityRing(GameObject parent)
        {
            if (_ringTemplate == null) CreateTemplate();
            if (_ringTemplate == null) return null;

            try
            {
                GameObject ringGo = UnityEngine.Object.Instantiate(_ringTemplate, parent.transform);
                ringGo.name = "ProximityRing";
                ringGo.transform.localPosition = new Vector3(0, 0.2f, 0);
                ringGo.transform.localRotation = Quaternion.Euler(90, 0, 0);
                ringGo.SetActive(true);

                return ringGo;
            }
            catch (Exception e) { Log($"[Proximity Manager] Failed to draw ring: {e.Message}"); return null; }
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


                        if (obj.Listener == null || obj.Listener.Pointer == IntPtr.Zero)
                        {
                            Log($"[Proximity Manager] -Unregistered (Destroyed or GCed): {obj.Name} [{obj.Type}]");
                            if (obj.VisualRing != null) GameObject.Destroy(obj.VisualRing);
                            activeObjects.RemoveAt(i);
                            continue;
                        }

                        try
                        {
                            if (obj.Trans == null || obj.Trans.Pointer == IntPtr.Zero)
                            {
                                Log($"[Proximity Manager] -Unregistered (Unparented?): {obj.Name} [{obj.Type}]");
                                if (obj.VisualRing != null) GameObject.Destroy(obj.VisualRing);
                                activeObjects.RemoveAt(i);
                                continue;
                            }

                            GameObject go = obj.Trans.gameObject;

                            // remove from tracking so OnEnable can re-catch it later
                            if (go == null || !go.activeInHierarchy)
                            {
                                //allows the OnEnable patch to "see" it again if the game flickers it
                                knownPtrs.Remove(obj.PtrAddr);
                                Log($"[Proximity Manager] -Unregistered (Disabled): {obj.Name} [{obj.Type}]");
                                if (obj.VisualRing != null) GameObject.Destroy(obj.VisualRing);
                                activeObjects.RemoveAt(i);
                                continue;
                            }

                            Vector3 diff = pPos - obj.Trans.position;
                            float sqrMag = diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;

                            if (sqrMag <= SQUARED_DIST_LIMIT)
                            {
                                obj.Listener.ObjectClick(obj.Trans.gameObject, true);
                                Log($"[Proximity Manager] [Auto-Activate] Success: {obj.Name} ({obj.Type})");
                                if (obj.VisualRing != null) GameObject.Destroy(obj.VisualRing);
                                activeObjects.RemoveAt(i);
                            }
                        }
                        catch (Exception e)
                        {
                            Log($"[Proximity Manager] -Unregistered (Error): {obj.Name} | {e.Message}");
                            if (obj.VisualRing != null) GameObject.Destroy(obj.VisualRing);
                            activeObjects.RemoveAt(i);
                        }
                    }
                }
                yield return wait;
            }
        }
    }
}