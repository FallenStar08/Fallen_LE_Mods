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
        public static bool IsPaused { get; private set; } = false;
        public static float totalGold = 0f;
        public static float goldPerHour = 0f;
        public static float totalExp = 0f;
        public static float expPerHour = 0f;
        public static float totalRep = 0f;
        public static float repPerHour = 0f;
        public static float totalFavor = 0f;
        public static float favorPerHour = 0f;
        public static float memoryAmberPerHour = 0f;
        public static float dps = 0f;
        private static float startTime;
        private static float pausedTime;
        public static float expToNextLevel;

        public override void OnInitializeMelon()
        {
            startTime = Time.time;
        }

        //public static float GetElapsedTime() 
        //{
        //    return (Time.time - startTime);
        //}

        public static float GetElapsedTime()
        {
            return IsPaused ? pausedTime - startTime : Time.time - startTime;
        }

        public static void Pause()
        {
            if (!IsPaused)
            {
                IsPaused = true;
                pausedTime = Time.time;
            }
        }

        public static void Resume()
        {
            if (IsPaused)
            {
                IsPaused = false;
                startTime += Time.time - pausedTime; // Adjust startTime to compensate for paused time
            }
        }

        [HarmonyPatch(typeof(GoldTracker), "modifyGold")]
        public class GoldTrackerUpdater
        {
            private static void Postfix(ref GoldTracker __instance, ref int changeValue)
            {
                //Melon<MyMod>.Logger.Msg("Gold Tracker updated");
                if (changeValue >= 0)
                {
                    totalGold += changeValue;
                }

            }
        }

        [HarmonyPatch(typeof(ExperienceTracker), "GainExp")]
        public class ExpTrackerUpdater
        {
            private static void Postfix(ref ExperienceTracker __instance, ref long characterExp, ref long abilityExp, ref long expForFavourGain)
            {
                //Melon<MyMod>.Logger.Msg("Exp Tracker updated");
                if (characterExp >= 0)
                {
                    totalExp += characterExp;
                }

            }
        }

        [HarmonyPatch(typeof(ExperienceTracker), "SetExp")]
        public class ExpToNextLevelTracker
        {
            private static void Postfix(ref ExperienceTracker __instance, ref long newExp)
            {
                //Melon<MyMod>.Logger.Msg("Exp Tracker updated");
                expToNextLevel = __instance.NextLevelExperience - newExp;
            }
        }

        [HarmonyPatch(typeof(Faction), "GainReputation")]
        public class ReputationTracker
        {
            private static void Postfix(ref Faction __instance, ref int value)
            {
                //Melon<MyMod>.Logger.Msg($"Gained {value} reputation");
                if (value >= 0)
                {
                    totalRep += value;
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
                    totalFavor += gainedFavor;
                }

            }
        }

        [HarmonyPatch(typeof(RelayDamageEvents), "AddDamage")]
        public class DamageTracker
        {
            private class DamageEntry
            {
                public float damage;
                public float time;
                public DamageEntry(float damage, float time)
                {
                    this.damage = damage;
                    this.time = time;
                }
            }

            private static readonly Queue<DamageEntry> damageQueue = new();
            private static float trackingWindow = 10f;
            private static float lastCleanupTime = 0f;
            private static float cleanupInterval = 1f;

            public static float totalDamageDealt = 0f;
            public static float averageDps = 0f;
            public static float maxDps = 0f;
            public static float minDps = float.MaxValue;

            private static void Postfix(ref RelayDamageEvents __instance, ref float additionalDamage)
            {
                if (__instance.ToString().Contains("MainPlayer"))
                {
                    return;
                }

                float now = Time.time;

                damageQueue.Enqueue(new DamageEntry(additionalDamage, now));
                totalDamageDealt += additionalDamage;

                while (damageQueue.Count > 0 && now - damageQueue.Peek().time > trackingWindow)
                {
                    damageQueue.Dequeue();
                }

                float windowStart = now - trackingWindow;
                float windowDamage = 0f;

                foreach (var entry in damageQueue)
                {
                    if (entry.time >= windowStart)
                        windowDamage += entry.damage;
                }

                float windowDuration = Mathf.Max(now - (damageQueue.Count > 0 ? damageQueue.Peek().time : now), 0.1f);
                dps = windowDamage / windowDuration;

                averageDps = windowDamage / trackingWindow;
                if (dps > maxDps) maxDps = dps;
                if (dps < minDps) minDps = dps;

                if (now - lastCleanupTime > cleanupInterval)
                {
                    lastCleanupTime = now;
                }
            }
        }




    }

}


#endif