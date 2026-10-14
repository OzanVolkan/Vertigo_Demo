using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Rewards.Views
{
    public class InventoryRewardSlotView : MonoBehaviour
    {
        [SerializeField] private Image ui_image_icon;
        [SerializeField] private TMP_Text ui_text_amount_value;

        public void Setup(Sprite icon, int amount, string formattedText)
        {
            if (ui_image_icon != null)
            {
                ui_image_icon.sprite = icon;
                ui_image_icon.enabled = icon != null;
            }

            if (ui_text_amount_value != null)
            {
                ui_text_amount_value.text = string.IsNullOrEmpty(formattedText) 
                    ? amount.ToString("N0") 
                    : formattedText;
            }
        }
    }
}
