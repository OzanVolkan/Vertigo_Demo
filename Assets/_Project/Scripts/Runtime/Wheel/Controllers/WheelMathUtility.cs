using System;
using System.Collections.Generic;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Controllers
{
    public static class WheelMathUtility
    {
        public const float SliceAngle = 45f; // 360 / 8
        public const int SliceCount = 8;
        private const float MaxRandomOffset = 12f; // Safely within half slice (22.5)

        /// <summary>
        /// Selects a slice based on weights.
        /// </summary>
        public static int RollWeightedSliceIndex(IReadOnlyList<WheelSliceData> slices)
        {
            if (slices == null || slices.Count == 0) return 0;

            float totalWeight = 0f;
            for (int i = 0; i < slices.Count; i++)
            {
                totalWeight += Mathf.Max(0.01f, slices[i].Weight);
            }

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;

            for (int i = 0; i < slices.Count; i++)
            {
                cumulative += Mathf.Max(0.01f, slices[i].Weight);
                if (roll <= cumulative)
                {
                    return i;
                }
            }

            return slices.Count - 1;
        }

        /// <summary>
        /// Calculates the final Z rotation angle for the wheel to land on targetSliceIndex.
        /// Slices are laid out clockwise at -i * 45 degrees.
        /// Spinning clockwise means rotating in negative Z direction.
        /// To bring slice i to the top indicator (0 deg), wheel rotator local Euler Z must equal (i * 45) mod 360.
        /// </summary>
        /// <param name="targetSliceIndex">0 to 7 index</param>
        /// <param name="currentAngle">Current wheel rotation angle</param>
        /// <param name="exactCenterAngle">Calculated dead-center angle without any offset</param>
        /// <param name="minFullTurns">Number of full 360-degree spins (default 5 to 7)</param>
        /// <param name="addRandomOffset">Subtle jitter so it lands naturally within slice</param>
        public static float CalculateTargetRotation(
            int targetSliceIndex,
            float currentAngle,
            out float exactCenterAngle,
            int minFullTurns = 6,
            bool addRandomOffset = true)
        {
            // Slices are laid out clockwise at -i * 45 degrees.
            // To bring slice i to the top, target angle mod 360 is: i * 45 degrees.
            float targetSliceAngle = (targetSliceIndex * SliceAngle) % 360f;

            // Current angle normalized into [0, 360)
            float currentMod = currentAngle % 360f;
            if (currentMod < 0f) currentMod += 360f;

            // Clockwise distance (positive number of degrees in [0, 360)) from currentMod to targetSliceAngle.
            // Spinning clockwise decreases Z angle.
            float clockwiseDelta = currentMod - targetSliceAngle;
            if (clockwiseDelta < 0f) clockwiseDelta += 360f;

            // Total negative rotation: at least minFullTurns full turns plus clockwiseDelta
            float totalSpinDegrees = (minFullTurns * 360f) + clockwiseDelta;

            // Base dead-center angle relative to currentMod
            exactCenterAngle = currentMod - totalSpinDegrees;

            // Random natural offset inside the slice
            float offset = 0f;
            if (addRandomOffset)
            {
                // Visible natural offset between 5.5 and 12 degrees (well inside the 22.5 degree slice boundary)
                float sign = UnityEngine.Random.value > 0.5f ? 1f : -1f;
                offset = sign * UnityEngine.Random.Range(5.5f, MaxRandomOffset);
            }

            // Final initial landing target angle with natural jitter offset
            float targetAngle = exactCenterAngle + offset;

            return targetAngle;
        }

        public static float CalculateTargetRotation(
            int targetSliceIndex,
            float currentAngle,
            int minFullTurns = 6,
            bool addRandomOffset = true)
        {
            return CalculateTargetRotation(targetSliceIndex, currentAngle, out _, minFullTurns, addRandomOffset);
        }

        /// <summary>
        /// Returns slice index under indicator at given rotation angle.
        /// </summary>
        public static int GetSliceIndexAtAngle(float zRotation)
        {
            float positiveAngle = zRotation % 360f;
            if (positiveAngle < 0f) positiveAngle += 360f;

            // Half slice offset so center of slice is 0 deg
            float shifted = (positiveAngle + (SliceAngle / 2f)) % 360f;
            int index = Mathf.FloorToInt(shifted / SliceAngle);
            return Mathf.Clamp(index, 0, SliceCount - 1);
        }
    }
}
