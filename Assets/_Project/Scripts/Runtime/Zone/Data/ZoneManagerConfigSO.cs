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

        [Tooltip("How much the reward multiplier increases per zone (e.g., 0.25 = +25% per zone)")]
        [SerializeField] private float multiplierStepPerZone = 0.25f;

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
        public float MultiplierStepPerZone => multiplierStepPerZone;
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
            public float multiplier;
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
                    return new ZoneData(zoneNumber, type, zoneOverrides[i].wheelConfig, zoneOverrides[i].multiplier);
                }
            }

            ZoneType calculatedType = DetermineZoneType(zoneNumber);
            WheelConfigSO selectedConfig = calculatedType switch
            {
                ZoneType.Super => defaultGoldenWheel,
                ZoneType.Safe => defaultSilverWheel,
                _ => defaultNormalWheel
            };

            // Scaled multiplier: increases every zone so slice rewards get better each zone
            float multiplier = 1f + (zoneNumber - 1) * Mathf.Max(0f, multiplierStepPerZone);

            return new ZoneData(zoneNumber, calculatedType, selectedConfig, multiplier);
        }
    }
}
