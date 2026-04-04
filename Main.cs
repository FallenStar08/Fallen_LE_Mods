#if RELEASE
using Fallen_LE_Mods.Dev;
#endif
using Fallen_LE_Mods.Shared;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
#if IMPROVED_TOOLTIPS || RELEASE
using static Fallen_LE_Mods.Improved_Tooltips.GroundLabelManager;
#endif







namespace Fallen_LE_Mods
{

    public class MyMod : MelonMod
    {
#if IMPROVED_TOOLTIPS || RELEASE
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
#endif


#if RELEASE
        private static string[] pauseScenes = { "M_Rest", "ClientSplash", "PersistentUI", "Login", "CharacterSelectScene", "EoT", "MonolithHub", "Bazaar", "Observatory" };

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            Cosmetics_Offline.OnSceneLoaded(buildIndex, sceneName);
            if (pauseScenes.Contains(sceneName))
            {
                GameStatsTracker.Pause();
            }
            else
            {
                GameStatsTracker.Resume();
            }

        }
#endif

#if AUTO_ENABLER || RELEASE
        public override void OnInitializeMelon()
        {
            Fallen_LE_Mods.Auto_Enabler.UniversalProximityManager.Initialize();
        }
#endif


    }

}