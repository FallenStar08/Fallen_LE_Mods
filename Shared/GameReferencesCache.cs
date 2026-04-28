using HarmonyLib;
using Il2Cpp;
using Il2CppItemFiltering;
using Il2CppLE.Factions;
using Il2CppLE.UI;
using Il2CppSystem.Linq;

namespace Fallen_LE_Mods.Shared
{
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
        public static CharacterDataTracker? playerDataTracker;
        public static AncientBonesTracker? boneTracker;
        public static Faction? faction;
        public static CraftingManager? craftingManager;
        public static MaterialContainers? materialContainers;

        public static void Postfix(LoadingScreen __instance)
        {
            itemFilterManager = FallenUtils.GetFilterManager;
            playerVisuals = PlayerFinder.getPlayerVisuals();
            itemContainersManager = ItemContainersManager.Instance;
            gameUiBase = UIBase.instance;

            if (StashTabbedUIControls.instance?.container != null)
            {
                playerStash = StashTabbedUIControls.instance.container.containers;
            }

            playerData = PlayerFinder.getPlayerData();
            playerDataTracker = PlayerFinder.getPlayerDataTracker();
            expTracker = PlayerFinder.getExperienceTracker();
            goldTracker = PlayerFinder.getLocalGoldTracker();
            boneTracker = PlayerFinder.getAncientBonesTracker();
            player = PlayerFinder.getPlayerActor();

            if (player?.factionInfo != null)
            {
                var factions = player.factionInfo.GetFactions(true, true);
                if (factions != null && factions.Count() > 0)
                {
                    faction = factions.First();
                }
            }

            if (ItemContainersManager.Instance != null)
            {
                craftingManager = ItemContainersManager.Instance.craftingManager;
                materialContainers = ItemContainersManager.Instance.materials;
            }
        }
    }
}