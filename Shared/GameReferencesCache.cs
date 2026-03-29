using HarmonyLib;
using Il2Cpp;
using Il2CppItemFiltering;
using Il2CppLE.UI;

namespace Fallen_LE_Mods.Shared
{
    //Probably no need to refresh these on each load but idk...
    [HarmonyPatch(typeof(LoadingScreen), "Disable")]
    public class GameReferencesCache
    {
        public static Actor? player;
        public static ItemFilterManager? itemFilterManager;
        public static ActorVisuals? playerVisuals;
        public static ItemContainersManager? itemContainersManager;
        public static Il2CppSystem.Collections.Generic.List<ItemContainer>? playerStash;
        public static UIBase? gameUiBase;
        public static InventoryPanelUI? inventoryPanelUI;
        public static Il2CppLE.Data.CharacterData? playerData;
        public static ExperienceTracker? expTracker;
        public static GoldTracker? goldTracker;
        public static CharacterDataTracker? characterDataTracker;
        public static void Postfix(ref LoadingScreen __instance)
        {
            itemFilterManager = FallenUtils.GetFilterManager;
            playerVisuals = PlayerFinder.getPlayerVisuals();
            itemContainersManager = ItemContainersManager.Instance;
            playerStash = StashTabbedUIControls.instance.container.containers;
            gameUiBase = UIBase.instance;
            //inventoryPanelUI = gameUiBase.inventoryPanel.instance.GetComponent<InventoryPanelUI>();
            playerData = PlayerFinder.getPlayerData();
            characterDataTracker = PlayerFinder.getPlayerDataTracker();
            expTracker = PlayerFinder.getExperienceTracker();
            goldTracker = PlayerFinder.getLocalGoldTracker();
            player = PlayerFinder.getPlayerActor();

        }
    }
}

