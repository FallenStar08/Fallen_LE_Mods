using System.Collections;
using Fallen_LE_Mods.Shared;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using static Fallen_LE_Mods.Shared.FallenUtils;

namespace Fallen_LE_Mods.Auto_Enabler
{
    public static class UniversalProximityManager
    {

        public static MelonPreferences_Category? _category;
        public static MelonPreferences_Entry<bool>? _prefShowRings;
        public static MelonPreferences_Entry<float>? _prefDistance;
        public static MelonPreferences_Entry<Color>? _prefColor;

        public static readonly Dictionary<string, MelonPreferences_Entry<bool>> TypeToggles = new();

        private static float _currentSqrDist = 25f;
        private static float _currentRadius = 5f;
        private struct TrackedObject
        {
            public long PtrAddr;
            public Transform Trans;
            public WorldObjectClickListener Listener;
            public GameObject? VisualRing;
            public string Name;
            public string Type;
            public int NullStrikes;
        }

        private static readonly List<TrackedObject> activeObjects = new();
        private static readonly HashSet<long> knownPtrs = new();
        private static Transform? playerTrans;
        private static bool running = false;

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

            _category = MelonPreferences.CreateCategory("ProximityManager", "Proximity Manager");
            _category.SetFilePath("UserData/FallenProximity.cfg");
            _prefShowRings = _category.CreateEntry("ShowRings", true, "Show Visual Rings");
            _prefDistance = _category.CreateEntry("Distance", 5.0f, "Activation Distance");
            _prefColor = _category.CreateEntry("RingColor", new Color(0.1f, 0.8f, 1f, 0.5f), "Ring Color");

            foreach (var val in TargetKeywords.Values)
            {
                if (!TypeToggles.ContainsKey(val))
                {
                    var entry = _category.CreateEntry($"Enable_{val.Replace(" ", "")}", true, $"Enable {val}");
                    TypeToggles.Add(val, entry);
                }
            }

            _category.SaveToFile();

            UpdateSettings();

            _prefDistance.OnEntryValueChanged.Subscribe((oldV, newV) => UpdateSettings());
            _prefColor.OnEntryValueChanged.Subscribe((oldV, newV) => UpdateTemplateVisuals());

            CreateTemplate();
            MelonCoroutines.Start(UpdateLoop());
            MelonCoroutines.Start(InitialStartupSweep());
            running = true;
            FallenUI.RegisterMenu(DrawProximitySettings);
            Log("[Proximity Manager] Initialized and Prefs Saved!");
        }

        private static void DrawProximitySettings(Transform container)
        {
            FallenUI.CreateHeader(container, "Fallen's Proximity Manager", "ProxHeader");
            FallenUI.CreateToggle(container, "Show Proximity Rings", "Visual colored circles around shrines and chests.", _prefShowRings);
            FallenUI.CreateSlider(container, "Activation Radius", "Distance at which proximity activation occurs.", 1f, 10f, _prefDistance);

            // Sub-Filters
            FallenUI.CreateHeader(container, "Auto-Activation Filters", "ProxFilters");
            foreach (var entry in TypeToggles)
            {
                FallenUI.CreateToggle(container, $"Auto-Activate {entry.Key}s", $"Enable or disable proximity activation for {entry.Key} objects.", entry.Value);
            }
        }

        private static void UpdateSettings()
        {
            float dist = _prefDistance.Value;
            _currentSqrDist = dist * dist;
            _currentRadius = dist;

            //Re-draw template mesh if distance changed
            if (_ringTemplate != null) CreateTemplate(true);

            //Refresh all currently visible rings
            foreach (var obj in activeObjects)
            {
                if (obj.VisualRing == null) continue;

                var lr = obj.VisualRing.GetComponent<LineRenderer>();
                if (lr == null) continue;

                float deltaTheta = 2f * Mathf.PI / 32;
                for (int i = 0; i < 32; i++)
                {
                    float x = _currentRadius * Mathf.Cos(deltaTheta * i);
                    float z = _currentRadius * Mathf.Sin(deltaTheta * i);
                    lr.SetPosition(i, new Vector3(x, z, 0));
                }
            }
        }

        private static void UpdateTemplateVisuals()
        {
            if (_ringTemplate == null) return;
            var lr = _ringTemplate.GetComponent<LineRenderer>();
            if (lr != null) lr.material.color = _prefColor.Value;
        }

        private static GameObject? _ringTemplate;

