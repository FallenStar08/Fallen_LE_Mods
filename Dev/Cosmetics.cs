using Fallen_LE_Mods.Shared;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Services.Cosmetics;
using Il2CppRewired.Utils;
using MelonLoader;
using UnityEngine;

namespace Fallen_LE_Mods.Dev.Cosmetics
{
    [RegisterTypeInIl2Cpp]
    public class Cosmetics_Offline : MonoBehaviour
    {
        public static Cosmetics_Offline instance { get; private set; }
        public Cosmetics_Offline(System.IntPtr ptr) : base(ptr) { }

        public static bool Initialized = false;
        public static Il2CppSystem.Collections.Generic.List<string> list_id = null;

        void Awake()
        {
            instance = this;
        }
        void Update()
        {
            if (Scenes.IsGameScene())
            {
                if (!Initialized) { Init(); }
            }
            else { Initialized = false; }
        }
        void Init()
        {
            if (!GameReferencesCache.gameUiBase.IsNullOrDestroyed())
            {
                GameObject go = GameReferencesCache.gameUiBase.bottomScreenMenu.gameObject;
                if (!go.IsNullOrDestroyed())
                {
                    GameObject panel = go.GetChildByName("BottomScreenMenuPanel");
                    if (!panel.IsNullOrDestroyed())
                    {
                        GameObject cosmetics = panel.GetChildByName("Cosmetics");
                        if (!cosmetics.IsNullOrDestroyed())
                        {
                            cosmetics.active = false;
                            Initialized = true;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CosmeticsManager), "GetOwnedCosmetics")]
        public class CosmeticsManager_GetOwnedCosmetics
        {
            [HarmonyPrefix]
            static bool Prefix(ref CosmeticsManager __instance, ref Il2CppCysharp.Threading.Tasks.UniTask<Il2CppSystem.Collections.Generic.List<string>> __result)
            {
                if (list_id.IsNullOrDestroyed())
                {
                    System.Collections.Generic.List<Cosmetic> cosmetics = new System.Collections.Generic.List<Cosmetic>();
                    foreach (Cosmetic cosmetic in Resources.FindObjectsOfTypeAll<Cosmetic>()) { cosmetics.Add(cosmetic); }
                    if (cosmetics != null && cosmetics.Count > 0)
                    {
                        list_id = new Il2CppSystem.Collections.Generic.List<string>();
                        foreach (Cosmetic cosmetic in cosmetics)
                        {
                            if (!list_id.Contains(cosmetic.BackendID)) { list_id.Add(cosmetic.BackendID); }
                        }
                    }
                }

                __result = new Il2CppCysharp.Threading.Tasks.UniTask<Il2CppSystem.Collections.Generic.List<string>>(list_id);

                return false;
            }
        }
    }
}


