using Fallen_LE_Mods.Shared;
using HarmonyLib;
using Il2CppLE.Factions;
using UnityEngine;

namespace Fallen_LE_Mods.Dev
{
    public static class propheciesManager
    {
        [HarmonyPatch(typeof(Il2CppLE.Factions.Prophecy), "Init")]
        public class ProphecyInitPatch
        {
            private static void Postfix(Prophecy __instance, Il2CppLE.Factions.ObservatoryUI panel, Il2CppSystem.Collections.Generic.List<Il2CppLE.Factions.Prophecy> prophecies, int playerFavor)
            {
                ProphecyRegion pRegion = panel.CurrentRegion;
                GameObject observatoryGo = panel.gameObject;
                GameObject? currentConstellationRoot = observatoryGo.GetChildByName($"Constellation Root {pRegion.ToString()[0]}");
                Constellation? currentConstellation = currentConstellationRoot?.GetComponentInChildren<Constellation>();
                Il2CppSystem.Collections.Generic.IEnumerable<Il2CppLE.Factions.ConstellationStar> currentStars = currentConstellation.ActiveStars;
                //Get Searchbox go from stash at
                //GUI/Panel System/Panel Pool/StashPanelExpandable(Clone)/left-container/SearchBox

                //Searchbox should go at 
                // GUI/Panel System/Panel Stacks/Full Screen Panel Stack/Observatory(Clone)/ObservatoryConfig/

                //Rewire searchbox to hide stars that don't match the search query
                //BY looping over currentStars and setting active to false
                //using getStarCondition and getStarReward

            }
        }

        public static string getStarReward(ConstellationStar star)
        {
            Prophecy associatedProphecy = star.currentProphecy;
            ProphecyReward reward = associatedProphecy.Reward;
            string rewardString = reward.generatedRewardString;
            return rewardString;

        }

        public static string getStarCondition(ConstellationStar star)
        {
            Prophecy associatedProphecy = star.currentProphecy;
            ProphecyTarget target = associatedProphecy.Target;
            string targetString = target.name;
            return targetString;

        }





    }
}
