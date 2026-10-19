using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Vertigo.Core.Enums;
using Vertigo.Core.Events;
using Vertigo.Core.Interfaces;

namespace Vertigo.UI.Panels
{
    /// <summary>
    /// Handles the Cash Out reward reveal sequence (Phase 1: sequential chest opening)
    /// and the final summary grid (Phase 2: "YOU GOT:" cards with Continue button).
    /// </summary>
    public class CashOutRewardSummaryView : MonoBehaviour
    {
        [Header("Root & Canvas")]
        [SerializeField] private GameObject ui_panel_summary_root;
        [SerializeField] private CanvasGroup ui_canvas_group;

        [Header("Top Header & Controls")]
        [SerializeField] private TMP_Text ui_text_header;
        [SerializeField] private Button ui_button_skip;
        [SerializeField] private Button ui_button_continue;

        [Header("Phase 1 - Chest Reveal Stage")]
        [SerializeField] private RectTransform ui_transform_reveal_stage;
        [SerializeField] private RectTransform ui_transform_chest;
        [SerializeField] private RectTransform ui_badge_remaining;
        [SerializeField] private TMP_Text ui_text_remaining_count;
        [SerializeField] private RectTransform ui_transform_active_card;
        [SerializeField] private Image ui_image_active_card_bg;
        [SerializeField] private Image ui_image_active_card_glow;
        [SerializeField] private TMP_Text ui_text_active_name;
        [SerializeField] private Image ui_image_active_icon;
        [SerializeField] private TMP_Text ui_text_active_amount;
        [SerializeField] private CanvasGroup ui_active_card_canvas_group;

        [Header("Phase 1 - Active Cards (Prefab Instances)")]
        [SerializeField] private CashOutSummaryCardView activeSilverCard;
        [SerializeField] private CashOutSummaryCardView activeGoldCard;

        [Header("Card Themes (Silver vs Gold)")]
        [SerializeField] private Sprite silverCardSprite;
        [SerializeField] private Sprite goldCardSprite;

        [Header("Phase 2 - Final Summary Stage")]
        [SerializeField] private RectTransform ui_transform_summary_stage;
        [SerializeField] private Transform ui_container_cards;
        [SerializeField] private CashOutSummaryCardView silverCardPrefab;
        [SerializeField] private CashOutSummaryCardView goldCardPrefab;
        [SerializeField] private CashOutSummaryCardView cardTemplate;
        [SerializeField] private ScrollRect ui_scroll_rect;

        [Header("Animation Settings")]
        [SerializeField, Range(0.4f, 1.5f)] private float cardDisplayDuration = 0.75f;
        [SerializeField, Range(0.2f, 0.8f)] private float cardPopDuration = 0.4f;

        public bool IsPresenting { get; private set; }
        public event Action OnSummaryCompleted;

        private Coroutine _revealCoroutine;
        private Sequence _activeCardSequence;
        private Sequence _panelFadeSequence;
        private bool _isSkipped;
        private readonly List<(IRewardData reward, int amount)> _itemsList = new();
        private readonly List<CashOutSummaryCardView> _spawnedCardViews = new();

        private void Awake()
        {
            if (ui_panel_summary_root == null) ui_panel_summary_root = gameObject;

            BindButton(ui_button_skip, OnSkipClicked);
            BindButton(ui_button_continue, OnContinueClicked);

            if (cardTemplate != null && cardTemplate.gameObject.scene.IsValid())
            {
                cardTemplate.gameObject.SetActive(false);
            }

            EnsureActiveCards();
            HideImmediate();
        }

        private void OnEnable()
        {
            GameEvents.OnCashOutSuccess += HandleCashOutSuccess;
            GameEvents.OnGameRestartRequested += HideImmediate;
        }

        private void OnDisable()
        {
            GameEvents.OnCashOutSuccess -= HandleCashOutSuccess;
            GameEvents.OnGameRestartRequested -= HideImmediate;
            KillAllAnimations();
        }

