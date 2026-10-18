using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Events;

namespace Vertigo.UI.Panels
{
    /// <summary>
    /// Displays the grand victory / mission completed celebration panel when Level 60 is completed,
    /// immediately before transitioning into the Cash Out reward reveal sequence.
    /// </summary>
    public class MissionCompletePopupView : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private GameObject ui_panel_popup_root;
        [SerializeField] private CanvasGroup ui_canvas_group;
        [SerializeField] private RectTransform ui_transform_dialog;

        [Header("Texts")]
        [SerializeField] private TMP_Text ui_text_title;
        [SerializeField] private TMP_Text ui_text_subtitle;

        [Header("Buttons")]
        [SerializeField] private Button ui_button_collect_rewards;

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.35f;
        [SerializeField] private float fadeOutDuration = 0.25f;

        private Action _onCollectCallback;
        private Sequence _animSequence;
        private bool _isShowing;

        public bool IsShowing => _isShowing;

        private void Awake()
        {
            EnsureInitialized();
            if (!_isShowing)
            {
                HideImmediate();
            }
        }

        private void EnsureInitialized()
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
        }

        private void OnEnable()
        {
            GameEvents.OnGameRestartRequested += HideImmediate;
        }

        private void OnDisable()
        {
            GameEvents.OnGameRestartRequested -= HideImmediate;
            _animSequence?.Kill();
        }

        /// <summary>
        /// Displays the mission complete celebration screen.
        /// </summary>
        /// <param name="onCollect">Callback fired when the player clicks COLLECT REWARDS</param>
        public void Show(Action onCollect = null)
        {
            _isShowing = true;
            _onCollectCallback = onCollect;

            EnsureInitialized();

            GameEvents.FireSfx(Vertigo.Core.Enums.SoundEffectType.MissionCompleteVictory);

            if (ui_panel_popup_root != null)
            {
                ui_panel_popup_root.SetActive(true);
            }

            _animSequence?.Kill();
            _animSequence = DOTween.Sequence();

            if (ui_canvas_group != null)
            {
                ui_canvas_group.alpha = 0f;
                ui_canvas_group.blocksRaycasts = true;
                ui_canvas_group.interactable = true;
                _animSequence.Append(ui_canvas_group.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad));
            }

            if (ui_transform_dialog != null)
            {
                ui_transform_dialog.localScale = Vector3.one * 0.9f;
                _animSequence.Join(ui_transform_dialog.DOScale(1f, fadeInDuration * 1.2f).SetEase(Ease.OutBack));
            }

            if (ui_button_collect_rewards != null)
            {
                _animSequence.Append(ui_button_collect_rewards.transform.DOPunchScale(Vector3.one * 0.06f, 0.35f, 2, 0.5f));
            }
        }

        private void OnCollectRewardsClicked()
        {
            GameEvents.FireSfx(Vertigo.Core.Enums.SoundEffectType.ButtonClick);

            if (ui_canvas_group != null)
            {
                ui_canvas_group.interactable = false;
                ui_canvas_group.blocksRaycasts = false;
            }

            _animSequence?.Kill();
            _animSequence = DOTween.Sequence();

            if (ui_canvas_group != null)
            {
                _animSequence.Append(ui_canvas_group.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad));
            }

            if (ui_transform_dialog != null)
            {
                _animSequence.Join(ui_transform_dialog.DOScale(0.95f, fadeOutDuration).SetEase(Ease.InQuad));
            }

            _animSequence.AppendCallback(() =>
            {
                HideImmediate();
                Action callback = _onCollectCallback;
                _onCollectCallback = null;
                callback?.Invoke();
            });
        }

        public void HideImmediate()
        {
            _isShowing = false;
            _animSequence?.Kill();
            if (ui_canvas_group != null)
            {
                ui_canvas_group.alpha = 0f;
                ui_canvas_group.blocksRaycasts = false;
                ui_canvas_group.interactable = false;
            }
            if (ui_panel_popup_root != null)
            {
                ui_panel_popup_root.SetActive(false);
            }
        }
    }
}
