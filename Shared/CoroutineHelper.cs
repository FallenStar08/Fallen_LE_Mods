using System.Collections;
using MelonLoader;
using UnityEngine;

namespace Fallen_LE_Mods.Shared
{
    public static class CoroutineHelper
    {
        public static void DelayFixed(Action action)
        {
            MelonCoroutines.Start(WaitFixed(action));
        }

        public static void DelayFrames(int frames, Action action)
        {
            MelonCoroutines.Start(WaitFrames(frames, action));
        }

        public static void DelayMillis(float ms, Action action)
        {
            MelonCoroutines.Start(WaitSeconds(ms / 1000f, action));
        }

        private static IEnumerator WaitFixed(Action action)
        {
            yield return new WaitForFixedUpdate();
            action?.Invoke();
        }

        private static IEnumerator WaitFrames(int frames, Action action)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }
            action?.Invoke();
        }

        private static IEnumerator WaitSeconds(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action?.Invoke();
        }
    }
}