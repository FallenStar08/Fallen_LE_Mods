#if RELEASE
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Factions;
using MelonLoader;
using UnityEngine;

namespace Fallen_LE_Mods.Dev
{

    public class GameStatsTracker : MelonMod
    {
        public static bool IsPaused { get; private set; }
        public static float TotalGold { get; private set; }
        public static float TotalExp { get; private set; }
        public static float TotalRep { get; private set; }
        public static float TotalFavor { get; private set; }
        public static float ExpToNextLevel { get; private set; }
        public static float Dps { get; private set; }

        private static float _startTime;
        private static float _pausedTime;

        public override void OnInitializeMelon()
        {
            _startTime = Time.time;
        }



        public static float GetElapsedTime()
        {
            return IsPaused ? _pausedTime - _startTime : Time.time - _startTime;
        }

        public static void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            _pausedTime = Time.time;
        }

        public static void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            _startTime += Time.time - _pausedTime;
        }

        [HarmonyPatch(typeof(GoldTracker), "modifyGold")]
        public class GoldTrackerUpdater
        {
            private static void Postfix(int changeValue)
            {
                if (changeValue >= 0) TotalGold += changeValue;
            }
        }

        [HarmonyPatch(typeof(ExperienceTracker), "GainExp")]
        public class ExpTrackerUpdater
        {
            private static void Postfix(long characterExp)
            {
                if (characterExp >= 0) TotalExp += characterExp;
            }
        }

        [HarmonyPatch(typeof(ExperienceTracker), "SetExp")]
        public class ExpToNextLevelTracker
        {
            private static void Postfix(ExperienceTracker __instance, long newExp)
            {
                ExpToNextLevel = __instance.NextLevelExperience - newExp;
            }
        }

        [HarmonyPatch(typeof(Faction), "GainReputation")]
        public class ReputationTracker
        {
            private static void Postfix(ref Faction __instance, ref int value)
            {
                if (value >= 0)
                {
                    TotalRep += value;
                }

            }
        }

        [HarmonyPatch(typeof(Faction), "GainFavor")]
        public class FavorTracker
        {
            private static float previousFavor = 0f;
            private static bool gotFirstFavorValue = false;
            private static void Postfix(Faction __instance, int gainedFavor, bool ignoreRepGain, bool ignoreMultiplier)
            {
                if (gainedFavor >= 0)
                {
                    TotalFavor += gainedFavor;
                }

            }
        }

        [HarmonyPatch(typeof(RelayDamageEvents), "AddDamage")]
        public class DamageTracker
        {
            public static float TotalDamageDealt { get; private set; }
            public static float AverageDps { get; private set; }
            public static float MaxDps { get; private set; }
            public static float MinDps { get; private set; } = float.MaxValue;

            private static readonly Queue<DamageEntry> _damageQueue = new();
            private const float TrackingWindow = 10f;

            private static void Postfix(RelayDamageEvents __instance, float additionalDamage)
            {
                if (__instance.ToString().Contains("MainPlayer")) return;

                float now = Time.time;

                _damageQueue.Enqueue(new DamageEntry(additionalDamage, now));
                TotalDamageDealt += additionalDamage;

                while (_damageQueue.Count > 0 && now - _damageQueue.Peek().time > TrackingWindow)
                {
                    _damageQueue.Dequeue();
                }

                float windowDamage = 0f;
                foreach (var entry in _damageQueue)
                {
                    windowDamage += entry.damage;
                }

                float windowDuration = Mathf.Max(now - (_damageQueue.Count > 0 ? _damageQueue.Peek().time : now), 0.1f);

                Dps = windowDamage / windowDuration;

                AverageDps = windowDamage / TrackingWindow;

                MaxDps = Mathf.Max(MaxDps, Dps);
                MinDps = Mathf.Min(MinDps, Dps);
            }

            private record DamageEntry(float damage, float time);
        }
    }
}




#endif