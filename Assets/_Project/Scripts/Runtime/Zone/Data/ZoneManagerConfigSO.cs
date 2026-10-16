using System;
using System.Collections.Generic;
using UnityEngine;
using Vertigo.Core.Enums;
using Vertigo.Wheel.Data;

namespace Vertigo.Zone.Data
{
    [CreateAssetMenu(fileName = "ZoneConfig_", menuName = "Vertigo/Zone/Zone Manager Config")]
    public class ZoneManagerConfigSO : ScriptableObject
    {
        [Header("Zone Progression Limits")]
        [Tooltip("Final zone number where the run completes and automatically cashes out")]
        [SerializeField] private int maxZone = 60;

        [Tooltip("How much the integer reward multiplier increases per zone (1 = +1x per zone: x1, x2, x3...)")]
        [SerializeField] private int multiplierStepPerZone = 1;

        [Header("Zone Intervals")]
        [Tooltip("Interval for Safe Zones (Silver Spin without bomb)")]
        [SerializeField] private int safeZoneInterval = 5;

        [Tooltip("Interval for Super Zones (Golden Spin without bomb)")]
        [SerializeField] private int superZoneInterval = 30;

        [Header("Default Wheel Configs")]
        [SerializeField] private WheelConfigSO defaultNormalWheel;
        [SerializeField] private WheelConfigSO defaultSilverWheel;
        [SerializeField] private WheelConfigSO defaultGoldenWheel;

        [Header("Zone Custom Overrides (Optional)")]
        [SerializeField] private List<ZoneOverrideEntry> zoneOverrides = new List<ZoneOverrideEntry>();

        public int MaxZone => Mathf.Max(1, maxZone);
        public int MultiplierStepPerZone => Mathf.Max(1, multiplierStepPerZone);
        public int SafeZoneInterval => safeZoneInterval;
        public int SuperZoneInterval => superZoneInterval;
        public WheelConfigSO DefaultNormalWheel => defaultNormalWheel;
        public WheelConfigSO DefaultSilverWheel => defaultSilverWheel;
        public WheelConfigSO DefaultGoldenWheel => defaultGoldenWheel;

        [Serializable]
        public struct ZoneOverrideEntry
        {
            public int zoneNumber;
            public WheelConfigSO wheelConfig;
            public int multiplier;
        }

        public ZoneType DetermineZoneType(int zoneNumber)
        {
            if (zoneNumber <= 0) zoneNumber = 1;

            if (zoneNumber % superZoneInterval == 0)
            {
                return ZoneType.Super;
            }
            if (zoneNumber % safeZoneInterval == 0)
            {
                return ZoneType.Safe;
            }
            return ZoneType.Normal;
        }

        public ZoneData GetZoneData(int zoneNumber)
        {
            if (zoneNumber <= 0) zoneNumber = 1;

            // Check manual override first
            for (int i = 0; i < zoneOverrides.Count; i++)
            {
                if (zoneOverrides[i].zoneNumber == zoneNumber)
                {
                    ZoneType type = DetermineZoneType(zoneNumber);
                    return new ZoneData(zoneNumber, type, zoneOverrides[i].wheelConfig, Mathf.Max(1, zoneOverrides[i].multiplier));
                }
            }

            ZoneType calculatedType = DetermineZoneType(zoneNumber);
            WheelConfigSO selectedConfig = calculatedType switch
            {
                ZoneType.Super => defaultGoldenWheel,
                ZoneType.Safe => defaultSilverWheel,
                _ => defaultNormalWheel
            };

            // Whole-integer multiplier: Zone 1 = x1, Zone 2 = x2, Zone 3 = x3...
            int multiplier = 1 + (zoneNumber - 1) * Mathf.Max(1, multiplierStepPerZone);

            return new ZoneData(zoneNumber, calculatedType, selectedConfig, multiplier);
        }

        private void OnValidate()
        {
            if (maxZone < 1) maxZone = 60;
            if (multiplierStepPerZone < 1) multiplierStepPerZone = 1;
        }
    }
}
