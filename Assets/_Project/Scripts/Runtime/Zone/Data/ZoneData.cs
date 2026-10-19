using System;
using UnityEngine;
using Vertigo.Core.Enums;
using Vertigo.Core.Interfaces;
using Vertigo.Wheel.Data;

namespace Vertigo.Zone.Data
{
    [Serializable]
    public class ZoneData : IZoneConfig
    {
        [SerializeField] private int zoneNumber;
        [SerializeField] private ZoneType zoneType;
        [SerializeField] private WheelConfigSO wheelConfig;
        [SerializeField] private float rewardMultiplier = 1f;

        public int ZoneNumber => zoneNumber;
        public ZoneType ZoneType => zoneType;
        public IWheelConfig WheelConfig => wheelConfig;
        public WheelConfigSO ConcreteWheelConfig => wheelConfig;
        public float RewardMultiplier => rewardMultiplier;

        // Player can cash out during safe or super zones as specified in document
        public bool CanCashOut => zoneType == ZoneType.Safe || zoneType == ZoneType.Super;

        public ZoneData(int zoneNumber, ZoneType zoneType, WheelConfigSO wheelConfig, float rewardMultiplier = 1f)
        {
            this.zoneNumber = zoneNumber;
            this.zoneType = zoneType;
            this.wheelConfig = wheelConfig;
            this.rewardMultiplier = rewardMultiplier;
        }
    }
}
