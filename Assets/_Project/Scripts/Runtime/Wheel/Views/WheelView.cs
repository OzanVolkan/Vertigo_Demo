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

        [Header("Indicator Settings")]
        [Tooltip("Deflection angle when a slice peg pushes the needle")]
        [SerializeField, Range(5f, 35f)] private float indicatorKickAngle = 18f;
        [Tooltip("Duration to spring back to center")]
        [SerializeField, Range(0.05f, 0.25f)] private float indicatorSnapDuration = 0.12f;

        [Header("Centering Polish Settings")]
        [Tooltip("Pause duration after stopping before centering begins")]
        [SerializeField, Range(0.1f, 0.8f)] private float centerDelay = 0.35f;
        [Tooltip("Duration of the smooth centering rotation")]
        [SerializeField, Range(0.2f, 0.8f)] private float centerDuration = 0.45f;
        [Tooltip("Easing curve for centering rotation")]
        [SerializeField] private Ease centerEase = Ease.OutQuad;

        [Header("Dependencies")]
        [SerializeField] private WheelController wheelController;

        private Sequence _spinSequence;
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

            // Enforce indicator top hinge pivot (0.5, 0.88) so it flutters from the top pin
            if (ui_transform_indicator != null)
            {
                ui_transform_indicator.pivot = new Vector2(0.5f, 0.88f);
                ui_transform_indicator.localEulerAngles = Vector3.zero;
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
            _spinSequence?.Kill();
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

        private void OnSpinStarted(int targetIndex, float targetAngle, float exactCenterAngle, float duration)
        {
            _currentTargetIndex = targetIndex;
            _lastTickSliceIndex = -1;

            _spinSequence?.Kill();
            _spinTween?.Kill();
            _indicatorTween?.Kill();

            if (ui_transform_indicator != null)
            {
                ui_transform_indicator.pivot = new Vector2(0.5f, 0.88f);
                ui_transform_indicator.localEulerAngles = Vector3.zero;
            }

            // 1. Main Spin to landing angle (with natural offset like in Image 1)
            _spinTween = ui_transform_wheel_rotator.DOLocalRotate(new Vector3(0, 0, targetAngle), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic)
                .OnUpdate(() =>
                {
                    // Detect slice passing to trigger indicator tick
                    float currentZ = ui_transform_wheel_rotator.localEulerAngles.z;
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
                    // 2. Pause naturally on the offset position (suspense moment)
                    DOVirtual.DelayedCall(centerDelay, () =>
                    {
                        // 3. Smooth micro-rotation to center the slice under the indicator (Image 2)
                        // The exact corrective rotation from targetAngle to exactCenterAngle is (exactCenterAngle - targetAngle)
                        // which is at most ±12°, mathematically guaranteed to stay strictly within the landed slot.
                        float correctionDelta = exactCenterAngle - targetAngle;

                        if (Mathf.Abs(correctionDelta) > 0.1f)
                        {
                            Vector3 targetEuler = ui_transform_wheel_rotator.localEulerAngles;
                            targetEuler.z += correctionDelta;

                            // Smooth micro-rotation to center (strictly shortest path, never spins a full turn)
                            _spinTween = ui_transform_wheel_rotator.DOLocalRotate(targetEuler, centerDuration, RotateMode.Fast)
                                .SetEase(centerEase)
                                .OnComplete(CompleteSpin);
                        }
                        else
                        {
                            CompleteSpin();
                        }
                    });
                });
        }

        private void CompleteSpin()
        {
            _indicatorTween?.Kill();
            if (ui_transform_indicator != null)
                ui_transform_indicator.localEulerAngles = Vector3.zero;

            // Play win celebration on the centered slice (Image 2)
            if (_currentTargetIndex >= 0 && _currentTargetIndex < ui_slice_views.Count)
            {
                ui_slice_views[_currentTargetIndex]?.PlayWinAnimation();
            }

            // Notify controller
            wheelController?.OnSpinAnimationComplete();
        }

        private void TriggerIndicatorTick()
        {
            if (ui_transform_indicator == null) return;

            _indicatorTween?.Kill();
            // Peg deflects the needle tip around the top hinge; snap back to 0 with spring overshoot
            ui_transform_indicator.localEulerAngles = new Vector3(0, 0, indicatorKickAngle);
            _indicatorTween = ui_transform_indicator.DOLocalRotate(Vector3.zero, indicatorSnapDuration)
                .SetEase(Ease.OutBack, 2.5f);
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
