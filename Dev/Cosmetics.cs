using Fallen_LE_Mods.Shared;
using Il2Cpp;
using Il2CppRewired.Utils;
using MelonLoader;
using UnityEngine;
using HarmonyLib;
using Il2CppLE.Services.Cosmetics;

namespace Fallen_LE_Mods.Dev;

[RegisterTypeInIl2Cpp]
public class Cosmetics_Offline : MonoBehaviour
{
    public static Cosmetics_Offline? Instance { get; private set; }
    private static Il2CppSystem.Collections.Generic.List<string>? _cachedListId;

    public Cosmetics_Offline(System.IntPtr ptr) : base(ptr) { }

    void Awake() => Instance = this;

    public static void OnSceneLoaded(int buildIndex, string sceneName)
    {
        if (Scenes.IsGameScene())
        {
            Instance?.TryDisableCosmeticsUI();
        }
    }

    private void TryDisableCosmeticsUI()
    {
        var cosmeticsBtn = GameReferencesCache.gameUiBase?.bottomScreenMenu?
            .gameObject.GetChildByName("BottomScreenMenuPanel")?
            .GetChildByName("Cosmetics");

        if (cosmeticsBtn != null)
        {
            cosmeticsBtn.active = false;
        }
    }

    [HarmonyPatch(typeof(CosmeticsManager), "GetOwnedCosmetics")]
    public class CosmeticsManager_GetOwnedCosmetics
    {
        [HarmonyPrefix]
        static bool Prefix(ref Il2CppCysharp.Threading.Tasks.UniTask<Il2CppSystem.Collections.Generic.List<string>> __result)
        {
            if (_cachedListId.IsNullOrDestroyed())
            {
                _cachedListId = new Il2CppSystem.Collections.Generic.List<string>();

                var allCosmetics = Resources.FindObjectsOfTypeAll<Cosmetic>();

                foreach (var c in allCosmetics)
                {
                    if (c != null && !string.IsNullOrEmpty(c.BackendID) && !_cachedListId.Contains(c.BackendID))
                    {
                        _cachedListId.Add(c.BackendID);
                    }
                }
            }

            __result = new Il2CppCysharp.Threading.Tasks.UniTask<Il2CppSystem.Collections.Generic.List<string>>(_cachedListId!);
            return false;
        }
    }
}