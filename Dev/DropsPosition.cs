#if RELEASE
using Fallen_LE_Mods.Shared;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Factions;
using Il2CppLE.Gameplay.PrimalHunt;
using MelonLoader;
using UnityEngine;
namespace Fallen_LE_Mods.Dev

{
    [HarmonyPatch(typeof(GroundItemManager), "dropItemForPlayer")]
    public class ItemDropHandler : MelonMod
    {


        public static bool Prefix(GroundItemManager __instance, Actor player, ItemData itemData, ref Vector3 location, bool playDropSound)
        {
            if (!ItemList.isCraftingItem(itemData.itemType) && !ItemList.isWovenEcho(itemData.itemType) && !Item.isKey(itemData.itemType))
                return true;

            Vector3 playerPosition = player.position();
            location = playerPosition;

            if (!ItemContainersManager.Instance.attemptToPickupItem(itemData, playerPosition))
                return true;

            ItemContainersManager.Instance.TryStoreMaterials(player);
            return false;
        }

    }




    [HarmonyPatch(typeof(GroundItemManager), "dropGoldForPlayer", new Type[] { typeof(Actor), typeof(int), typeof(Vector3), typeof(bool) })]
    public class GoldDropHandler : MelonMod
    {

        public static bool Prefix(GroundItemManager __instance, Actor player, int goldValue, ref Vector3 location, ref bool playDropSound)
        {
            if (GameReferencesCache.goldTracker == null) return true;
            GameReferencesCache.goldTracker.modifyGold(goldValue);
            playDropSound = false;
            return false;

        }
    }
    [HarmonyPatch(typeof(GroundItemManager), "dropXPTomeForPlayer")]
    public class XPTomeDropHandler : MelonMod
    {
        public static void Prefix(GroundItemManager __instance, Actor player, int experience, ref Vector3 location, bool playDropSound)
        {
            if (GameReferencesCache.player == null) return;
            Vector3 playerPosition = GameReferencesCache.player.position();
            location = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);

        }
    }

    [HarmonyPatch(typeof(SilkenCocoonData), "DropMemoryAmberAfterDelay")]
    public class MemoryAmberHandler : MelonMod
    {
        public static void Prefix(ref UnityEngine.Vector3 position, uint quantity, float delay, PickupableObjectCondition condition)
        {
            if (GameReferencesCache.player == null) return;
            Vector3 playerPosition = GameReferencesCache.player.position();
            position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);

        }
    }

    [HarmonyPatch(typeof(SilkenCocoonData), "DropMemoryAmberInPilesForWeaverMembers")]
    public class DropMemoryAmberInPilesForWeaverMembersHandler : MelonMod
    {
        public static void Prefix(ref UnityEngine.Vector3 position, int piles, int corruption, float quantityModifier)
        {
            if (GameReferencesCache.player == null) return;
            Vector3 playerPosition = GameReferencesCache.player.position();
            position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);

        }
    }



    [HarmonyPatch(typeof(GroundItemManager), "dropAncientBoneForPlayer")]
    public class DropAncientBoneForPlayerHandler : MelonMod
    {
        public static bool Prefix(GroundItemManager __instance, Actor player, int amount, ref UnityEngine.Vector3 location, ref bool playDropSound, ref bool randomiseLocation)
        {
            if (GameReferencesCache.boneTracker == null) return true;
            GameReferencesCache.boneTracker.modifyAncientBones(amount);
            playDropSound = false;
            return false;
        }

    }


}
#endif