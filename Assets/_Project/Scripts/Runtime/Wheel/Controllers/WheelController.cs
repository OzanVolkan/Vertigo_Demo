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
        [SerializeField] private bool disableBombChance = false;
        [SerializeField] private bool forceBombChance = false;

        private bool _isSpinning;
        private int _currentTargetIndex;
        private float _currentWheelAngle;

        public WheelConfigSO CurrentConfig => currentConfig;

        public bool DisableBombChance
        {
            get => disableBombChance;
            set => disableBombChance = value;
        }

        public bool ForceBombChance
        {
            get => forceBombChance;
            set => forceBombChance = value;
        }

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

        public void SetWheelConfig(WheelConfigSO newConfig) => currentConfig = newConfig;

        public void ResetSpinState() => _isSpinning = false;

        public void RequestSpin()
        {
            if (_isSpinning || currentConfig == null || currentConfig.SliceCount == 0) return;

            _isSpinning = true;
            _currentTargetIndex = WheelMathUtility.RollWeightedSliceIndex(
                currentConfig.ConcreteSlices,
                disableBombChance,
                forceBombChance
            );

            float targetAngle = WheelMathUtility.CalculateTargetRotation(
                _currentTargetIndex,
                _currentWheelAngle,
                out float exactCenterAngle,
                minFullTurns
            );

            _currentWheelAngle = Mathf.Repeat(exactCenterAngle, 360f);
            GameEvents.FireSpinStarted(_currentTargetIndex, targetAngle, exactCenterAngle, spinDuration);
        }

        public void OnSpinAnimationComplete()
        {
            _isSpinning = false;
            GameEvents.FireSpinCompleted(currentConfig.GetSlice(_currentTargetIndex));
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
