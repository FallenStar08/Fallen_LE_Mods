using Fallen_LE_Mods.Shared;

namespace Fallen_LE_Mods.Dev
{
    public class QuickShatterFeature : IFallenFeature
    {
        public void OnMelonInitialize()
        {
            QuickShatter.Initialize();

        }
    }
}
