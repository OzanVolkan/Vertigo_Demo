using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
            if (ui_panel_summary_root == null)
                ui_panel_summary_root = gameObject;

            if (ui_canvas_group == null)
                ui_canvas_group = ui_panel_summary_root.GetComponent<CanvasGroup>();

            if (ui_button_skip != null)
            {
                ui_button_skip.onClick.RemoveListener(OnSkipClicked);
                ui_button_skip.onClick.AddListener(OnSkipClicked);
            }

            if (ui_button_continue != null)
            {
                ui_button_continue.onClick.RemoveListener(OnContinueClicked);
                ui_button_continue.onClick.AddListener(OnContinueClicked);
            }

            if (cardTemplate != null && cardTemplate.gameObject.scene.IsValid())
            {
                cardTemplate.gameObject.SetActive(false);
            }

            EnsureActiveCards();
            HideImmediate();
        }

        private void EnsureActiveCards()
        {
            if (ui_transform_active_card == null) return;

            // Hide old legacy static card parts if they exist in scene
            if (ui_image_active_card_bg != null) ui_image_active_card_bg.gameObject.SetActive(false);
            if (ui_text_active_name != null) ui_text_active_name.gameObject.SetActive(false);
            if (ui_image_active_icon != null) ui_image_active_icon.gameObject.SetActive(false);
            if (ui_text_active_amount != null) ui_text_active_amount.gameObject.SetActive(false);

            if (activeSilverCard == null)
            {
                Transform silverTr = ui_transform_active_card.Find("ActiveCard_Silver");
                if (silverTr != null)
                {
                    activeSilverCard = silverTr.GetComponent<CashOutSummaryCardView>();
                }
                else if (silverCardPrefab != null)
                {
                    activeSilverCard = Instantiate(silverCardPrefab, ui_transform_active_card);
                    activeSilverCard.name = "ActiveCard_Silver";
                    ResetCardTransform(activeSilverCard.GetComponent<RectTransform>());
                }
            }

            if (activeGoldCard == null)
            {
                Transform goldTr = ui_transform_active_card.Find("ActiveCard_Gold");
                if (goldTr != null)
                {
                    activeGoldCard = goldTr.GetComponent<CashOutSummaryCardView>();
                }
                else if (goldCardPrefab != null)
                {
                    activeGoldCard = Instantiate(goldCardPrefab, ui_transform_active_card);
                    activeGoldCard.name = "ActiveCard_Gold";
                    ResetCardTransform(activeGoldCard.GetComponent<RectTransform>());
                }
            }

            if (activeSilverCard != null) activeSilverCard.gameObject.SetActive(false);
            if (activeGoldCard != null) activeGoldCard.gameObject.SetActive(false);
        }

        private void ResetCardTransform(RectTransform rt)
        {
            if (rt == null) return;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
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

            if (ui_canvas_group != null)
            {
                ui_canvas_group.alpha = 0f;
                ui_canvas_group.blocksRaycasts = false;
                ui_canvas_group.interactable = false;
            }

            if (ui_transform_reveal_stage != null) ui_transform_reveal_stage.gameObject.SetActive(false);
            if (ui_transform_summary_stage != null) ui_transform_summary_stage.gameObject.SetActive(false);
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
                Debug.Log("[CashOutSummary] No rewards to display. Skipping summary screen.");
                IsPresenting = false;
                return;
            }

            StartPresentation(bankedItems);
        }

        public void StartPresentation(IReadOnlyDictionary<string, (IRewardData reward, int amount)> items)
        {
            KillAllAnimations();
            _itemsList.Clear();

            foreach (var kvp in items)
            {
                if (kvp.Value.amount > 0 && kvp.Value.reward != null)
                {
                    _itemsList.Add((kvp.Value.reward, kvp.Value.amount));
                }
            }

            if (_itemsList.Count == 0)
            {
                IsPresenting = false;
                return;
            }

            IsPresenting = true;
            _isSkipped = false;

            // Prepare UI stages
            if (ui_panel_summary_root != null && !ui_panel_summary_root.activeSelf)
            {
                ui_panel_summary_root.SetActive(true);
            }

            if (ui_transform_reveal_stage != null) ui_transform_reveal_stage.gameObject.SetActive(true);
            if (ui_transform_summary_stage != null) ui_transform_summary_stage.gameObject.SetActive(false);
            if (ui_button_skip != null) ui_button_skip.gameObject.SetActive(true);
            if (ui_text_header != null) ui_text_header.gameObject.SetActive(false);
            if (ui_button_continue != null) ui_button_continue.gameObject.SetActive(false);

            if (ui_canvas_group != null)
            {
                ui_canvas_group.alpha = 0f;
                ui_canvas_group.blocksRaycasts = true;
                ui_canvas_group.interactable = true;
                _panelFadeSequence = DOTween.Sequence()
                    .Append(ui_canvas_group.DOFade(1f, 0.3f));
            }

            EnsureActiveCards();
            _revealCoroutine = StartCoroutine(RevealRoutine());
        }

        private IEnumerator RevealRoutine()
        {
            int remainingCount = _itemsList.Count;

            GameEvents.FireSfx(Vertigo.Core.Enums.SoundEffectType.ChestAppear);

            // Chest entrance
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

            if (ui_text_remaining_count != null)
            {
                ui_text_remaining_count.text = remainingCount.ToString();
            }

            if (ui_transform_active_card != null)
            {
                ui_transform_active_card.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(0.4f);

            // Sequential reveal loop
            for (int i = 0; i < _itemsList.Count; i++)
            {
                if (_isSkipped) break;

                var item = _itemsList[i];
                bool isValuable = item.reward != null && item.reward.IsValuable;

                GameEvents.FireSfx(isValuable
                    ? Vertigo.Core.Enums.SoundEffectType.CardRevealGold
                    : Vertigo.Core.Enums.SoundEffectType.CardRevealSilver);

                // Setup Active Card from Prefabs
                CashOutSummaryCardView cardToShow = isValuable ? activeGoldCard : activeSilverCard;
                CashOutSummaryCardView cardToHide = isValuable ? activeSilverCard : activeGoldCard;

                if (cardToHide != null) cardToHide.gameObject.SetActive(false);
                if (cardToShow != null)
                {
                    cardToShow.gameObject.SetActive(true);
                    cardToShow.Setup(item.reward, item.amount);
                }
                else
                {
                    // Fallback to legacy static fields if prefabs not instantiated
                    if (ui_image_active_card_bg != null)
                    {
                        ui_image_active_card_bg.gameObject.SetActive(true);
                        ui_image_active_card_bg.sprite = isValuable ? goldCardSprite : silverCardSprite;
                    }
                    if (ui_text_active_name != null)
                    {
                        ui_text_active_name.gameObject.SetActive(true);
                        ui_text_active_name.text = item.reward.DisplayName.ToUpper();
                    }
                    if (ui_image_active_icon != null)
                    {
                        ui_image_active_icon.gameObject.SetActive(true);
                        ui_image_active_icon.sprite = item.reward.Icon;
                    }
                    if (ui_text_active_amount != null)
                    {
                        ui_text_active_amount.gameObject.SetActive(true);
                        ui_text_active_amount.text = item.reward.FormatAmount(item.amount);
                    }
                }

                // Decrement chest remaining badge
                remainingCount--;
                if (ui_text_remaining_count != null)
                {
                    ui_text_remaining_count.text = remainingCount.ToString();
                }
                if (ui_badge_remaining != null)
                {
                    ui_badge_remaining.DOPunchScale(Vector3.one * 0.35f, 0.22f, 7, 1f);
                }

                // Animate card popping out of chest and flipping forward
                if (ui_transform_active_card != null)
                {
                    ui_transform_active_card.gameObject.SetActive(true);
                    ui_transform_active_card.anchoredPosition = new Vector2(0f, -180f);
                    ui_transform_active_card.localScale = Vector3.zero;
                    ui_transform_active_card.localEulerAngles = new Vector3(0f, 90f, 0f);

                    if (ui_active_card_canvas_group != null)
                    {
                        ui_active_card_canvas_group.alpha = 1f;
                    }

                    _activeCardSequence?.Kill();
                    _activeCardSequence = DOTween.Sequence()
                        .Append(ui_transform_active_card.DOAnchorPosY(70f, cardPopDuration).SetEase(Ease.OutCubic))
                        .Join(ui_transform_active_card.DOScale(1.08f, cardPopDuration).SetEase(Ease.OutBack))
                        .Join(ui_transform_active_card.DOLocalRotate(Vector3.zero, cardPopDuration * 0.75f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic))
                        .Append(ui_transform_active_card.DOScale(1f, 0.15f).SetEase(Ease.InOutSine));

                    yield return _activeCardSequence.WaitForCompletion();
                }

                // Display pause (user can observe card)
                float elapsed = 0f;
                while (elapsed < cardDisplayDuration && !_isSkipped)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (_isSkipped) break;

                // Exit card before next one
                if (i < _itemsList.Count - 1 && ui_transform_active_card != null)
                {
                    Tween exitTween = ui_transform_active_card.DOScale(0.2f, 0.18f).SetEase(Ease.InBack);
                    if (ui_active_card_canvas_group != null)
                    {
                        ui_active_card_canvas_group.DOFade(0f, 0.18f);
                    }
                    yield return exitTween.WaitForCompletion();
                }
            }

            // Finished all reveals naturally
            TransitionToSummaryGrid();
        }

        private void TransitionToSummaryGrid()
        {
            _activeCardSequence?.Kill();

            if (ui_transform_reveal_stage != null)
            {
                ui_transform_reveal_stage.gameObject.SetActive(false);
            }

            if (ui_button_skip != null)
            {
                ui_button_skip.gameObject.SetActive(false);
            }

            ShowFinalSummaryGrid();
        }

        private void ShowFinalSummaryGrid()
        {
            GameEvents.FireSfx(Vertigo.Core.Enums.SoundEffectType.CashOutSummaryComplete);

            if (ui_transform_summary_stage != null)
            {
                ui_transform_summary_stage.gameObject.SetActive(true);
            }

            // Top Header: "YOU GOT:"
            if (ui_text_header != null)
            {
                ui_text_header.gameObject.SetActive(true);
                ui_text_header.transform.localScale = Vector3.zero;
                ui_text_header.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
            }

            // Populate cards
            PopulateSummaryCards();

            // Continue Button at bottom
            if (ui_button_continue != null)
            {
                ui_button_continue.gameObject.SetActive(true);
                ui_button_continue.transform.localScale = Vector3.zero;
                ui_button_continue.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetDelay(0.2f);
            }
        }

        private void PopulateSummaryCards()
        {
            // Clean up previously spawned cards
            for (int i = 0; i < _spawnedCardViews.Count; i++)
            {
                if (_spawnedCardViews[i] != null)
                {
                    Destroy(_spawnedCardViews[i].gameObject);
                }
            }
            _spawnedCardViews.Clear();

            if (ui_container_cards == null) return;

            // Ensure ScrollRect can receive drags and scroll events
            if (ui_scroll_rect != null)
            {
                ui_scroll_rect.horizontal = true;
                ui_scroll_rect.vertical = false;
                ui_scroll_rect.scrollSensitivity = 25f;

                if (ui_scroll_rect.viewport != null)
                {
                    Image vpImg = ui_scroll_rect.viewport.GetComponent<Image>();
                    if (vpImg == null)
                    {
                        vpImg = ui_scroll_rect.viewport.gameObject.AddComponent<Image>();
                        vpImg.color = Color.clear;
                    }
                    vpImg.raycastTarget = true;
                }
            }

            // Ensure content container anchors are properly left-aligned for horizontal scrolling
            if (ui_container_cards is RectTransform contentRT)
            {
                contentRT.anchorMin = new Vector2(0f, 0.5f);
                contentRT.anchorMax = new Vector2(0f, 0.5f);
                contentRT.pivot = new Vector2(0f, 0.5f);
                contentRT.anchoredPosition = Vector2.zero;
            }

            // Instantiate cards from distinct Silver and Gold prefabs
            for (int i = 0; i < _itemsList.Count; i++)
            {
                var item = _itemsList[i];
                bool isValuable = item.reward != null && item.reward.IsValuable;

                CashOutSummaryCardView prefabToUse = isValuable
                    ? (goldCardPrefab != null ? goldCardPrefab : (silverCardPrefab != null ? silverCardPrefab : cardTemplate))
                    : (silverCardPrefab != null ? silverCardPrefab : (goldCardPrefab != null ? goldCardPrefab : cardTemplate));

                if (prefabToUse == null) continue;

                CashOutSummaryCardView cardView = Instantiate(prefabToUse, ui_container_cards);
                cardView.gameObject.SetActive(true);
                cardView.Setup(item.reward, item.amount);
                _spawnedCardViews.Add(cardView);

                // Punchy staggered scale-in animation
                cardView.transform.localScale = Vector3.zero;
                float delay = 0.05f * i;
                cardView.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetDelay(delay);
            }

            // Force layout rebuild so ScrollRect immediately computes the exact content width
            if (ui_container_cards is RectTransform cRT)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(cRT);

                float viewportWidth = ui_scroll_rect != null && ui_scroll_rect.viewport != null
                    ? ui_scroll_rect.viewport.rect.width
                    : 1720f;

                if (cRT.rect.width < viewportWidth)
                {
                    cRT.anchorMin = new Vector2(0.5f, 0.5f);
                    cRT.anchorMax = new Vector2(0.5f, 0.5f);
                    cRT.pivot = new Vector2(0.5f, 0.5f);
                    cRT.anchoredPosition = Vector2.zero;
                }
                else
                {
                    cRT.anchorMin = new Vector2(0f, 0.5f);
                    cRT.anchorMax = new Vector2(0f, 0.5f);
                    cRT.pivot = new Vector2(0f, 0.5f);
                    cRT.anchoredPosition = Vector2.zero;
                }
            }

            // Reset scroll position to beginning
            if (ui_scroll_rect != null)
            {
                Canvas.ForceUpdateCanvases();
                ui_scroll_rect.horizontalNormalizedPosition = 0f;
            }
        }

        private void OnSkipClicked()
        {
            if (_isSkipped) return;
            _isSkipped = true;

            GameEvents.FireSfx(Vertigo.Core.Enums.SoundEffectType.ButtonClick);

            if (_revealCoroutine != null)
            {
                StopCoroutine(_revealCoroutine);
                _revealCoroutine = null;
            }

            TransitionToSummaryGrid();
        }

        private void OnContinueClicked()
        {
            GameEvents.FireSfx(Vertigo.Core.Enums.SoundEffectType.ButtonClick);
            KillAllAnimations();

            if (ui_canvas_group != null)
            {
                ui_canvas_group.blocksRaycasts = false;
                ui_canvas_group.interactable = false;
                ui_canvas_group.DOFade(0f, 0.28f).OnComplete(() =>
                {
                    HideImmediate();
                    OnSummaryCompleted?.Invoke();
                });
            }
            else
            {
                HideImmediate();
                OnSummaryCompleted?.Invoke();
            }
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

            if (dummyItems.Count > 0)
            {
                StartPresentation(dummyItems);
            }
            else
            {
                Debug.LogWarning("[CashOutSummary] No RewardDataSO assets found to test with.");
            }
        }
#endif
    }
}
