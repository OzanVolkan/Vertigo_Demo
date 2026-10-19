using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Enums;
using Vertigo.Core.Events;
using Vertigo.Wheel.Controllers;
using Vertigo.Zone.Data;

namespace Vertigo.Wheel.Views
{
    public class WheelView : MonoBehaviour
    {
        private static readonly Vector2 IndicatorHingePivot = new Vector2(0.5f, 0.88f);

        [Header("Wheel Transforms")]
        [SerializeField] private RectTransform ui_transform_wheel_rotator;
        [SerializeField] private Image ui_image_wheel_base;
        [SerializeField] private RectTransform ui_transform_indicator;
        [SerializeField] private Image ui_image_wheel_indicator;
        [SerializeField] private Button ui_button_spin;

        [Header("Wheel Header & Footer Labels")]
        [SerializeField] private TMP_Text ui_text_wheel_title_value;
        [SerializeField] private TMP_Text ui_text_wheel_subtitle_value;
        [SerializeField] private string bronzeTitle = "BRONZE SPIN";
        [SerializeField] private string silverTitle = "SILVER SPIN";
        [SerializeField] private string goldTitle = "GOLD SPIN";
        [SerializeField] private string subtitleFormat = "Up To x{0} Rewards";
        [SerializeField] private Color bronzeTextColor = new Color(0.96f, 0.68f, 0.32f, 1f);
        [SerializeField] private Color silverTextColor = new Color(0.86f, 0.94f, 1.00f, 1f);
        [SerializeField] private Color goldTextColor = new Color(1.00f, 0.82f, 0.08f, 1f);

        [Header("Slices")]
        [SerializeField] private List<WheelSliceView> ui_slice_views = new List<WheelSliceView>();

        [Header("Slice Layout Configuration")]
        [Tooltip("Distance from wheel center to slice items")]
        [SerializeField, Range(80f, 260f)] private float sliceRadialDistance = 160f;
        [Tooltip("Size of the slice item container")]
        [SerializeField] private Vector2 sliceContentSize = new Vector2(90f, 90f);
        [Tooltip("Standard icon dimensions")]
        [SerializeField] private Vector2 iconSize = new Vector2(50f, 50f);
        [Tooltip("Standard icon position offset inside the slice content")]
        [SerializeField] private Vector2 iconOffset = new Vector2(0f, 12f);
        [Tooltip("Bomb icon dimensions (larger to fill empty text space)")]
        [SerializeField] private Vector2 bombIconSize = new Vector2(68f, 68f);
        [Tooltip("Bomb icon position offset inside the slice content")]
        [SerializeField] private Vector2 bombIconOffset = Vector2.zero;
        [Tooltip("Amount text dimensions")]
        [SerializeField] private Vector2 textSize = new Vector2(90f, 26f);
        [Tooltip("Amount text position offset inside the slice content")]
        [SerializeField] private Vector2 textOffset = new Vector2(0f, -22f);
        [Tooltip("Font size for the amount text")]
        [SerializeField, Range(10f, 36f)] private float textFontSize = 18f;
        [Tooltip("Win shine effect dimensions")]
        [SerializeField] private Vector2 shineSize = new Vector2(110f, 110f);

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

        private Tween _spinTween;
        private Tween _indicatorTween;
        private int _lastTickSliceIndex = -1;
        private int _currentTargetIndex = -1;

        private void Awake()
        {
            if (ui_button_spin != null)
                ui_button_spin.onClick.AddListener(GameEvents.FireSpinRequested);

            ResetIndicator();
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
            KillTweens();
        }

        private void OnGameStateChanged(GameState state)
        {
            if (ui_button_spin != null)
                ui_button_spin.interactable = state == GameState.Idle;
        }

        private void OnZoneChanged(ZoneData zone)
        {
            var config = zone?.ConcreteWheelConfig;
            if (config == null) return;

            if (ui_image_wheel_base != null && config.WheelBaseSprite != null)
                ui_image_wheel_base.sprite = config.WheelBaseSprite;

            if (ui_image_wheel_indicator != null && config.IndicatorSprite != null)
                ui_image_wheel_indicator.sprite = config.IndicatorSprite;

            UpdateWheelLabels(config.WheelType, zone.RewardMultiplier);

            for (int i = 0; i < ui_slice_views.Count && i < config.SliceCount; i++)
                ui_slice_views[i]?.Setup(config.GetSlice(i), zone.RewardMultiplier);
        }

        public void UpdateWheelLabels(WheelType wheelType, float rewardMultiplier)
        {
            var (title, themeColor) = wheelType switch
            {
                WheelType.SilverSafe => (silverTitle, silverTextColor),
                WheelType.GoldenSuper => (goldTitle, goldTextColor),
                _ => (bronzeTitle, bronzeTextColor)
            };

            if (ui_text_wheel_title_value != null)
            {
                ui_text_wheel_title_value.text = title;
                ui_text_wheel_title_value.color = themeColor;
            }

            if (ui_text_wheel_subtitle_value != null)
            {
                int intMult = Mathf.Max(1, Mathf.RoundToInt(rewardMultiplier));
                ui_text_wheel_subtitle_value.text = string.Format(subtitleFormat, intMult.ToString(CultureInfo.InvariantCulture));
                ui_text_wheel_subtitle_value.color = themeColor;
            }
        }

