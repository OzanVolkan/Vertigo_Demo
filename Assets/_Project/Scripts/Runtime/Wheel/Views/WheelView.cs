using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Enums;
using Vertigo.Core.Events;
using Vertigo.Wheel.Controllers;
using Vertigo.Wheel.Data;
using Vertigo.Zone.Data;

namespace Vertigo.Wheel.Views
{
    public class WheelView : MonoBehaviour
    {
        [Header("Wheel Transforms")]
        [SerializeField] private RectTransform ui_transform_wheel_rotator;
        [SerializeField] private Image ui_image_wheel_base;
        [SerializeField] private RectTransform ui_transform_indicator;
        [SerializeField] private Image ui_image_wheel_indicator;
        [SerializeField] private Button ui_button_spin;

        [Header("Slices")]
        [SerializeField] private List<WheelSliceView> ui_slice_views = new List<WheelSliceView>();

        [Header("Dependencies")]
        [SerializeField] private WheelController wheelController;

        private Tween _spinTween;
        private Tween _indicatorTween;
        private int _lastTickSliceIndex = -1;
        private int _currentTargetIndex = -1;

        private void Awake()
        {
            if (ui_button_spin != null)
            {
                ui_button_spin.onClick.AddListener(OnSpinButtonClicked);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnZoneChanged += OnZoneChanged;
            GameEvents.OnSpinStarted += OnSpinStarted;
            GameEvents.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnZoneChanged -= OnZoneChanged;
            GameEvents.OnSpinStarted -= OnSpinStarted;
            GameEvents.OnGameStateChanged -= OnGameStateChanged;
            _spinTween?.Kill();
            _indicatorTween?.Kill();
        }

        private void OnSpinButtonClicked()
        {
            GameEvents.FireSpinRequested();
        }

        private void OnGameStateChanged(GameState state)
        {
            if (ui_button_spin != null)
            {
                ui_button_spin.interactable = (state == GameState.Idle);
            }
        }

        private void OnZoneChanged(ZoneData zone)
        {
            if (zone?.WheelConfig == null) return;
            var config = zone.ConcreteWheelConfig;

            // Update Base and Indicator sprites
            if (ui_image_wheel_base != null && config.WheelBaseSprite != null)
                ui_image_wheel_base.sprite = config.WheelBaseSprite;

            if (ui_image_wheel_indicator != null && config.IndicatorSprite != null)
                ui_image_wheel_indicator.sprite = config.IndicatorSprite;

            // Populate 8 slices
            for (int i = 0; i < ui_slice_views.Count; i++)
            {
                if (ui_slice_views[i] != null && i < config.SliceCount)
                {
                    ui_slice_views[i].Setup(config.GetSlice(i));
                }
            }
        }

        private void OnSpinStarted(int targetIndex, float targetAngle, float duration)
        {
            _currentTargetIndex = targetIndex;
            _lastTickSliceIndex = -1;

            _spinTween?.Kill();
            _indicatorTween?.Kill();

            if (ui_transform_indicator != null)
                ui_transform_indicator.localRotation = Quaternion.identity;

            // Spin with realistic easing curve
            _spinTween = ui_transform_wheel_rotator.DORotate(new Vector3(0, 0, targetAngle), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic)
                .OnUpdate(() =>
                {
                    // Detect slice passing to trigger indicator punch tick
                    float currentZ = ui_transform_wheel_rotator.eulerAngles.z;
                    int currentSlice = WheelMathUtility.GetSliceIndexAtAngle(currentZ);
                    if (currentSlice != _lastTickSliceIndex)
                    {
                        _lastTickSliceIndex = currentSlice;
                        TriggerIndicatorTick();
                        GameEvents.FireSliceTick();
                    }
                })
                .OnComplete(() =>
                {
                    if (ui_transform_indicator != null)
                        ui_transform_indicator.localRotation = Quaternion.identity;

                    // Play win animation on the landed slice
                    if (_currentTargetIndex >= 0 && _currentTargetIndex < ui_slice_views.Count)
                    {
                        ui_slice_views[_currentTargetIndex]?.PlayWinAnimation();
                    }

                    // Notify controller
                    wheelController?.OnSpinAnimationComplete();
                });
        }

        private void TriggerIndicatorTick()
        {
            if (ui_transform_indicator == null) return;

            _indicatorTween?.Kill();
            // Subtle spring tick punch on the indicator
            _indicatorTween = ui_transform_indicator.DOPunchRotation(new Vector3(0, 0, -18f), 0.12f, 1, 0.4f);
        }

        private void OnValidate()
        {
            if (ui_button_spin == null)
            {
                var tr = transform.Find("ui_button_spin");
                if (tr != null) ui_button_spin = tr.GetComponent<Button>();
            }

            if (wheelController == null)
            {
                wheelController = FindFirstObjectByType<WheelController>();
            }

            if (ui_slice_views == null || ui_slice_views.Count == 0)
            {
                ui_slice_views = new List<WheelSliceView>(GetComponentsInChildren<WheelSliceView>(true));
            }
        }
    }
}
