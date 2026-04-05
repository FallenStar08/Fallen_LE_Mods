#if RELEASE
using System.Text;
using Fallen_LE_Mods.Dev;
using Fallen_LE_Mods.Shared;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using static Fallen_LE_Mods.Dev.GameStatsTracker;

namespace Fallen_LE_Mods.MonoScripts
{
    [RegisterTypeInIl2Cpp]
    public class CornerUpdater : MonoBehaviour
    {
        public TextMeshProUGUI? textToUpdate;
        public GameObject? backgroundObject;
        public float updateInterval = 1f;

        private float timer = 0f;
        private bool isVisible = true;
        private const KeyCode ToggleKey = KeyCode.F1;

        public CornerUpdater(IntPtr ptr) : base(ptr) { }

        private void Update()
        {
            if (Input.GetKeyDown(ToggleKey))
            {
                ToggleVisibility();
            }

            if (!isVisible || textToUpdate == null) return;

            timer += Time.deltaTime;
            if (timer >= updateInterval)
            {
                timer = 0f;
                UpdateUIText();
            }
        }

        private void ToggleVisibility()
        {
            isVisible = !isVisible;

            if (textToUpdate != null)
            {
                textToUpdate.gameObject.SetActive(isVisible);
                FallenUtils.MakeNotification($"Visibility toggled : {(isVisible ? "ON" : "OFF")}");
            }


            backgroundObject?.SetActive(isVisible);
        }

        private void UpdateUIText()
        {
            StringBuilder infos = new();

            int expPerHour = Mathf.Max(Mathf.RoundToInt(totalExp / GetElapsedTime() * 3600f), 0);
            TimeSpan estimatedTimeToLevelUp = expPerHour > 0
                ? TimeSpan.FromHours((double)expToNextLevel / expPerHour)
                : TimeSpan.Zero;

            infos.AppendLine($"{"Status",-8}: {(IsPaused ? "Paused" : "Active")}");
            infos.AppendLine($"{"Gold/h",-8}: {Mathf.Max(Mathf.RoundToInt(totalGold / GetElapsedTime() * 3600f), 0)}");
            infos.AppendLine($"{"Exp/h",-8}: {expPerHour}");
            infos.AppendLine($"{"lvl↑ in",-8}: {estimatedTimeToLevelUp:hh\\:mm\\:ss}");
            infos.AppendLine($"{"Rep/h",-8}: {Mathf.Max(Mathf.RoundToInt(totalRep / GetElapsedTime() * 3600f), 0)}");
            infos.AppendLine($"{"Favor/h",-8}: {Mathf.Max(Mathf.RoundToInt(totalFavor / GetElapsedTime() * 3600f), 0)}");
            infos.AppendLine($"{"DPS",-8}: {Mathf.RoundToInt(dps)}");
            infos.AppendLine($"{"DPS(avg)",-8}: {Mathf.RoundToInt(DamageTracker.averageDps)}");
            infos.AppendLine($"{"DPS(max)",-8}: {Mathf.RoundToInt(DamageTracker.maxDps)}");
            infos.AppendLine($"{"DMG Ttl",-8}: {Mathf.RoundToInt(DamageTracker.totalDamageDealt)}");
            textToUpdate.text = infos.ToString();
            InfoCorner.ResizeBackgroundToText();
        }
    }
}
#endif