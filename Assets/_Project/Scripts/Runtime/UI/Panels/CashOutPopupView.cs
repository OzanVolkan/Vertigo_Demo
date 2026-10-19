using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Vertigo.Core.Enums;
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
            if (ui_panel_popup_root == null) ui_panel_popup_root = gameObject;

            BindButton(ui_button_collect_rewards, OnCollectRewardsClicked);
            BindButton(ui_button_go_back, OnGoBackClicked);

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

        private void OnDestroy() => _animSequence?.Kill();

        public void ShowPopup()
        {
            _animSequence?.Kill();

            if (ui_panel_popup_root != null && !ui_panel_popup_root.activeSelf)
            {
                ui_panel_popup_root.SetActive(true);
            }

            SetCanvasGroupState(0f, true);
            if (ui_transform_dialog != null) ui_transform_dialog.localScale = Vector3.one * 0.85f;

            _animSequence = DOTween.Sequence();
            if (ui_canvas_group != null)
                _animSequence.Append(ui_canvas_group.DOFade(1f, 0.22f).SetEase(Ease.OutQuad));

            if (ui_transform_dialog != null)
                _animSequence.Join(ui_transform_dialog.DOScale(1f, 0.28f).SetEase(Ease.OutBack));
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
            SetCanvasGroupState(0f, false);
        }

        private void SetCanvasGroupState(float alpha, bool interactable)
        {
            if (ui_canvas_group == null) return;
            ui_canvas_group.alpha = alpha;
            ui_canvas_group.blocksRaycasts = interactable;
            ui_canvas_group.interactable = interactable;
        }

        private static void BindButton(Button button, UnityAction action)
        {
            if (button == null) return;
            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        private void OnCollectRewardsClicked()
        {
            HidePopup();
            GameEvents.FireCashOutRequested();
        }

        private void OnGoBackClicked()
        {
            GameEvents.FireSfx(SoundEffectType.ButtonClick);
            HidePopup();
        }

        private void OnValidate()
        {
            if (ui_panel_popup_root == null) ui_panel_popup_root = gameObject;
            if (ui_canvas_group == null) ui_canvas_group = GetComponent<CanvasGroup>();

            if (ui_transform_dialog == null)
            {
                Transform tr = transform.Find("ui_transform_dialog");
                if (tr != null) ui_transform_dialog = tr.GetComponent<RectTransform>();
            }

            if (ui_transform_dialog == null) return;

            if (ui_button_collect_rewards == null)
            {
                Transform tr = ui_transform_dialog.Find("ui_button_collect_rewards");
                if (tr != null) ui_button_collect_rewards = tr.GetComponent<Button>();
            }

            if (ui_button_go_back == null)
            {
                Transform tr = ui_transform_dialog.Find("ui_button_go_back");
                if (tr != null) ui_button_go_back = tr.GetComponent<Button>();
            }
        }
    }
}