        private void OnSpinStarted(int targetIndex, float targetAngle, float exactCenterAngle, float duration)
        {
            _currentTargetIndex = targetIndex;
            float initialZ = ui_transform_wheel_rotator != null ? ui_transform_wheel_rotator.localEulerAngles.z : 0f;
            _lastTickSliceIndex = WheelMathUtility.GetSliceIndexAtAngle(initialZ);

            ResetWheel();

            _spinTween = ui_transform_wheel_rotator
                .DOLocalRotate(new Vector3(0f, 0f, targetAngle), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic)
                .OnUpdate(CheckSliceTick)
                .OnComplete(() => DOVirtual.DelayedCall(centerDelay, () => CenterOnTargetSlice(targetAngle, exactCenterAngle)));
        }

        private void CheckSliceTick()
        {
            int currentSlice = WheelMathUtility.GetSliceIndexAtAngle(ui_transform_wheel_rotator.localEulerAngles.z);
            if (currentSlice == _lastTickSliceIndex) return;

            _lastTickSliceIndex = currentSlice;
            TriggerIndicatorTick();
            GameEvents.FireSliceTick();
        }

        private void CenterOnTargetSlice(float targetAngle, float exactCenterAngle)
        {
            float correctionDelta = exactCenterAngle - targetAngle;
            if (Mathf.Abs(correctionDelta) <= 0.1f)
            {
                CompleteSpin();
                return;
            }

            Vector3 targetEuler = ui_transform_wheel_rotator.localEulerAngles;
            targetEuler.z += correctionDelta;

            _spinTween = ui_transform_wheel_rotator
                .DOLocalRotate(targetEuler, centerDuration, RotateMode.Fast)
                .SetEase(centerEase)
                .OnComplete(CompleteSpin);
        }

        private void CompleteSpin()
        {
            ResetIndicator();

            if (_currentTargetIndex >= 0 && _currentTargetIndex < ui_slice_views.Count)
                ui_slice_views[_currentTargetIndex]?.PlayWinAnimation();

            wheelController?.OnSpinAnimationComplete();
        }

        public void ResetWheel()
        {
            KillTweens();
            ResetIndicator();
        }

        public Vector3 GetWinningSlicePosition()
        {
            if (_currentTargetIndex >= 0 && _currentTargetIndex < ui_slice_views.Count && ui_slice_views[_currentTargetIndex] != null)
                return ui_slice_views[_currentTargetIndex].IconWorldPosition;

            return ui_transform_wheel_rotator != null ? ui_transform_wheel_rotator.position : transform.position;
        }

        public void PlayBombImpactShake()
        {
            if (ui_transform_wheel_rotator == null) return;

            ui_transform_wheel_rotator.DOComplete();
            ui_transform_wheel_rotator.DOShakeAnchorPos(0.35f, new Vector2(15f, 15f), 28, 90f, false, true);
        }

        private void TriggerIndicatorTick()
        {
            if (ui_transform_indicator == null) return;

            _indicatorTween?.Kill();
            ui_transform_indicator.localEulerAngles = new Vector3(0f, 0f, indicatorKickAngle);
            _indicatorTween = ui_transform_indicator
                .DOLocalRotate(Vector3.zero, indicatorSnapDuration)
                .SetEase(Ease.OutBack, 2.5f);
        }

        private void KillTweens()
        {
            _spinTween?.Kill();
            _indicatorTween?.Kill();
        }

        private void ResetIndicator()
        {
            _indicatorTween?.Kill();
            if (ui_transform_indicator == null) return;

            ui_transform_indicator.pivot = IndicatorHingePivot;
            ui_transform_indicator.localEulerAngles = Vector3.zero;
        }

        private void OnValidate()
        {
            if (ui_button_spin == null) ui_button_spin = FindChildComponent<Button>("ui_button_spin");
            if (ui_text_wheel_title_value == null) ui_text_wheel_title_value = FindChildComponent<TMP_Text>("ui_text_wheel_title_value");
            if (ui_text_wheel_subtitle_value == null) ui_text_wheel_subtitle_value = FindChildComponent<TMP_Text>("ui_text_wheel_subtitle_value");
            if (wheelController == null) wheelController = FindFirstObjectByType<WheelController>();

            EnsureSliceViews();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this != null) ApplySliceLayout();
                };
            }
#endif
        }

        private T FindChildComponent<T>(string childName) where T : Component =>
            transform.Find(childName)?.GetComponent<T>();

        private void EnsureSliceViews()
        {
            if (ui_slice_views == null || ui_slice_views.Count == 0)
                ui_slice_views = new List<WheelSliceView>(GetComponentsInChildren<WheelSliceView>(true));
        }

#if UNITY_EDITOR
        [ContextMenu("Apply Slice Layout")]
        public void ApplySliceLayout()
        {
            EnsureSliceViews();

            int count = ui_slice_views.Count;
            if (count == 0) return;

            for (int i = 0; i < count; i++)
            {
                var slice = ui_slice_views[i];
                if (slice == null) continue;

                UnityEditor.Undo.RecordObject(slice.gameObject, "Apply Slice Layout");

                RectTransform sliceRT = (RectTransform)slice.transform;
                sliceRT.localRotation = Quaternion.Euler(0f, 0f, -i * (360f / count));
                sliceRT.anchoredPosition = Vector2.zero;

                slice.ApplyLayout(
                    sliceRadialDistance,
                    sliceContentSize,
                    iconSize,
                    iconOffset,
                    bombIconSize,
                    bombIconOffset,
                    textSize,
                    textOffset,
                    textFontSize,
                    shineSize);
            }
        }
#endif
    }
}
