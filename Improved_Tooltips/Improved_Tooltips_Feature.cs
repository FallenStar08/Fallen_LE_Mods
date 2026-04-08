using Fallen_LE_Mods.Shared;
using HarmonyLib;
using Il2Cpp;

namespace Fallen_LE_Mods.Improved_Tooltips
{
    public class ImprovedTooltipsFeature : IFallenFeature
    {
        public void OnMelonInitialize()
        {
            GroundLabelManager.Initialize();

        }

        public void OnMelonLateInitialize()
        {
            var targetMethod = AccessTools.Method(typeof(GroundItemLabel), "SetGroundTooltipText", new Type[] { typeof(bool) });
            if (targetMethod != null)
            {
                var patch = new HarmonyMethod(AccessTools.Method(typeof(GroundLabelManager.GroundLabelPatch), "Postfix"));
                FallenUtils.Harmony.Patch(targetMethod, null, patch);
            }
        }
    }
}
