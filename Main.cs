using Fallen_LE_Mods.Shared;
using Fallen_LE_Mods.Shared.UI;
using MelonLoader;

namespace Fallen_LE_Mods
{
    public class MyMod : MelonMod
    {
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