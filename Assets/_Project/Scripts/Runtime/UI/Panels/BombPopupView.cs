using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Events;

namespace Vertigo.UI.Panels
{
    public class BombPopupView : MonoBehaviour
    {
        [Header("Popup Containers")]
        [SerializeField] private GameObject ui_panel_popup_root;
        [SerializeField] private RectTransform ui_transform_box;

        [Header("Buttons")]
        [SerializeField] private Button ui_button_giveup;
        [SerializeField] private Button ui_button_revive;

        private void Awake()
        {
            if (ui_button_giveup != null)
                ui_button_giveup.onClick.AddListener(OnGiveUpClicked);

            if (ui_button_revive != null)
                ui_button_revive.onClick.AddListener(OnReviveClicked);

            HidePopup();
        }

        private void OnEnable()
        {
            GameEvents.OnBombTriggered += ShowPopup;
            GameEvents.OnReviveSuccess += HidePopup;
            GameEvents.OnGameRestartRequested += HidePopup;
        }

        private void OnDisable()
        {
            GameEvents.OnBombTriggered -= ShowPopup;
            GameEvents.OnReviveSuccess -= HidePopup;
            GameEvents.OnGameRestartRequested -= HidePopup;
        }

        private void ShowPopup()
        {
            if (ui_panel_popup_root != null)
                ui_panel_popup_root.SetActive(true);

            if (ui_transform_box != null)
            {
                ui_transform_box.localScale = Vector3.one * 0.7f;
                ui_transform_box.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
            }
        }

        private void HidePopup()
        {
            if (ui_panel_popup_root != null)
                ui_panel_popup_root.SetActive(false);
        }

        private void OnGiveUpClicked()
        {
            HidePopup();
            GameEvents.FireGameRestartRequested();
        }

        private void OnReviveClicked()
        {
            HidePopup();
            GameEvents.FireReviveRequested();
        }

        private void OnValidate()
        {
            if (ui_button_giveup == null)
            {
                var tr = transform.Find("ui_button_giveup");
                if (tr != null) ui_button_giveup = tr.GetComponent<Button>();
            }
            if (ui_button_revive == null)
            {
                var tr = transform.Find("ui_button_revive");
                if (tr != null) ui_button_revive = tr.GetComponent<Button>();
            }
        }
    }
}