        private static void CreateTemplate(bool forceRefresh = false)
        {
            if (_ringTemplate != null && !forceRefresh) return;

            if (_ringTemplate == null)
            {
                _ringTemplate = new GameObject("ProximityRing_Template");
                _ringTemplate.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(_ringTemplate);
                _ringTemplate.AddComponent<LineRenderer>();
            }

            LineRenderer lr = _ringTemplate.GetComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.widthMultiplier = 0.12f;
            lr.positionCount = 32;
            lr.castShadows = false;
            lr.loop = true;

            if (lr.material == null || lr.material.name.Contains("Default"))
            {
                lr.material = new Material(Shader.Find("Sprites/Default"));
            }
            lr.material.color = _prefColor.Value;

            float deltaTheta = 2f * Mathf.PI / 32;
            for (int i = 0; i < 32; i++)
            {
                float x = _currentRadius * Mathf.Cos(deltaTheta * i);
                float z = _currentRadius * Mathf.Sin(deltaTheta * i);
                lr.SetPosition(i, new Vector3(x, z, 0));
            }
        }

        private static GameObject? CreateProximityRing(GameObject parent)
        {
            if (!_prefShowRings.Value) return null;

            if (_ringTemplate == null) CreateTemplate();
            if (_ringTemplate == null) return null;

            try
            {
                GameObject ringGo = UnityEngine.Object.Instantiate(_ringTemplate, parent.transform);
                ringGo.name = "ProximityRing";
                ringGo.transform.localPosition = new Vector3(0, 0.12f, 0);
                ringGo.transform.localRotation = Quaternion.Euler(90, 0, 0);
                ringGo.SetActive(true);
                return ringGo;
            }
            catch { return null; }
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
                if (TypeToggles.TryGetValue(type, out var toggle) && !toggle.Value)
                {
                    return;
                }
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
                NullStrikes = 0
            });

            LogDebug($"[Proximity Manager] +Tracked [{type}]: {go.name}");
        }


        private static IEnumerator UpdateLoop()
        {
            var wait = new WaitForSeconds(0.3f);
            while (true)
            {
                //Nothing to do :(
                int count = activeObjects.Count;
                if (count == 0)
                {
                    yield return wait;
                    continue;
                }

                if (playerTrans == null || playerTrans.Pointer == IntPtr.Zero)
                {
                    if (GameReferencesCache.player != null)
                        playerTrans = GameReferencesCache.player.gameObject.transform;
                }

                if (playerTrans != null)
                {
                    Vector3 pPos = playerTrans.position;
                    float limit = _currentSqrDist;

                    for (int i = count - 1; i >= 0; i--)
                    {
                        var obj = activeObjects[i];

                        if (IsObjectInvalid(ref obj, i)) continue;

                        if (HandleProximity(obj, pPos, limit, i)) continue;
                    }
                }
                yield return wait;
            }
        }

        private static bool IsObjectInvalid(ref TrackedObject obj, int index)
        {
            if (obj.Listener == null || obj.Listener.Pointer == IntPtr.Zero || obj.Trans == null || obj.Trans.Pointer == IntPtr.Zero)
            {
                obj.NullStrikes++;
                activeObjects[index] = obj;

                if (obj.NullStrikes >= 3)
                {
                    LogDebug($"[Proximity Manager] -Unregistered (Confirmed Dead): {obj.Name}");
                    CleanupObject(index);
                    return true;
                }
                return true;
            }

            if (obj.NullStrikes > 0)
            {
                obj.NullStrikes = 0;
                activeObjects[index] = obj;
            }

            if (!obj.Trans.gameObject.activeInHierarchy)
            {
                LogDebug($"[Proximity Manager] -Unregistered (Inactive): {obj.Name}");
                CleanupObject(index);
                return true;
            }

            return false;
        }

        private static bool HandleProximity(TrackedObject obj, Vector3 pPos, float limit, int index)
        {
            try
            {
                Vector3 oPos = obj.Trans.position;

                float dx = pPos.x - oPos.x;
                float dz = pPos.z - oPos.z;
                float sqrMag2D = (dx * dx) + (dz * dz);

                if (sqrMag2D <= limit)
                {
                    obj.Listener.ObjectClick(obj.Trans.gameObject, true);
                    LogDebug($"[Proximity Manager] [Auto-Activate] Success: {obj.Name}");
                    CleanupObject(index);
                    return true;
                }
            }
            catch (Exception e)
            {
                LogDebug($"[Proximity Manager] Error processing {obj.Name}: {e.Message}");
                CleanupObject(index);
                return true;
            }
            return false;
        }

        private static void CleanupObject(int index)
        {
            var obj = activeObjects[index];
            if (obj.VisualRing != null) UnityEngine.Object.Destroy(obj.VisualRing);

            knownPtrs.Remove(obj.PtrAddr);
            activeObjects.RemoveAt(index);
        }
    }
}