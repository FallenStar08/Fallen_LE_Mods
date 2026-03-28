#if RELEASE
using Fallen_LE_Mods.Shared;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Factions;
using MelonLoader;
using UnityEngine;
namespace Fallen_LE_Mods.Dev

{
    [HarmonyPatch(typeof(GroundItemManager), "dropItemForPlayer")]
    public class ItemDropHandler : MelonMod
    {


        public static bool Prefix(GroundItemManager __instance, Actor player, ItemData itemData, Vector3 location, bool playDropSound)
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

        public static void Prefix(ref GroundItemManager __instance, ref Actor player, ref int goldValue, ref Vector3 location, ref bool playDropSound)
        {
            Vector3 playerPosition = player.position();
            location = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
            playDropSound = false;

        }
    }
    [HarmonyPatch(typeof(GroundItemManager), "dropXPTomeForPlayer")]
    public class XPTomeDropHandler : MelonMod
    {
        public static void Prefix(ref GroundItemManager __instance, ref Actor player, ref int experience, ref Vector3 location, ref bool playDropSound)
        {
            Vector3 playerPosition = player.position();
            location = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);

        }
    }

    [HarmonyPatch(typeof(SilkenCocoonData), "DropMemoryAmberAfterDelay")]
    public class MemoryAmberHandler : MelonMod
    {
        public static void Prefix(UnityEngine.Vector3 position, uint quantity, float delay, PickupableObjectCondition condition)
        {
            Vector3 playerPosition = GameReferencesCache.player.position();
            position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);

        }
    }

    [HarmonyPatch(typeof(SilkenCocoonData), "DropMemoryAmberInPilesForWeaverMembers")]
    public class DropMemoryAmberInPilesForWeaverMembersHandler : MelonMod
    {
        public static void Prefix(UnityEngine.Vector3 position, int piles, int corruption, float quantityModifier)
        {

            Vector3 playerPosition = GameReferencesCache.player.position();
            position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);

        }
    }

    //[HarmonyPatch(typeof(SilkenCocoonData), "DropMemoryAmberInPilesForExplicitActor", new Type[] { typeof(UnityEngine.Vector3), typeof(int), typeof(int), typeof(float), typeof(Il2CppSystem.Func<Il2Cpp.Actor, bool>) })]

    //public class DropMemoryAmberInPilesForExplicitActorHandler : MelonMod
    //{
    //    public static void Prefix(UnityEngine.Vector3 position, int piles, int corruption, float quantityModifier, Il2CppSystem.Func<Il2Cpp.Actor, bool> actor)
    //    {
    //        Vector3 playerPosition = GameReferencesCache.player.position();
    //        position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);

    //    }
    //}

}
#endif