        private void EnsureActiveCards()
        {
            if (ui_transform_active_card == null) return;

            if (ui_image_active_card_bg != null) ui_image_active_card_bg.gameObject.SetActive(false);
            if (ui_text_active_name != null) ui_text_active_name.gameObject.SetActive(false);
            if (ui_image_active_icon != null) ui_image_active_icon.gameObject.SetActive(false);
            if (ui_text_active_amount != null) ui_text_active_amount.gameObject.SetActive(false);

            EnsureCardInstance(ref activeSilverCard, silverCardPrefab, "ActiveCard_Silver");
            EnsureCardInstance(ref activeGoldCard, goldCardPrefab, "ActiveCard_Gold");

            if (activeSilverCard != null) activeSilverCard.gameObject.SetActive(false);
            if (activeGoldCard != null) activeGoldCard.gameObject.SetActive(false);
        }

        private void EnsureCardInstance(ref CashOutSummaryCardView cardField, CashOutSummaryCardView prefab, string childName)
        {
            if (cardField != null) return;

            Transform existing = ui_transform_active_card.Find(childName);
            if (existing != null)
            {
                cardField = existing.GetComponent<CashOutSummaryCardView>();
            }
            else if (prefab != null)
            {
                cardField = Instantiate(prefab, ui_transform_active_card);
                cardField.name = childName;
                RectTransform rt = (RectTransform)cardField.transform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
            }
        }

