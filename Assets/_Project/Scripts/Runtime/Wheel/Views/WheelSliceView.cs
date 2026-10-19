using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Interfaces;

namespace Vertigo.Wheel.Views
{
    public class WheelSliceView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image ui_image_icon;
        [SerializeField] private TMP_Text ui_text_amount_value;
        [SerializeField] private Image ui_image_shine;

        [Header("Layout Settings")]
        [SerializeField] private Vector2 standardIconSize = new Vector2(50f, 50f);
        [SerializeField] private Vector2 standardIconOffset = new Vector2(0f, 12f);
        [SerializeField] private Vector2 bombIconSize = new Vector2(68f, 68f);
        [SerializeField] private Vector2 bombIconOffset = Vector2.zero;

        private Tween _winTween;

        public Vector3 IconWorldPosition => ui_image_icon != null ? ui_image_icon.transform.position : transform.position;

        public void Setup(IWheelSliceData sliceData, float rewardMultiplier = 1f)
        {
            _winTween?.Kill();
            transform.localScale = Vector3.one;

            if (ui_image_shine != null)
                ui_image_shine.gameObject.SetActive(false);

            var reward = sliceData?.Reward;
            if (reward == null)
            {
                if (ui_image_icon != null) ui_image_icon.enabled = false;
                if (ui_text_amount_value != null) ui_text_amount_value.text = string.Empty;
                return;
            }

            if (ui_image_icon != null)
            {
                ui_image_icon.sprite = reward.Icon;
                ui_image_icon.enabled = reward.Icon != null;
                ApplyIconTransform(ui_image_icon.rectTransform, reward.IsBomb);
            }

            if (ui_text_amount_value != null)
            {
                int scaledAmount = Mathf.Max(1, Mathf.RoundToInt(sliceData.Amount * rewardMultiplier));
                ui_text_amount_value.text = reward.IsBomb ? string.Empty : reward.FormatAmount(scaledAmount);
            }
        }

        public void PlayWinAnimation()
        {
            _winTween?.Kill();

            if (ui_image_shine != null)
            {
                ui_image_shine.gameObject.SetActive(true);
                ui_image_shine.color = new Color(1f, 1f, 1f, 0f);
                ui_image_shine.DOFade(0.8f, 0.2f).SetLoops(2, LoopType.Yoyo);
            }

            _winTween = transform
                .DOPunchScale(Vector3.one * 0.25f, 0.5f, 3, 0.6f)
                .OnComplete(() =>
                {
                    if (ui_image_shine != null)
                        ui_image_shine.gameObject.SetActive(false);
                });
        }

        private void ApplyIconTransform(RectTransform iconRT, bool isBomb)
        {
            iconRT.sizeDelta = isBomb ? bombIconSize : standardIconSize;
            iconRT.anchoredPosition = isBomb ? bombIconOffset : standardIconOffset;
        }

        private void OnDestroy() => _winTween?.Kill();

#if UNITY_EDITOR
        public void ApplyLayout(
            float radialDistance,
            Vector2 contentSize,
            Vector2 iconSize,
            Vector2 iconOffset,
            Vector2 bombSize,
            Vector2 bombOffset,
            Vector2 textSize,
            Vector2 textOffset,
            float fontSize,
            Vector2 shineSize)
        {
            standardIconSize = iconSize;
            standardIconOffset = iconOffset;
            bombIconSize = bombSize;
            bombIconOffset = bombOffset;

            var contentTrans = transform.Find("ui_slice_content");
            if (contentTrans != null && contentTrans.TryGetComponent(out RectTransform contentRT))
                SetRectLayout(contentRT, contentSize, new Vector2(0f, radialDistance));

            if (ui_image_icon != null && ui_image_icon.TryGetComponent(out RectTransform iconRT))
            {
                UnityEditor.Undo.RecordObject(iconRT, "Adjust Slice Icon Layout");
                bool isBomb = ui_image_icon.sprite != null && ui_image_icon.sprite.name.ToLowerInvariant().Contains("bomb");
                ApplyIconTransform(iconRT, isBomb);
                UnityEditor.EditorUtility.SetDirty(iconRT);
            }

            if (ui_text_amount_value != null && ui_text_amount_value.TryGetComponent(out RectTransform textRT))
            {
                SetRectLayout(textRT, textSize, textOffset);
                UnityEditor.Undo.RecordObject(ui_text_amount_value, "Adjust Slice Text Font");
                ui_text_amount_value.fontSize = fontSize;
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

        private static void SetRectLayout(RectTransform rt, Vector2 size, Vector2 position)
        {
            UnityEditor.Undo.RecordObject(rt, "Adjust Slice Layout");
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
            UnityEditor.EditorUtility.SetDirty(rt);
        }
#endif
    }
}
