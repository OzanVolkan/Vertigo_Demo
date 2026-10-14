using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Interfaces;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Views
{
    public class WheelSliceView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image ui_image_icon;
        [SerializeField] private TMP_Text ui_text_amount_value;
        [SerializeField] private Image ui_image_shine;

        private Tween _winTween;

        public void Setup(IWheelSliceData sliceData)
        {
            _winTween?.Kill();
            transform.localScale = Vector3.one;

            if (ui_image_shine != null)
                ui_image_shine.gameObject.SetActive(false);

            if (sliceData?.Reward == null)
            {
                if (ui_image_icon != null) ui_image_icon.enabled = false;
                if (ui_text_amount_value != null) ui_text_amount_value.text = string.Empty;
                return;
            }

            if (ui_image_icon != null)
            {
                ui_image_icon.sprite = sliceData.Reward.Icon;
                ui_image_icon.enabled = sliceData.Reward.Icon != null;
            }

            if (ui_text_amount_value != null)
            {
                if (sliceData.Reward.IsBomb)
                {
                    ui_text_amount_value.text = string.Empty;
                }
                else
                {
                    ui_text_amount_value.text = sliceData.Reward.FormatAmount(sliceData.Amount);
                }
            }
        }

        public void PlayWinAnimation()
        {
            _winTween?.Kill();

            if (ui_image_shine != null)
            {
                ui_image_shine.gameObject.SetActive(true);
                ui_image_shine.color = new Color(1, 1, 1, 0);
                ui_image_shine.DOFade(0.8f, 0.2f).SetLoops(2, LoopType.Yoyo);
            }

            _winTween = transform.DOPunchScale(Vector3.one * 0.25f, 0.5f, 3, 0.6f)
                .OnComplete(() =>
                {
                    if (ui_image_shine != null)
                        ui_image_shine.gameObject.SetActive(false);
                });
        }

        private void OnDestroy()
        {
            _winTween?.Kill();
        }
    }
}
