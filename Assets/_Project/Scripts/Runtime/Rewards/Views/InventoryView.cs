using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Enums;
using Vertigo.Core.Events;
using Vertigo.Core.Interfaces;
using Vertigo.Zone.Data;

namespace Vertigo.Rewards.Views
{
    public class InventoryView : MonoBehaviour
    {
        [Header("Cash Out Button")]
        [SerializeField] private Button ui_button_cashout;

        [Header("Scroll Settings")]
        [SerializeField] private ScrollRect ui_scroll_rect;

        [Header("Slots Container")]
        [SerializeField] private Transform ui_container_slots;
        [SerializeField] private InventoryRewardSlotView slotPrefab;

        private readonly Dictionary<string, InventoryRewardSlotView> _spawnedSlots 
            = new Dictionary<string, InventoryRewardSlotView>();

        private bool _canCashOutInZone;
        private bool _isSpinning;

        private void Awake()
        {
            if (ui_button_cashout != null)
            {
                ui_button_cashout.onClick.AddListener(OnCashOutClicked);
            }
            SetManualScrollEnabled(true);
        }

        private void OnEnable()
        {
            GameEvents.OnInventoryUpdated += OnInventoryUpdated;
            GameEvents.OnZoneChanged += OnZoneChanged;
            GameEvents.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnInventoryUpdated -= OnInventoryUpdated;
            GameEvents.OnZoneChanged -= OnZoneChanged;
            GameEvents.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnCashOutClicked()
        {
            GameEvents.FireCashOutRequested();
        }

        private void OnGameStateChanged(GameState state)
        {
            _isSpinning = (state != GameState.Idle);
            UpdateCashOutButtonState();

            // Disable manual scrolling during spin/award/transition, re-enable when Idle
            bool allowManualScroll = (state == GameState.Idle);
            SetManualScrollEnabled(allowManualScroll);
        }

        public void SetManualScrollEnabled(bool isEnabled)
        {
            if (ui_scroll_rect != null)
            {
                ui_scroll_rect.vertical = isEnabled;
                if (!isEnabled)
                {
                    ui_scroll_rect.StopMovement();
                }
            }
        }

        private void OnZoneChanged(ZoneData zone)
        {
            _canCashOutInZone = zone != null && zone.CanCashOut;
            UpdateCashOutButtonState();
        }

        private void UpdateCashOutButtonState()
        {
            if (ui_button_cashout != null)
            {
                // Can only cash out when wheel is NOT spinning AND zone is safe/super
                ui_button_cashout.interactable = _canCashOutInZone && !_isSpinning;
            }
        }

        public InventoryRewardSlotView GetOrCreateSlot(IRewardData reward, int initialAmount)
        {
            if (reward == null) return null;
            if (!_spawnedSlots.TryGetValue(reward.Id, out var slotView) || slotView == null)
            {
                if (slotPrefab != null && ui_container_slots != null)
                {
                    slotView = Instantiate(slotPrefab, ui_container_slots);
                    slotView.gameObject.SetActive(true);
                    slotView.Setup(reward.Icon, initialAmount);
                    _spawnedSlots[reward.Id] = slotView;

                    if (ui_container_slots is RectTransform containerRT)
                    {
                        Canvas.ForceUpdateCanvases();
                        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRT);
                    }
                }
            }
            return slotView;
        }

        public bool IsSlotVisible(InventoryRewardSlotView slotView, float tolerance = 15f)
        {
            if (ui_scroll_rect == null || slotView == null) return false;

            RectTransform viewportRT = ui_scroll_rect.viewport != null 
                ? ui_scroll_rect.viewport 
                : ui_scroll_rect.GetComponent<RectTransform>();

            if (viewportRT == null) return false;

            RectTransform slotRT = slotView.GetComponent<RectTransform>();
            Vector3[] slotCorners = new Vector3[4];
            slotRT.GetWorldCorners(slotCorners);

            float slotMinY = float.MaxValue;
            float slotMaxY = float.MinValue;

            for (int i = 0; i < 4; i++)
            {
                Vector3 localPos = viewportRT.InverseTransformPoint(slotCorners[i]);
                if (localPos.y < slotMinY) slotMinY = localPos.y;
                if (localPos.y > slotMaxY) slotMaxY = localPos.y;
            }

            Rect viewRect = viewportRT.rect;
            return (slotMinY >= viewRect.yMin + tolerance && slotMaxY <= viewRect.yMax - tolerance);
        }

