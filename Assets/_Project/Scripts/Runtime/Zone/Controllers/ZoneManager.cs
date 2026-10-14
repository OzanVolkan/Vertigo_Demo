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

        private ZoneData _currentZoneData;

        public int CurrentZoneNumber => currentZoneNumber;
        public ZoneData CurrentZoneData => _currentZoneData;
        public bool IsSafeZone => _currentZoneData != null && _currentZoneData.ZoneType == ZoneType.Safe;
        public bool IsSuperZone => _currentZoneData != null && _currentZoneData.ZoneType == ZoneType.Super;
        public bool CanCashOut => _currentZoneData != null && _currentZoneData.CanCashOut;

        private void OnEnable()
        {
            GameEvents.OnGameRestartRequested += OnRestartRequested;
        }

        private void OnDisable()
        {
            GameEvents.OnGameRestartRequested -= OnRestartRequested;
        }

        public void Initialize(int startZone = 1)
        {
            currentZoneNumber = Mathf.Max(1, startZone);
            LoadCurrentZone();
        }

        public void AdvanceToNextZone()
        {
            currentZoneNumber++;
            LoadCurrentZone();
        }

        public void ResetToStart()
        {
            currentZoneNumber = 1;
            LoadCurrentZone();
        }

        private void LoadCurrentZone()
        {
            if (zoneConfig == null)
            {
                Debug.LogError("[ZoneManager] ZoneManagerConfigSO is not assigned!");
                return;
            }

            _currentZoneData = zoneConfig.GetZoneData(currentZoneNumber);
            Debug.Log($"[ZoneManager] Entered Zone {currentZoneNumber} ({_currentZoneData.ZoneType}) - Multiplier: {_currentZoneData.RewardMultiplier:F2}");
            GameEvents.FireZoneChanged(_currentZoneData);
        }

        private void OnRestartRequested()
        {
            ResetToStart();
        }
    }
}
