using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Interfaces;

namespace Vertigo.UI.Panels
{
    /// <summary>
    /// Displays a single reward card in the Cash Out final summary grid.
    /// Can be instantiated from distinct Silver or Gold prefabs for full manual design control.
    /// </summary>
    public class CashOutSummaryCardView : MonoBehaviour
    {
        private static readonly Color ValuableTextColor = new Color(1f, 0.88f, 0.35f, 1f);

        [Header("Card UI Elements")]
        [SerializeField] private TMP_Text ui_text_name;
        [SerializeField] private Image ui_image_icon;
        [SerializeField] private TMP_Text ui_text_amount;
        [SerializeField] private Image ui_image_bg;

        [Header("Card Style Configuration")]
        [Tooltip("If true, overrides text colors based on reward IsValuable. If false, preserves the exact colors set on this prefab.")]
        [SerializeField] private bool overrideTextColorFromReward = false;
        [SerializeField] private Sprite silverCardSprite;
        [SerializeField] private Sprite goldCardSprite;

        private void Awake() => EnsureMaskable();

        private void EnsureMaskable()
        {
            if (ui_text_name != null) ui_text_name.maskable = true;
            if (ui_text_amount != null) ui_text_amount.maskable = true;
            if (ui_image_icon != null) ui_image_icon.maskable = true;
            if (ui_image_bg != null) ui_image_bg.maskable = true;
        }

        public void Setup(IRewardData reward, int amount) =>
            Setup(reward, amount, silverCardSprite, goldCardSprite);

        public void Setup(IRewardData reward, int amount, Sprite defaultSilver, Sprite defaultGold)
        {
            EnsureMaskable();
            bool isValuable = reward != null && reward.IsValuable;

            if (ui_image_bg != null)
            {
                if (ui_image_bg.sprite == null)
                {
                    Sprite sSprite = silverCardSprite != null ? silverCardSprite : defaultSilver;
                    Sprite gSprite = goldCardSprite != null ? goldCardSprite : defaultGold;
                    ui_image_bg.sprite = isValuable ? gSprite : sSprite;
                }
                ui_image_bg.color = Color.white;
                ui_image_bg.preserveAspect = true;
            }

            if (ui_image_icon != null)
            {
                ui_image_icon.sprite = reward?.Icon;
                ui_image_icon.enabled = reward?.Icon != null;
            }

            ApplyText(ui_text_name, reward != null ? reward.DisplayName.ToUpper() : string.Empty, isValuable);
            ApplyText(ui_text_amount, reward != null ? reward.FormatAmount(amount) : $"x{amount:N0}", isValuable);
        }

        private void ApplyText(TMP_Text label, string text, bool isValuable)
        {
            if (label == null) return;
            label.text = text;
            if (overrideTextColorFromReward)
            {
                label.color = isValuable ? ValuableTextColor : Color.white;
            }
        }
    }
}
