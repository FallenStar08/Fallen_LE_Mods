using HarmonyLib;
using Il2Cpp;
using MelonLoader;


namespace Fallen_LE_Mods.Dev
{
    public class AffixDisplayManager : MelonMod
    {
        [HarmonyPatch(typeof(TooltipItemManager), "AffixFormatter")]
        public class TooltipItemManagerPatch
        {
            private static void Postfix(ItemDataUnpacked item, ItemAffix affix, SP modProperty, AT tags, ref string __result)
            {

            }
        }

    }
}
