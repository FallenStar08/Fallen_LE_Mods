using Fallen_LE_Mods.Dev;
using MelonLoader;
using UnityEngine;

namespace Fallen_LE_Mods.MonoScripts
{
    [RegisterTypeInIl2Cpp]
    public class ShrineAutoRegister : MonoBehaviour
    {
        public ShrineAutoRegister(IntPtr ptr) : base(ptr) { }
        void Start()
        {
            ShrineProximityManager.RegisterShrine(gameObject);
        }

        void OnDestroy()
        {
            ShrineProximityManager.UnregisterShrine(gameObject);
        }
    }
}