        public void HideImmediate()
        {
            KillAllAnimations();
            IsPresenting = false;

            if (activeSilverCard != null) activeSilverCard.gameObject.SetActive(false);
            if (activeGoldCard != null) activeGoldCard.gameObject.SetActive(false);
            if (ui_panel_summary_root != null && !ui_panel_summary_root.activeSelf)
            {
                ui_panel_summary_root.SetActive(true);
            }

            SetCanvasGroupState(0f, false);
            if (ui_transform_reveal_stage != null) ui_transform_reveal_stage.gameObject.SetActive(false);
            if (ui_transform_summary_stage != null) ui_transform_summary_stage.gameObject.SetActive(false);
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

        private void KillAllAnimations()
        {
            if (_revealCoroutine != null)
            {
                StopCoroutine(_revealCoroutine);
                _revealCoroutine = null;
            }
            _activeCardSequence?.Kill();
            _panelFadeSequence?.Kill();
        }

        private void HandleCashOutSuccess(IReadOnlyDictionary<string, (IRewardData reward, int amount)> bankedItems)
        {
            if (bankedItems == null || bankedItems.Count == 0)
            {
                IsPresenting = false;
                return;
            }
            StartPresentation(bankedItems);
        }

        public void StartPresentation(IReadOnlyDictionary<string, (IRewardData reward, int amount)> items)
        {
            KillAllAnimations();
            _itemsList.Clear();

            foreach (var (_, (reward, amount)) in items)
            {
                if (amount > 0 && reward != null)
                {
                    _itemsList.Add((reward, amount));
                }
            }

            if (_itemsList.Count == 0)
            {
                IsPresenting = false;
                return;
            }

            IsPresenting = true;
            _isSkipped = false;

            if (ui_panel_summary_root != null && !ui_panel_summary_root.activeSelf)
                ui_panel_summary_root.SetActive(true);

            if (ui_transform_reveal_stage != null) ui_transform_reveal_stage.gameObject.SetActive(true);
            if (ui_transform_summary_stage != null) ui_transform_summary_stage.gameObject.SetActive(false);
            if (ui_button_skip != null) ui_button_skip.gameObject.SetActive(true);
            if (ui_text_header != null) ui_text_header.gameObject.SetActive(false);
            if (ui_button_continue != null) ui_button_continue.gameObject.SetActive(false);

            SetCanvasGroupState(0f, true);
            if (ui_canvas_group != null)
            {
                _panelFadeSequence = DOTween.Sequence().Append(ui_canvas_group.DOFade(1f, 0.3f));
            }

            EnsureActiveCards();
            _revealCoroutine = StartCoroutine(RevealRoutine());
        }

        private IEnumerator RevealRoutine()
        {
            int remainingCount = _itemsList.Count;
            GameEvents.FireSfx(SoundEffectType.ChestAppear);

            if (ui_transform_chest != null)
            {
                ui_transform_chest.localScale = Vector3.zero;
                ui_transform_chest.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            }

            if (ui_badge_remaining != null)
            {
                ui_badge_remaining.localScale = Vector3.zero;
                ui_badge_remaining.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetDelay(0.1f);
            }

            if (ui_text_remaining_count != null) ui_text_remaining_count.text = remainingCount.ToString();
            if (ui_transform_active_card != null) ui_transform_active_card.gameObject.SetActive(false);

            yield return new WaitForSeconds(0.4f);

            for (int i = 0; i < _itemsList.Count; i++)
            {
                if (_isSkipped) break;

                var (reward, amount) = _itemsList[i];
                bool isValuable = reward != null && reward.IsValuable;

                GameEvents.FireSfx(isValuable ? SoundEffectType.CardRevealGold : SoundEffectType.CardRevealSilver);
                SetupActiveRevealCard(reward, amount, isValuable);

                remainingCount--;
                if (ui_text_remaining_count != null) ui_text_remaining_count.text = remainingCount.ToString();
                if (ui_badge_remaining != null) ui_badge_remaining.DOPunchScale(Vector3.one * 0.35f, 0.22f, 7, 1f);

                if (ui_transform_active_card != null)
                {
                    ui_transform_active_card.gameObject.SetActive(true);
                    ui_transform_active_card.anchoredPosition = new Vector2(0f, -180f);
                    ui_transform_active_card.localScale = Vector3.zero;
                    ui_transform_active_card.localEulerAngles = new Vector3(0f, 90f, 0f);

                    if (ui_active_card_canvas_group != null) ui_active_card_canvas_group.alpha = 1f;

                    _activeCardSequence?.Kill();
                    _activeCardSequence = DOTween.Sequence()
                        .Append(ui_transform_active_card.DOAnchorPosY(70f, cardPopDuration).SetEase(Ease.OutCubic))
                        .Join(ui_transform_active_card.DOScale(1.08f, cardPopDuration).SetEase(Ease.OutBack))
                        .Join(ui_transform_active_card.DOLocalRotate(Vector3.zero, cardPopDuration * 0.75f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic))
                        .Append(ui_transform_active_card.DOScale(1f, 0.15f).SetEase(Ease.InOutSine));

                    yield return _activeCardSequence.WaitForCompletion();
                }

                float elapsed = 0f;
                while (elapsed < cardDisplayDuration && !_isSkipped)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (_isSkipped) break;

                if (i < _itemsList.Count - 1 && ui_transform_active_card != null)
                {
                    Tween exitTween = ui_transform_active_card.DOScale(0.2f, 0.18f).SetEase(Ease.InBack);
                    if (ui_active_card_canvas_group != null) ui_active_card_canvas_group.DOFade(0f, 0.18f);
                    yield return exitTween.WaitForCompletion();
                }
            }

            TransitionToSummaryGrid();
        }

        private void SetupActiveRevealCard(IRewardData reward, int amount, bool isValuable)
        {
            CashOutSummaryCardView cardToShow = isValuable ? activeGoldCard : activeSilverCard;
            CashOutSummaryCardView cardToHide = isValuable ? activeSilverCard : activeGoldCard;

            if (cardToHide != null) cardToHide.gameObject.SetActive(false);
            if (cardToShow != null)
            {
                cardToShow.gameObject.SetActive(true);
                cardToShow.Setup(reward, amount);
            }
        }

        private void TransitionToSummaryGrid()
        {
            _activeCardSequence?.Kill();
            if (ui_transform_reveal_stage != null) ui_transform_reveal_stage.gameObject.SetActive(false);
            if (ui_button_skip != null) ui_button_skip.gameObject.SetActive(false);
            ShowFinalSummaryGrid();
        }

        private void ShowFinalSummaryGrid()
        {
            GameEvents.FireSfx(SoundEffectType.CashOutSummaryComplete);

            if (ui_transform_summary_stage != null) ui_transform_summary_stage.gameObject.SetActive(true);

            if (ui_text_header != null)
            {
                ui_text_header.gameObject.SetActive(true);
                ui_text_header.transform.localScale = Vector3.zero;
                ui_text_header.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
            }

            PopulateSummaryCards();

            if (ui_button_continue != null)
            {
                ui_button_continue.gameObject.SetActive(true);
                ui_button_continue.transform.localScale = Vector3.zero;
                ui_button_continue.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetDelay(0.2f);
            }
        }

        private void PopulateSummaryCards()
        {
            foreach (var card in _spawnedCardViews)
            {
                if (card != null) Destroy(card.gameObject);
            }
            _spawnedCardViews.Clear();

            if (ui_container_cards == null) return;

            ConfigureSummaryScrollRect();

            if (ui_container_cards is RectTransform contentRT)
            {
                SetHorizontalAlignment(contentRT, 0f);
            }

            for (int i = 0; i < _itemsList.Count; i++)
            {
                var (reward, amount) = _itemsList[i];
                CashOutSummaryCardView prefabToUse = GetCardPrefab(reward != null && reward.IsValuable);
                if (prefabToUse == null) continue;

                CashOutSummaryCardView cardView = Instantiate(prefabToUse, ui_container_cards);
                cardView.gameObject.SetActive(true);
                cardView.Setup(reward, amount);
                _spawnedCardViews.Add(cardView);

                cardView.transform.localScale = Vector3.zero;
                cardView.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetDelay(0.05f * i);
            }

            if (ui_container_cards is RectTransform cRT)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(cRT);
                float viewportWidth = ui_scroll_rect != null && ui_scroll_rect.viewport != null
                    ? ui_scroll_rect.viewport.rect.width
                    : 1720f;

                SetHorizontalAlignment(cRT, cRT.rect.width < viewportWidth ? 0.5f : 0f);
            }

            if (ui_scroll_rect != null)
            {
                Canvas.ForceUpdateCanvases();
                ui_scroll_rect.horizontalNormalizedPosition = 0f;
            }
        }

