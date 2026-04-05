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

        private float _timer;
        private bool _isVisible = true;
        private const KeyCode ToggleKey = KeyCode.F1;

        public CornerUpdater(IntPtr ptr) : base(ptr) { }

        private void Update()
        {
            if (Input.GetKeyDown(ToggleKey)) ToggleVisibility();

            if (!_isVisible || textToUpdate == null) return;

            _timer += Time.deltaTime;
            if (_timer >= updateInterval)
            {
                _timer = 0f;
                UpdateUIText();
            }
        }

        private void ToggleVisibility()
        {
            _isVisible = !_isVisible;

            if (textToUpdate != null)
            {
                textToUpdate.gameObject.SetActive(_isVisible);
                FallenUtils.MakeNotification($"Visibility toggled: {(_isVisible ? "ON" : "OFF")}");
            }

            backgroundObject?.SetActive(_isVisible);
        }

        private void UpdateUIText()
        {
            if (textToUpdate == null) return;

            var infos = new StringBuilder();
            float elapsed = GetElapsedTime();
            const int pad = -9;

            int GetRate(float total)
            {
                return elapsed > 0 ? Mathf.Max(Mathf.RoundToInt(total / elapsed * 3600f), 0) : 0;
            }

            void AddRow(string label, object value)
            {
                infos.AppendLine($"{label,pad}: {value}");
            }

            int expRate = GetRate(TotalExp);

            var eta = expRate > 0
                ? TimeSpan.FromHours(ExpToNextLevel / (double)expRate)
                : TimeSpan.Zero;


            AddRow("Status", IsPaused ? "Paused" : "Active");
            AddRow("Gold/h", GetRate(TotalGold));
            AddRow("Exp/h", expRate);
            AddRow("lvl↑ in", $"{eta:hh\\:mm\\:ss}");
            AddRow("Rep/h", GetRate(TotalRep));
            AddRow("Favor/h", GetRate(TotalFavor));
            AddRow("DPS", Mathf.RoundToInt(Dps));
            AddRow("DPS(avg)", Mathf.RoundToInt(DamageTracker.AverageDps));
            AddRow("DPS(max)", Mathf.RoundToInt(DamageTracker.MaxDps));
            AddRow("DMG Ttl", Mathf.RoundToInt(DamageTracker.TotalDamageDealt));

            textToUpdate.text = infos.ToString();
            InfoCorner.ResizeBackgroundToText();
        }
    }
}
#endif