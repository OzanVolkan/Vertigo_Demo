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

        public Vector3 IconWorldPosition => ui_image_icon != null ? ui_image_icon.transform.position : transform.position;

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

#if UNITY_EDITOR
        public void ApplyLayout(
            float radialDistance,
            Vector2 contentSize,
            Vector2 iconSize,
            Vector2 iconOffset,
            Vector2 textSize,
            Vector2 textOffset,
            float fontSize,
            Vector2 shineSize)
        {
            Transform contentTrans = transform.Find("ui_slice_content");
            if (contentTrans != null && contentTrans.TryGetComponent(out RectTransform contentRT))
            {
                UnityEditor.Undo.RecordObject(contentRT, "Adjust Slice Content Layout");
                contentRT.anchoredPosition = new Vector2(0f, radialDistance);
                contentRT.sizeDelta = contentSize;
                UnityEditor.EditorUtility.SetDirty(contentRT);
            }

            if (ui_image_icon != null && ui_image_icon.TryGetComponent(out RectTransform iconRT))
            {
                UnityEditor.Undo.RecordObject(iconRT, "Adjust Slice Icon Layout");
                iconRT.sizeDelta = iconSize;
                iconRT.anchoredPosition = iconOffset;
                UnityEditor.EditorUtility.SetDirty(iconRT);
            }

            if (ui_text_amount_value != null && ui_text_amount_value.TryGetComponent(out RectTransform textRT))
            {
                UnityEditor.Undo.RecordObject(textRT, "Adjust Slice Text Layout");
                UnityEditor.Undo.RecordObject(ui_text_amount_value, "Adjust Slice Text Font");
                textRT.sizeDelta = textSize;
                textRT.anchoredPosition = textOffset;
                ui_text_amount_value.fontSize = fontSize;
                UnityEditor.EditorUtility.SetDirty(textRT);
                UnityEditor.EditorUtility.SetDirty(ui_text_amount_value);
            }

            if (ui_image_shine != null && ui_image_shine.TryGetComponent(out RectTransform shineRT))
            {
                UnityEditor.Undo.RecordObject(shineRT, "Adjust Slice Shine Layout");
                shineRT.sizeDelta = shineSize;
                UnityEditor.EditorUtility.SetDirty(shineRT);
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
