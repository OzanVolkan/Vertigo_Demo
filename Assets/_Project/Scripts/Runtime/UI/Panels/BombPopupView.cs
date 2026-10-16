using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Events;

namespace Vertigo.UI.Panels
{
    public class BombPopupView : MonoBehaviour
    {
        [Header("Popup Containers")]
        [SerializeField] private GameObject ui_panel_popup_root;
        [SerializeField] private CanvasGroup ui_canvas_group;
        [SerializeField] private RectTransform ui_transform_header;
        [SerializeField] private RectTransform ui_transform_box;
        [SerializeField] private RectTransform ui_transform_buttons;

        [Header("Visual Elements")]
        [SerializeField] private Image ui_image_red_glow;
        [SerializeField] private RectTransform ui_transform_danger_rays;
        [SerializeField] private Image ui_image_circle_bg;
        [SerializeField] private RectTransform ui_transform_bomb_icon;
        [SerializeField] private TMP_Text ui_text_revive_cost_value;

        [Header("Buttons")]
        [SerializeField] private Button ui_button_giveup;
        [SerializeField] private Button ui_button_revive;
        [SerializeField] private Button ui_button_revive_ad;

        [Header("Settings")]
        [SerializeField] private int reviveCost = 25;

        private Sequence _showSequence;
        private Tween _glowPulseTween;
        private Tween _raysRotateTween;
        private Tween _iconIdleTween;

        private void Awake()
        {
            if (ui_panel_popup_root == null)
                ui_panel_popup_root = gameObject;

            if (ui_canvas_group == null)
                ui_canvas_group = ui_panel_popup_root.GetComponent<CanvasGroup>();

            if (ui_button_giveup != null)
            {
                ui_button_giveup.onClick.RemoveListener(OnGiveUpClicked);
                ui_button_giveup.onClick.AddListener(OnGiveUpClicked);
            }

            if (ui_button_revive != null)
            {
                ui_button_revive.onClick.RemoveListener(OnReviveClicked);
                ui_button_revive.onClick.AddListener(OnReviveClicked);
            }

            if (ui_button_revive_ad != null)
            {
                ui_button_revive_ad.onClick.RemoveListener(OnReviveClicked);
                ui_button_revive_ad.onClick.AddListener(OnReviveClicked);
            }

            if (ui_text_revive_cost_value != null)
            {
                ui_text_revive_cost_value.text = reviveCost.ToString();
            }

            HideImmediate();
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
            KillTweens();
        }

        private void OnDestroy()
        {
            KillTweens();
        }

        private void KillTweens()
        {
            _showSequence?.Kill();
            _glowPulseTween?.Kill();
            _raysRotateTween?.Kill();
            _iconIdleTween?.Kill();
        }

        public void ShowPopup()
        {
            KillTweens();

            if (ui_panel_popup_root != null && !ui_panel_popup_root.activeSelf)
            {
                ui_panel_popup_root.SetActive(true);
            }

            if (ui_text_revive_cost_value != null)
            {
                ui_text_revive_cost_value.text = reviveCost.ToString();
            }

            if (ui_canvas_group != null)
            {
                ui_canvas_group.alpha = 0f;
                ui_canvas_group.blocksRaycasts = true;
                ui_canvas_group.interactable = true;
            }

            if (ui_transform_header != null)
            {
                ui_transform_header.localScale = Vector3.one * 0.75f;
            }

            if (ui_transform_box != null)
            {
                ui_transform_box.localScale = Vector3.one * 0.35f;
                ui_transform_box.localEulerAngles = Vector3.zero;
            }

            if (ui_transform_buttons != null)
            {
                ui_transform_buttons.localScale = Vector3.zero;
            }

            // Start slow danger ray rotation immediately
            if (ui_transform_danger_rays != null)
            {
                ui_transform_danger_rays.localEulerAngles = Vector3.zero;
                _raysRotateTween = ui_transform_danger_rays
                    .DOLocalRotate(new Vector3(0f, 0f, -360f), 16f, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart);
            }

            _showSequence = DOTween.Sequence();

            // 1. Fade in dark red overlay
            if (ui_canvas_group != null)
            {
                _showSequence.Append(ui_canvas_group.DOFade(1f, 0.25f).SetEase(Ease.OutQuad));
            }

            // 2. Pop in Header ("OH NO, A BOMB EXPLODED RIGHT IN YOUR HANDS!")
            if (ui_transform_header != null)
            {
                _showSequence.Join(ui_transform_header.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
            }

            // 3. Dramatic Circular Medallion Pop + Bomb Icon Impact Shake
            if (ui_transform_box != null)
            {
                _showSequence.Join(ui_transform_box.DOScale(1f, 0.42f).SetEase(Ease.OutBack, 1.45f));
            }

            if (ui_transform_bomb_icon != null)
            {
                _showSequence.Join(ui_transform_bomb_icon.DOShakeAnchorPos(0.45f, new Vector2(15f, 12f), 24, 90f, false, true).SetDelay(0.10f));
            }

            // 4. Pop in Give Up & Revive Buttons
            if (ui_transform_buttons != null)
            {
                _showSequence.Insert(0.18f, ui_transform_buttons.DOScale(1f, 0.32f).SetEase(Ease.OutBack));
            }

            // 5. Continuous menacing red glow pulse & subtle icon heartbeat
            _showSequence.OnComplete(StartIdleLoops);
        }

        private void StartIdleLoops()
        {
            if (ui_image_red_glow != null)
            {
                Color c = ui_image_red_glow.color;
                c.a = 0.55f;
                ui_image_red_glow.color = c;
                _glowPulseTween = ui_image_red_glow.DOFade(0.92f, 0.75f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }

            if (ui_transform_bomb_icon != null)
            {
                ui_transform_bomb_icon.localScale = Vector3.one;
                _iconIdleTween = ui_transform_bomb_icon.DOScale(1.07f, 0.75f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        public void HidePopup()
        {
            KillTweens();

            if (ui_canvas_group != null)
            {
                ui_canvas_group.blocksRaycasts = false;
                ui_canvas_group.interactable = false;
                ui_canvas_group.DOFade(0f, 0.2f).SetEase(Ease.InQuad);
            }
            else if (ui_panel_popup_root != null)
            {
                ui_panel_popup_root.SetActive(false);
            }
        }

        public void HideImmediate()
        {
            KillTweens();

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
            if (ui_panel_popup_root == null)
                ui_panel_popup_root = gameObject;

            if (ui_canvas_group == null)
                ui_canvas_group = GetComponent<CanvasGroup>();

            if (ui_button_giveup == null)
            {
                var ButtonsTr = transform.Find("ui_transform_buttons");
                var tr = ButtonsTr != null ? ButtonsTr.Find("ui_button_giveup") : transform.Find("ui_button_giveup");
                if (tr != null) ui_button_giveup = tr.GetComponent<Button>();
            }

            if (ui_button_revive == null)
            {
                var ButtonsTr = transform.Find("ui_transform_buttons");
                var tr = ButtonsTr != null ? ButtonsTr.Find("ui_button_revive") : transform.Find("ui_button_revive");
                if (tr != null) ui_button_revive = tr.GetComponent<Button>();
            }

            if (ui_button_revive_ad == null)
            {
                var ButtonsTr = transform.Find("ui_transform_buttons");
                var tr = ButtonsTr != null ? ButtonsTr.Find("ui_button_revive_ad") : transform.Find("ui_button_revive_ad");
                if (tr != null) ui_button_revive_ad = tr.GetComponent<Button>();
            }
        }
    }
}
