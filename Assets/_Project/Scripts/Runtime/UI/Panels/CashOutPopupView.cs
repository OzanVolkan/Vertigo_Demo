using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Events;

namespace Vertigo.UI.Panels
{
    public class CashOutPopupView : MonoBehaviour
    {
        [Header("Popup Containers")]
        [SerializeField] private GameObject ui_panel_popup_root;
        [SerializeField] private CanvasGroup ui_canvas_group;
        [SerializeField] private RectTransform ui_transform_dialog;

        [Header("Texts & Buttons")]
        [SerializeField] private TMP_Text ui_text_prompt;
        [SerializeField] private Button ui_button_collect_rewards;
        [SerializeField] private Button ui_button_go_back;

        private Sequence _animSequence;

        private void Awake()
        {
            if (ui_panel_popup_root == null)
                ui_panel_popup_root = gameObject;

            if (ui_canvas_group == null)
                ui_canvas_group = ui_panel_popup_root.GetComponent<CanvasGroup>();

            if (ui_button_collect_rewards != null)
            {
                ui_button_collect_rewards.onClick.RemoveListener(OnCollectRewardsClicked);
                ui_button_collect_rewards.onClick.AddListener(OnCollectRewardsClicked);
            }

            if (ui_button_go_back != null)
            {
                ui_button_go_back.onClick.RemoveListener(OnGoBackClicked);
                ui_button_go_back.onClick.AddListener(OnGoBackClicked);
            }

            HideImmediate();
        }

        private void OnEnable()
        {
            GameEvents.OnCashOutPromptRequested += ShowPopup;
            GameEvents.OnCashOutRequested += HidePopup;
            GameEvents.OnGameRestartRequested += HidePopup;
        }

        private void OnDisable()
        {
            GameEvents.OnCashOutPromptRequested -= ShowPopup;
            GameEvents.OnCashOutRequested -= HidePopup;
            GameEvents.OnGameRestartRequested -= HidePopup;
            _animSequence?.Kill();
        }

        private void OnDestroy()
        {
            _animSequence?.Kill();
        }

        public void ShowPopup()
        {
            _animSequence?.Kill();

            if (ui_panel_popup_root != null && !ui_panel_popup_root.activeSelf)
            {
                ui_panel_popup_root.SetActive(true);
            }

            if (ui_canvas_group != null)
            {
                ui_canvas_group.alpha = 0f;
                ui_canvas_group.blocksRaycasts = true;
                ui_canvas_group.interactable = true;
            }

            if (ui_transform_dialog != null)
            {
                ui_transform_dialog.localScale = Vector3.one * 0.85f;
            }

            _animSequence = DOTween.Sequence();

            if (ui_canvas_group != null)
            {
                _animSequence.Append(ui_canvas_group.DOFade(1f, 0.22f).SetEase(Ease.OutQuad));
            }

            if (ui_transform_dialog != null)
            {
                _animSequence.Join(ui_transform_dialog.DOScale(1f, 0.28f).SetEase(Ease.OutBack));
            }
        }

        public void HidePopup()
        {
            _animSequence?.Kill();

            if (ui_canvas_group != null)
            {
                ui_canvas_group.blocksRaycasts = false;
                ui_canvas_group.interactable = false;
                ui_canvas_group.DOFade(0f, 0.18f).SetEase(Ease.InQuad);
            }
            else if (ui_panel_popup_root != null)
            {
                ui_panel_popup_root.SetActive(false);
            }
        }

        public void HideImmediate()
        {
            _animSequence?.Kill();

            if (ui_panel_popup_root != null && !ui_panel_popup_root.activeSelf)
            {
                ui_panel_popup_root.SetActive(true);
            }

            if (ui_canvas_group != null)
            {
                ui_canvas_group.alpha = 0f;
                ui_canvas_group.blocksRaycasts = false;
                ui_canvas_group.interactable = false;
            }
        }

        private void OnCollectRewardsClicked()
        {
            GameEvents.FireSfx(Vertigo.Core.Enums.SoundEffectType.ButtonClick);
            HidePopup();
            GameEvents.FireCashOutRequested();
        }

        private void OnGoBackClicked()
        {
            GameEvents.FireSfx(Vertigo.Core.Enums.SoundEffectType.ButtonClick);
            HidePopup();
        }

        private void OnValidate()
        {
            if (ui_panel_popup_root == null)
                ui_panel_popup_root = gameObject;

            if (ui_canvas_group == null)
                ui_canvas_group = GetComponent<CanvasGroup>();

            if (ui_transform_dialog == null)
            {
                var tr = transform.Find("ui_transform_dialog");
                if (tr != null) ui_transform_dialog = tr.GetComponent<RectTransform>();
            }

            if (ui_button_collect_rewards == null && ui_transform_dialog != null)
            {
                var tr = ui_transform_dialog.Find("ui_button_collect_rewards");
                if (tr != null) ui_button_collect_rewards = tr.GetComponent<Button>();
            }

            if (ui_button_go_back == null && ui_transform_dialog != null)
            {
                var tr = ui_transform_dialog.Find("ui_button_go_back");
                if (tr != null) ui_button_go_back = tr.GetComponent<Button>();
            }
        }
    }
}
