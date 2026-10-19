using System.Collections.Generic;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Controllers
{
    public static class WheelMathUtility
    {
        public const float SliceAngle = 45f; // 360 / 8
        public const int SliceCount = 8;
        private const float MinRandomOffset = 5.5f;
        private const float MaxRandomOffset = 12f;

        public static int RollWeightedSliceIndex(
            IReadOnlyList<WheelSliceData> slices,
            bool ignoreBombs = false,
            bool forceBomb = false)
        {
            if (slices == null || slices.Count == 0) return 0;

            if (forceBomb && !ignoreBombs)
            {
                for (int i = 0; i < slices.Count; i++)
                {
                    if (IsBombSlice(slices[i])) return i;
                }
            }

            float totalWeight = 0f;
            for (int i = 0; i < slices.Count; i++)
            {
                if (ignoreBombs && IsBombSlice(slices[i])) continue;
                totalWeight += Mathf.Max(0.01f, slices[i].Weight);
            }

            if (totalWeight <= 0f) return 0;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            int lastValidIndex = 0;

            for (int i = 0; i < slices.Count; i++)
            {
                if (ignoreBombs && IsBombSlice(slices[i])) continue;

                lastValidIndex = i;
                cumulative += Mathf.Max(0.01f, slices[i].Weight);
                if (roll <= cumulative) return i;
            }

            return lastValidIndex;
        }

        public static float CalculateTargetRotation(
            int targetSliceIndex,
            float currentAngle,
            out float exactCenterAngle,
            int minFullTurns = 6,
            bool addRandomOffset = true)
        {
            float targetSliceAngle = Mathf.Repeat(targetSliceIndex * SliceAngle, 360f);
            float currentMod = Mathf.Repeat(currentAngle, 360f);
            float clockwiseDelta = Mathf.Repeat(currentMod - targetSliceAngle, 360f);

            exactCenterAngle = currentMod - ((minFullTurns * 360f) + clockwiseDelta);

            if (!addRandomOffset) return exactCenterAngle;

            float sign = Random.value > 0.5f ? 1f : -1f;
            return exactCenterAngle + (sign * Random.Range(MinRandomOffset, MaxRandomOffset));
        }

        public static int GetSliceIndexAtAngle(float zRotation)
        {
            float shifted = Mathf.Repeat(zRotation + (SliceAngle * 0.5f), 360f);
            return Mathf.Clamp(Mathf.FloorToInt(shifted / SliceAngle), 0, SliceCount - 1);
        }

        private static bool IsBombSlice(WheelSliceData slice) =>
            slice?.Reward != null && slice.Reward.IsBomb;
    }
}