        public Tween ScrollToSlot(InventoryRewardSlotView slotView, bool smooth = true, float duration = 0.35f)
        {
            if (ui_scroll_rect == null || slotView == null) return null;

            RectTransform viewportRT = ui_scroll_rect.viewport != null 
                ? ui_scroll_rect.viewport 
                : ui_scroll_rect.GetComponent<RectTransform>();

            if (ui_container_slots is RectTransform contentRT && viewportRT != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);

                float contentHeight = contentRT.rect.height;
                float viewportHeight = viewportRT.rect.height;

                if (contentHeight <= viewportHeight)
                {
                    ui_scroll_rect.verticalNormalizedPosition = 1f;
                    return null;
                }

                RectTransform slotRT = slotView.GetComponent<RectTransform>();
                Vector3 slotLocalInContent = contentRT.InverseTransformPoint(slotRT.position);
                float slotCenterY = -slotLocalInContent.y;
                float targetY = slotCenterY - (viewportHeight * 0.5f);
                float maxScroll = contentHeight - viewportHeight;
                float targetNorm = 1f - Mathf.Clamp01(targetY / maxScroll);

                ui_scroll_rect.DOKill();
                if (smooth)
                {
                    return DOTween.To(() => ui_scroll_rect.verticalNormalizedPosition, 
                               x => ui_scroll_rect.verticalNormalizedPosition = x, 
                               targetNorm, duration)
                           .SetEase(Ease.OutQuad);
                }
                else
                {
                    ui_scroll_rect.verticalNormalizedPosition = targetNorm;
                    return null;
                }
            }
            return null;
        }

        public void ScrollToTop(bool smooth = true)
        {
            if (ui_scroll_rect == null) return;
            ui_scroll_rect.DOKill();
            if (smooth)
                DOTween.To(() => ui_scroll_rect.verticalNormalizedPosition, x => ui_scroll_rect.verticalNormalizedPosition = x, 1f, 0.3f).SetEase(Ease.OutQuad);
            else
                ui_scroll_rect.verticalNormalizedPosition = 1f;
        }

        public void ScrollToBottom(bool smooth = true)
        {
            if (ui_scroll_rect == null) return;
            ui_scroll_rect.DOKill();
            if (smooth)
                DOTween.To(() => ui_scroll_rect.verticalNormalizedPosition, x => ui_scroll_rect.verticalNormalizedPosition = x, 0f, 0.3f).SetEase(Ease.OutQuad);
            else
                ui_scroll_rect.verticalNormalizedPosition = 0f;
        }

        public InventoryRewardSlotView GetSlot(string rewardId)
        {
            if (string.IsNullOrEmpty(rewardId)) return null;
            _spawnedSlots.TryGetValue(rewardId, out var slot);
            return slot;
        }

        private void OnInventoryUpdated(IReadOnlyDictionary<string, (IRewardData reward, int amount)> items)
        {
            if (items == null || items.Count == 0)
            {
                ClearAllSlots();
                return;
            }

            foreach (var kvp in items)
            {
                string id = kvp.Key;
                IRewardData reward = kvp.Value.reward;
                int amount = kvp.Value.amount;

                var slotView = GetOrCreateSlot(reward, amount);
                if (slotView != null)
                {
                    slotView.Setup(reward.Icon, amount);
                }
            }
        }

        private void ClearAllSlots()
        {
            if (ui_scroll_rect != null)
            {
                ui_scroll_rect.DOKill();
                ui_scroll_rect.verticalNormalizedPosition = 1f;
            }

            foreach (var kvp in _spawnedSlots)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }
            _spawnedSlots.Clear();
        }

        private void OnValidate()
        {
            if (ui_button_cashout == null)
            {
                var tr = transform.Find("ui_button_cashout");
                if (tr != null) ui_button_cashout = tr.GetComponent<Button>();
            }

            if (ui_scroll_rect == null)
            {
                ui_scroll_rect = GetComponentInChildren<ScrollRect>(true);
            }
        }
    }
}
