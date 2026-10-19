using UnityEngine;
using Vertigo.Core.Enums;
using Vertigo.Core.Events;
using Vertigo.Zone.Data;

namespace Vertigo.Zone.Controllers
{
    public class ZoneManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private ZoneManagerConfigSO zoneConfig;

        [Header("State")]
        [SerializeField] private int currentZoneNumber = 1;

        public int CurrentZoneNumber => currentZoneNumber;
        public int MaxZone => zoneConfig != null ? zoneConfig.MaxZone : 60;
        public bool IsFinalZone => currentZoneNumber >= MaxZone;
        public ZoneData CurrentZoneData { get; private set; }
        public bool IsSafeZone => CurrentZoneData?.ZoneType == ZoneType.Safe;
        public bool IsSuperZone => CurrentZoneData?.ZoneType == ZoneType.Super;
        public bool CanCashOut => CurrentZoneData?.CanCashOut == true;

        private void OnEnable() => GameEvents.OnGameRestartRequested += ResetToStart;

        private void OnDisable() => GameEvents.OnGameRestartRequested -= ResetToStart;

        public void Initialize(int startZone = 1) => JumpToZone(startZone);

        public void AdvanceToNextZone() => JumpToZone(currentZoneNumber + 1);

        public void ResetToStart() => JumpToZone(1);

        public void JumpToZone(int targetZone)
        {
            currentZoneNumber = Mathf.Clamp(targetZone, 1, MaxZone);
            LoadCurrentZone();
        }

        private void LoadCurrentZone()
        {
            if (zoneConfig == null)
            {
                Debug.LogError("[ZoneManager] ZoneManagerConfigSO is not assigned!");
                return;
            }

            CurrentZoneData = zoneConfig.GetZoneData(currentZoneNumber);
            Debug.Log($"[ZoneManager] Entered Zone {currentZoneNumber} ({CurrentZoneData.ZoneType}) - Multiplier: {CurrentZoneData.RewardMultiplier:F2}");
            GameEvents.FireZoneChanged(CurrentZoneData);
        }
    }
}
