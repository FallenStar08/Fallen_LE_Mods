using Fallen_LE_Mods.Shared;
using Fallen_LE_Mods.Shared.UI;
using Il2CppLE.Data;
using MelonLoader;

namespace Fallen_LE_Mods
{
    public class MyMod : MelonMod
    {
        private bool _onlineAnnoyed = false;
        private readonly List<IFallenFeature> _features = new();
        public override void OnInitializeMelon()
        {
            MelonCoroutines.Start(VersionChecker.CheckGitHubBuildInfo());
            FallenUtils.Harmony = this.HarmonyInstance;
#if IMPROVED_TOOLTIPS || RELEASE
            _features.Add(new Fallen_LE_Mods.Improved_Tooltips.ImprovedTooltipsFeature());
#endif

#if AUTO_ENABLER || RELEASE
            _features.Add(new Fallen_LE_Mods.Auto_Enabler.ProximityFeature());
#endif

#if RELEASE
            _features.Add(new Fallen_LE_Mods.Dev.QuickShatterFeature());
#endif

            foreach (var feature in _features)
            {
                feature.OnMelonInitialize();
            }
        }

        public override void OnLateInitializeMelon()
        {
            foreach (var feature in _features)
            {
                feature.OnMelonLateInitialize();
            }
        }

        private static readonly string[] pauseScenes = { "M_Rest", "ClientSplash", "PersistentUI", "Login", "CharacterSelectScene", "EoT", "MonolithHub", "Bazaar", "Observatory" };

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (Scenes.IsGameScene())
            {
                if (GameReferencesCache.Player.Value)
                {
                    CharacterData? charData = GameReferencesCache.Player.Value.TryGetCharacterData(out var data) ? data : GameReferencesCache.Player.Value.GetCharacterDataTracker()?.charData;

                    bool gotCharData = charData != null;

                    if (gotCharData)
                    {
                        FallenUtils.LogDebug($"Online state : {(charData.IsOffline ? "offline" : "online ")}");
                        if (!charData.IsOffline & !_onlineAnnoyed)
                        {
                            FallenUtils.Error("You're playing in Online Mode buckaroo, this isn't a good idea. (I ain't stopping you tho)");
                            _onlineAnnoyed = true;
                        }
                    }
                    else
                    {
                        FallenUtils.Error("Couldn't get CharData we're cooked I guess?");
                    }
                }
            }


            foreach (var feature in _features)
            {
                feature.OnMelonSceneLoaded(sceneName);
            }
#if RELEASE

            Fallen_LE_Mods.Dev.Cosmetics_Offline.OnSceneLoaded(buildIndex, sceneName);
            if (pauseScenes.Contains(sceneName))
                Fallen_LE_Mods.Dev.GameStatsTracker.Pause();
            else
                Fallen_LE_Mods.Dev.GameStatsTracker.Resume();
#endif
        }
    }
}