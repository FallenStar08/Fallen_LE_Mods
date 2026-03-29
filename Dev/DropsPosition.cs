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

        public static bool Prefix(ref GroundItemManager __instance, ref Actor player, ref int goldValue, ref Vector3 location, ref bool playDropSound)
        {
            GameReferencesCache.goldTracker.modifyGold(goldValue);
            playDropSound = false;
            return false;

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



    [HarmonyPatch(typeof(GroundItemManager), "dropAncientBoneForPlayer")]
    public class dropAncientBoneForPlayerHandler : MelonMod
    {
        public static void Postfix(GroundItemManager __instance, Actor player, int amount, UnityEngine.Vector3 location, bool playDropSound, bool randomiseLocation)
        {
            if (__instance == null || GameReferencesCache.player == null || __instance.activeAncientBones == null)
                return;

            Vector3 playerPosition = GameReferencesCache.player.position();
            uint ancien_bone_id = __instance.nextAncientBoneId - 1;

            for (int i = 0; i < __instance.activeAncientBones.Count; i++)
            {
                var pick_ancien_bone_interaction = __instance.activeAncientBones[i];
                if (pick_ancien_bone_interaction.id == ancien_bone_id)
                {
                    __instance.pickupAncientBone(player, ancien_bone_id, pick_ancien_bone_interaction);
                    break;
                }
            }
        }
    }


}
#endif