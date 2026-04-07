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
        private const int POS_COUNT = 64;
        private const float LINE_WIDTH = 0.25f;
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
            { "void portal", "Void Portal" },
            { "Rune Prison Visuals", "Rune Prison" },
            { "Time Beast Rift Visuals", "Time Beast Rift" }
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
            _currentRadius = _prefDistance!.Value;
            _currentSqrDist = _currentRadius * _currentRadius;

            foreach (var obj in activeObjects)
            {
                if (obj.VisualRing == null) continue;

                obj.VisualRing.transform.localScale = new Vector3(_currentRadius, _currentRadius, _currentRadius);

                var lr = obj.VisualRing.GetComponent<LineRenderer>();
                if (lr != null) lr.widthMultiplier = LINE_WIDTH / _currentRadius;
            }
        }

        private static void UpdateTemplateVisuals()
        {
            if (_ringTemplate == null) return;
            var lr = _ringTemplate.GetComponent<LineRenderer>();
            if (lr != null) lr.material.color = _prefColor.Value;
        }

        private static GameObject? _ringTemplate;

        private static void CreateTemplate()
        {
            if (_ringTemplate != null) return;

            _ringTemplate = new GameObject("ProximityRing_Template");
            _ringTemplate.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(_ringTemplate);

            LineRenderer lr = _ringTemplate.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.widthMultiplier = LINE_WIDTH;
            lr.positionCount = POS_COUNT;
            lr.castShadows = false;
            lr.loop = true;

            if (lr.material == null || lr.material.shader.name != "UI/Default")
            {
                lr.material = new Material(Shader.Find("UI/Default"));
            }
            lr.material.color = _prefColor!.Value;

            float deltaTheta = 2f * Mathf.PI / POS_COUNT;
            for (int i = 0; i < POS_COUNT; i++)
            {
                float x = Mathf.Cos(deltaTheta * i);
                float z = Mathf.Sin(deltaTheta * i);
                lr.SetPosition(i, new Vector3(x, z, 0));
            }
        }

        private static GameObject? CreateProximityRing(GameObject parent)
        {
            if (!_prefShowRings.Value) return null;
            if (_ringTemplate == null) CreateTemplate();

            try
            {
                //Instantiate at Root
                GameObject ringGo = UnityEngine.Object.Instantiate(_ringTemplate);
                ringGo.name = "ProximityRing";
                ringGo.transform.SetParent(null);


                ringGo.transform.localScale = new Vector3(_currentRadius, _currentRadius, _currentRadius);

                var lr = ringGo.GetComponent<LineRenderer>();
                if (lr != null) lr.widthMultiplier = LINE_WIDTH / _currentRadius;

                float yOffset = 0.12f;
                Vector3 spawnPos = parent.transform.position + Vector3.up;
                ringGo.transform.position = Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 3.0f)
                    ? hit.point + (Vector3.up * yOffset)
                    : parent.transform.position + (Vector3.up * yOffset);

                ringGo.transform.rotation = Quaternion.Euler(90, 0, 0);
                ringGo.SetActive(true);
                return ringGo;
            }
            catch { return null; }
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
                        //may be needed later
                        //if (obj.VisualRing != null)
                        //    obj.VisualRing.transform.position = obj.Trans.position;
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
                    //fix cemetery chests?
                    if (obj.Name.Contains("Frontend"))
                    {
                        var condition = obj.Trans.gameObject.GetComponentInChildren<ConditionHandler>();
                        if (condition != null)
                        {
                            condition.canOnlyTriggerOnce = false;
                            LogDebug($"[Proximity Manager] Unlocked ConditionHandler for: {obj.Name}");
                        }
                    }

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