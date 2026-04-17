using Fallen_LE_Mods.Shared;
using HarmonyLib;
using Il2Cpp;

namespace Fallen_LE_Mods.Dev
{
    public static class Madge
    {
        [HarmonyPatch(typeof(Actor), nameof(Actor.Awake))]
        public static class ActorAwakePatch
        {
            public static void Postfix(Actor __instance)
            {
                var controller = __instance.GetComponent<MADStateController>();
                if (controller != null)
                {
                    CoroutineHelper.DelayFixed(() =>
                    {
                        controller.aggroRangeModifier = 255f;
                        controller.playerAggroRangeModifier = 255f;
                    });
                }
            }
        }

        [HarmonyPatch(typeof(SpawnController), nameof(SpawnController.Awake))]
        public static class SpawnControllerAwakePatch
        {
            public static void Postfix(SpawnController __instance)
            {
                var listener = __instance.GetComponent<WorldAreaEnterListener>();
                if (listener != null)
                {
                    CoroutineHelper.DelayFixed(() =>
                    {
                        listener.listeningAreaRadius = 255f;
                        listener.requiredStayDuration = 0;
                    });
                }
            }
        }

        [HarmonyPatch(typeof(SpawnerPlacementManager), nameof(SpawnerPlacementManager.Start))]
        public static class SpawnerPlacementManagerStartPatch
        {
            public static void Postfix(SpawnerPlacementManager __instance)
            {
                CoroutineHelper.DelayFixed(() =>
                {
                    __instance.increasedSpawnRange = 255f;
                });
            }
        }
    }
}