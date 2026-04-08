namespace Fallen_LE_Mods.Shared
{
    public interface IFallenFeature
    {
        void OnMelonInitialize() { }

        void OnMelonLateInitialize() { }
        void OnMelonSceneLoaded(string sceneName) { }
    }
}
