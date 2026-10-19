using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Enums;
using Vertigo.Core.Events;

namespace Vertigo.Rewards.Views
{
    public class RewardShowcaseView : MonoBehaviour
    {
        [Header("Dimmer & Root")]
        [SerializeField] private GameObject ui_root;
        [SerializeField] private CanvasGroup ui_dimmer_group;

        [Header("Sunburst & Glow")]
        [SerializeField] private RectTransform ui_transform_sunburst;
        [SerializeField] private Image ui_image_sunburst;
        [SerializeField] private Image ui_image_glow;

        [Header("Card & Contents")]
        [SerializeField] private RectTransform ui_transform_card_box;
        [SerializeField] private Image ui_image_card_bg;
        [SerializeField] private Image ui_image_reward_icon;
        [SerializeField] private TMP_Text ui_text_amount_value;

        [Header("Animation Settings")]
        [SerializeField, Range(0.2f, 0.6f)] private float popInDuration = 0.32f;
        [SerializeField, Range(0.4f, 2.0f)] private float displayDuration = 0.85f;
        [SerializeField, Range(0.15f, 0.5f)] private float popOutDuration = 0.22f;
        [SerializeField] private float rayRotationSpeed = 25f;

        private Sequence _showcaseSequence;
        private Tween _rayRotationTween;

        private void Awake()
        {
            if (ui_root == null) ui_root = gameObject;
            if (ui_image_card_bg != null) ui_image_card_bg.enabled = false;
            ResetVisualState();
        }

        private void OnDisable() => KillTweens();

        public void ShowReward(Sprite icon, string amountText, WheelType wheelType, Action onComplete)
        {
            KillTweens();
            GameEvents.FireSfx(SoundEffectType.RewardShowcasePop);

            if (ui_root != null && !ui_root.activeSelf) ui_root.SetActive(true);
            if (ui_image_card_bg != null) ui_image_card_bg.enabled = false;

            if (ui_image_reward_icon != null)
            {
                ui_image_reward_icon.sprite = icon;
                ui_image_reward_icon.enabled = icon != null;
            }

            if (ui_text_amount_value != null) ui_text_amount_value.text = amountText;
            if (ui_image_sunburst != null) ui_image_sunburst.color = GetRayColor(wheelType);
            if (ui_image_glow != null) ui_image_glow.color = GetGlowColor(wheelType);

            ResetVisualState(blocksRaycasts: true);

            if (ui_transform_sunburst != null)
            {
                ui_transform_sunburst.localEulerAngles = Vector3.zero;
                _rayRotationTween = ui_transform_sunburst
                    .DORotate(new Vector3(0f, 0f, 360f), 360f / Mathf.Max(5f, rayRotationSpeed), RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1);
            }

            _showcaseSequence = DOTween.Sequence();

            if (ui_dimmer_group != null)
            {
                _showcaseSequence.Append(ui_dimmer_group.DOFade(1f, 0.2f).SetEase(Ease.OutQuad));
            }

            if (ui_transform_card_box != null)
            {
                Tween scaleIn = ui_transform_card_box.DOScale(Vector3.one, popInDuration).SetEase(Ease.OutBack, 1.35f);
                if (ui_dimmer_group != null) _showcaseSequence.Join(scaleIn);
                else _showcaseSequence.Append(scaleIn);
            }

            _showcaseSequence.AppendInterval(displayDuration);

            if (ui_transform_card_box != null)
            {
                _showcaseSequence.Append(ui_transform_card_box.DOScale(Vector3.zero, popOutDuration).SetEase(Ease.InBack, 1.2f));
            }

            if (ui_dimmer_group != null)
            {
                _showcaseSequence.Join(ui_dimmer_group.DOFade(0f, popOutDuration).SetEase(Ease.InQuad));
            }

            _showcaseSequence.OnComplete(() =>
            {
                HideImmediate();
                onComplete?.Invoke();
            });
        }

        public void HideImmediate()
        {
            KillTweens();
            ResetVisualState();
            if (ui_root != null) ui_root.SetActive(false);
        }

        private void ResetVisualState(bool blocksRaycasts = false)
        {
            if (ui_dimmer_group != null)
            {
                ui_dimmer_group.alpha = 0f;
                ui_dimmer_group.blocksRaycasts = blocksRaycasts;
            }
            if (ui_transform_card_box != null)
            {
                ui_transform_card_box.localScale = Vector3.zero;
            }
        }

        private void KillTweens()
        {
            _showcaseSequence?.Kill();
            _rayRotationTween?.Kill();
        }

        private static Color GetRayColor(WheelType wheelType) => wheelType switch
        {
            WheelType.SilverSafe => new Color(0.85f, 0.95f, 1f, 0.38f),
            WheelType.GoldenSuper => new Color(1f, 0.88f, 0.25f, 0.45f),
            _ => new Color(1f, 0.78f, 0.35f, 0.32f)
        };

        private static Color GetGlowColor(WheelType wheelType) => wheelType switch
        {
            WheelType.SilverSafe => new Color(0.5f, 0.85f, 1f, 0.65f),
            WheelType.GoldenSuper => new Color(1f, 0.82f, 0.15f, 0.8f),
            _ => new Color(1f, 0.65f, 0.2f, 0.65f)
        };
    }
}
