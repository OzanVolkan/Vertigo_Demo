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
        /// Spinning clockwise means rotating in negative Z direction.
        /// </summary>
        /// <param name="targetSliceIndex">0 to 7 index</param>
        /// <param name="currentAngle">Current wheel rotation angle</param>
        /// <param name="minFullTurns">Number of full 360-degree spins (default 5 to 7)</param>
        /// <param name="addRandomOffset">Subtle jitter so it lands naturally within slice</param>
        public static float CalculateTargetRotation(
            int targetSliceIndex,
            float currentAngle,
            int minFullTurns = 6,
            bool addRandomOffset = true)
        {
            // Normalize current angle into [0, 360)
            float normalizedCurrent = currentAngle % 360f;
            if (normalizedCurrent > 0) normalizedCurrent -= 360f;

            // In our layout:
            // Slice 0 is at 0 degrees (Top)
            // Slices are laid out clockwise at -i * 45 degrees.
            // To bring slice i to the top, target base angle is: i * 45 degrees.
            float targetSliceAngle = targetSliceIndex * SliceAngle;

            float offset = 0f;
            if (addRandomOffset)
            {
                // Jitter within [-12, +12] degrees (well inside the 45 degree slice boundaries)
                offset = UnityEngine.Random.Range(-MaxRandomOffset, MaxRandomOffset);
            }

            // Total negative rotation for clockwise spin
            float turnsAngle = minFullTurns * 360f;
            
            // Calculate final absolute negative rotation target
            // We want (finalAngle % 360) to equal (targetSliceAngle + offset)
            float targetAngle = normalizedCurrent - turnsAngle - (360f - targetSliceAngle) + offset;
            
            return targetAngle;
        }

        /// <summary>
        /// Returns slice index under indicator at given rotation angle.
        /// </summary>
        public static int GetSliceIndexAtAngle(float zRotation)
        {
            float positiveAngle = (-zRotation) % 360f;
            if (positiveAngle < 0f) positiveAngle += 360f;

            // Half slice offset so center of slice is 0 deg
            float shifted = (positiveAngle + (SliceAngle / 2f)) % 360f;
            int index = Mathf.FloorToInt(shifted / SliceAngle);
            return Mathf.Clamp(index, 0, SliceCount - 1);
        }
    }
}
