using UnityEngine;
using Vertigo.Core.Events;
using Vertigo.Wheel.Data;
using Vertigo.Zone.Data;

namespace Vertigo.Wheel.Controllers
{
    public class WheelController : MonoBehaviour
    {
        [Header("Spin Settings")]
        [SerializeField, Range(2f, 8f)] private float spinDuration = 4.5f;
        [SerializeField, Range(3, 10)] private int minFullTurns = 6;

        [Header("Runtime State")]
        [SerializeField] private WheelConfigSO currentConfig;
        private bool _isSpinning;
        private int _currentTargetIndex;
        private float _currentWheelAngle;

        public WheelConfigSO CurrentConfig => currentConfig;
        public bool IsSpinning => _isSpinning;

        private void OnEnable()
        {
            GameEvents.OnZoneChanged += OnZoneChanged;
            GameEvents.OnSpinRequested += RequestSpin;
        }

        private void OnDisable()
        {
            GameEvents.OnZoneChanged -= OnZoneChanged;
            GameEvents.OnSpinRequested -= RequestSpin;
        }

        public void SetWheelConfig(WheelConfigSO newConfig)
        {
            currentConfig = newConfig;
        }

        public void RequestSpin()
        {
            if (_isSpinning)
            {
                Debug.LogWarning("[WheelController] Wheel is already spinning!");
                return;
            }

            if (currentConfig == null || currentConfig.SliceCount == 0)
            {
                Debug.LogError("[WheelController] No valid wheel config loaded!");
                return;
            }

            _isSpinning = true;

            // Roll target slice based on slice weights
            _currentTargetIndex = WheelMathUtility.RollWeightedSliceIndex(currentConfig.ConcreteSlices);
            
            // Calculate target angle (with natural offset) and exact center angle
            float targetAngle = WheelMathUtility.CalculateTargetRotation(
                _currentTargetIndex,
                _currentWheelAngle,
                out float exactCenterAngle,
                minFullTurns,
                addRandomOffset: true
            );

            // After centering completes, the wheel will be resting exactly at exactCenterAngle normalized to [0, 360)
            _currentWheelAngle = (exactCenterAngle % 360f + 360f) % 360f;

            Debug.Log($"[WheelController] Spin started! Target Slice: {_currentTargetIndex} ({currentConfig.GetSlice(_currentTargetIndex)?.Reward?.DisplayName}), Landing Angle: {targetAngle:F1}, Centered Angle: {exactCenterAngle:F1}");
            GameEvents.FireSpinStarted(_currentTargetIndex, targetAngle, exactCenterAngle, spinDuration);
        }

        /// <summary>
        /// Called by WheelView when the DOTween rotation finishes.
        /// </summary>
        public void OnSpinAnimationComplete()
        {
            _isSpinning = false;
            WheelSliceData landedSlice = currentConfig.GetSlice(_currentTargetIndex);
            Debug.Log($"[WheelController] Spin completed on Slice {_currentTargetIndex}: {landedSlice?.Reward?.DisplayName} x{landedSlice?.Amount}");
            GameEvents.FireSpinCompleted(landedSlice);
        }

        private void OnZoneChanged(ZoneData zoneData)
        {
            if (zoneData?.WheelConfig != null)
            {
                SetWheelConfig(zoneData.ConcreteWheelConfig);
            }
        }
    }
}
