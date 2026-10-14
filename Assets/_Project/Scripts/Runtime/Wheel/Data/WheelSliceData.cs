using System;
using UnityEngine;
using Vertigo.Core.Interfaces;
using Vertigo.Rewards.Data;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public class WheelSliceData : IWheelSliceData
    {
        [SerializeField] private int sliceIndex;
        [SerializeField] private RewardDataSO reward;
        [SerializeField] private int amount = 1;
        [SerializeField, Range(0.1f, 100f)] private float weight = 1f;

        public int SliceIndex => sliceIndex;
        public IRewardData Reward => reward;
        public RewardDataSO RewardSO => reward;
        public int Amount => amount;
        public float Weight => weight;

        public WheelSliceData() { }

        public WheelSliceData(int index, RewardDataSO rewardSO, int amount, float weight = 1f)
        {
            this.sliceIndex = index;
            this.reward = rewardSO;
            this.amount = amount;
            this.weight = weight;
        }

        public void SetIndex(int index) => this.sliceIndex = index;
    }
}