        private void ConfigureSummaryScrollRect()
        {
            if (ui_scroll_rect == null) return;

            ui_scroll_rect.horizontal = true;
            ui_scroll_rect.vertical = false;
            ui_scroll_rect.scrollSensitivity = 25f;

            if (ui_scroll_rect.viewport != null && ui_scroll_rect.viewport.TryGetComponent<Image>(out var vpImg))
            {
                vpImg.raycastTarget = true;
            }
        }

        private static void SetHorizontalAlignment(RectTransform rt, float xAnchor)
        {
            Vector2 anchor = new Vector2(xAnchor, 0.5f);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = Vector2.zero;
        }

        private CashOutSummaryCardView GetCardPrefab(bool isValuable)
        {
            if (isValuable)
                return goldCardPrefab != null ? goldCardPrefab : (silverCardPrefab != null ? silverCardPrefab : cardTemplate);

            return silverCardPrefab != null ? silverCardPrefab : (goldCardPrefab != null ? goldCardPrefab : cardTemplate);
        }

        private void OnSkipClicked()
        {
            if (_isSkipped) return;
            _isSkipped = true;

            GameEvents.FireSfx(SoundEffectType.ButtonClick);

            if (_revealCoroutine != null)
            {
                StopCoroutine(_revealCoroutine);
                _revealCoroutine = null;
            }

            TransitionToSummaryGrid();
        }

        private void OnContinueClicked()
        {
            GameEvents.FireSfx(SoundEffectType.ButtonClick);
            KillAllAnimations();

            if (ui_canvas_group != null)
            {
                ui_canvas_group.blocksRaycasts = false;
                ui_canvas_group.interactable = false;
                ui_canvas_group.DOFade(0f, 0.28f).OnComplete(CompleteSummary);
            }
            else
            {
                CompleteSummary();
            }
        }

        private void CompleteSummary()
        {
            HideImmediate();
            OnSummaryCompleted?.Invoke();
        }

#if UNITY_EDITOR
        [ContextMenu("Debug: Test Summary Presentation (Silver & Gold Mix)")]
        private void DebugTestPresentation()
        {
            var dummyItems = new Dictionary<string, (IRewardData reward, int amount)>();
            var rewards = Resources.FindObjectsOfTypeAll<Vertigo.Rewards.Data.RewardDataSO>();

            var standards = new List<Vertigo.Rewards.Data.RewardDataSO>();
            var valuables = new List<Vertigo.Rewards.Data.RewardDataSO>();

            foreach (var r in rewards)
            {
                if (r == null || r.IsBomb) continue;
                if (r.IsValuable) valuables.Add(r);
                else standards.Add(r);
            }

            if (standards.Count > 0) dummyItems[standards[0].Id] = (standards[0], 54050);
            if (valuables.Count > 0) dummyItems[valuables[0].Id] = (valuables[0], 16);
            if (standards.Count > 1) dummyItems[standards[1].Id] = (standards[1], 340);
            if (valuables.Count > 1) dummyItems[valuables[1].Id] = (valuables[1], 35);

            if (dummyItems.Count > 0) StartPresentation(dummyItems);
        }
#endif
    }
}
