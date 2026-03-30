#if RELEASE
using Fallen_LE_Mods.Dev;
#endif
using Fallen_LE_Mods.Shared;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using static Fallen_LE_Mods.Improved_Tooltips.GroundLabelManager;







namespace Fallen_LE_Mods
{
    public class MyMod : MelonMod
    {
        //Late Harmony patching for compatibility with other mods...
        public override void OnLateInitializeMelon()
        {


            var targetMethod = AccessTools.Method(typeof(GroundItemLabel), "SetGroundTooltipText", new Type[] { typeof(bool) });

            if (targetMethod == null)
            {
                FallenUtils.Log("Target method 'SetGroundTooltipText' not found.");
                return;
            }

            var patchMethod = AccessTools.Method(typeof(GroundLabelPatch), "Postfix");
            var patch = new HarmonyMethod(patchMethod);
            HarmonyInstance.Patch(targetMethod, null, patch);
            //FallenUtils.Log("Patch applied successfully.");


        }


#if RELEASE
        private static string[] pauseScenes = { "M_Rest", "ClientSplash", "PersistentUI", "Login", "CharacterSelectScene", "EoT", "MonolithHub", "Bazaar", "Observatory" };

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (pauseScenes.Contains(sceneName))
            {
                GameStatsTracker.Pause();
            }
            else
            {
                GameStatsTracker.Resume();
            }

            //ShrineAutoActivator.OnSceneWasLoadedHandler(buildIndex, sceneName);

        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (pauseScenes.Contains(sceneName)) return;
            UniversalProximityManager.Initialize();
        }

#endif
    }

}