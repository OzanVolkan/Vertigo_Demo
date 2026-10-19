using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Rewards.Views
{
    public class InventoryRewardSlotView : MonoBehaviour
    {
        [SerializeField] private Image ui_image_icon;
        [SerializeField] private TMP_Text ui_text_amount_value;

        private int _displayedAmount;
        private Tween _countTween;

        public Vector3 IconWorldPosition => ui_image_icon != null ? ui_image_icon.transform.position : transform.position;

        private void Awake() => EnforceMaskable();

        private void OnDestroy() => _countTween?.Kill();

        private void EnforceMaskable()
        {
            if (ui_image_icon != null) ui_image_icon.maskable = true;
            if (ui_text_amount_value != null) ui_text_amount_value.maskable = true;
        }

        public void Setup(Sprite icon, int amount, string formattedText = null)
        {
            _countTween?.Kill();
            EnforceMaskable();

            if (ui_image_icon != null)
            {
                ui_image_icon.sprite = icon;
                ui_image_icon.enabled = icon != null;
            }

            SetDisplayedAmount(amount, formattedText);
        }

        public void PlayPop()
        {
            Transform target = ui_image_icon != null ? ui_image_icon.transform : transform;
            target.DOKill(true);
            target.localScale = Vector3.one;
            target.DOPunchScale(Vector3.one * 0.32f, 0.32f, 2, 0.5f);
        }

        public void AnimateCountUp(int fromAmount, int toAmount, float duration)
        {
            _countTween?.Kill();
            SetDisplayedAmount(fromAmount);

            _countTween = DOTween.To(
                () => _displayedAmount,
                x => SetDisplayedAmount(x),
                toAmount,
                Mathf.Max(0.2f, duration)
            ).SetEase(Ease.OutQuad);
        }

        private void SetDisplayedAmount(int amount, string customText = null)
        {
            _displayedAmount = amount;
            if (ui_text_amount_value != null)
            {
                ui_text_amount_value.text = string.IsNullOrEmpty(customText)
                    ? amount.ToString("N0")
                    : customText;
            }
        }
    }
}
