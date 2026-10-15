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

        private void Awake()
        {
            EnforceMaskable();
        }

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

            _displayedAmount = amount;
            if (ui_text_amount_value != null)
            {
                ui_text_amount_value.text = string.IsNullOrEmpty(formattedText) 
                    ? amount.ToString("N0") 
                    : formattedText;
            }
        }

        public void SetAmount(int amount)
        {
            _countTween?.Kill();
            _displayedAmount = amount;
            if (ui_text_amount_value != null)
            {
                ui_text_amount_value.text = amount.ToString("N0");
            }
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
            _displayedAmount = fromAmount;
            if (ui_text_amount_value != null)
            {
                ui_text_amount_value.text = fromAmount.ToString("N0");
            }

            _countTween = DOTween.To(() => _displayedAmount, x =>
            {
                _displayedAmount = x;
                if (ui_text_amount_value != null)
                {
                    ui_text_amount_value.text = $"{_displayedAmount:N0}";
                }
            }, toAmount, Mathf.Max(0.2f, duration)).SetEase(Ease.OutQuad);
        }

        private void OnDestroy()
        {
            _countTween?.Kill();
        }
    }
}
