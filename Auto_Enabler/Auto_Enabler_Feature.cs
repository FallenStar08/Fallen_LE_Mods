using Fallen_LE_Mods.Shared;

namespace Fallen_LE_Mods.Auto_Enabler
{
    public class ProximityFeature : IFallenFeature
    {
        public void OnMelonInitialize()
        {
            UniversalProximityManager.Initialize();
        }
    }
}
