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

        private readonly Dictionary<string, InventoryRewardSlotView> _spawnedSlots = new();
        private readonly Vector3[] _slotCorners = new Vector3[4];

        private bool _canCashOut;
        private bool _isIdle;

        private RectTransform ViewportRT => ui_scroll_rect.viewport != null
            ? ui_scroll_rect.viewport
            : (RectTransform)ui_scroll_rect.transform;

        private void Awake()
        {
            if (ui_button_cashout != null)
                ui_button_cashout.onClick.AddListener(GameEvents.FireCashOutPromptRequested);

            SetManualScrollEnabled(true);
            UpdateCashOutButton();
        }

        private void OnEnable()
        {
            GameEvents.OnInventoryUpdated += OnInventoryUpdated;
            GameEvents.OnGameStateChanged += OnGameStateChanged;
            GameEvents.OnZoneChanged += OnZoneChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnInventoryUpdated -= OnInventoryUpdated;
            GameEvents.OnGameStateChanged -= OnGameStateChanged;
            GameEvents.OnZoneChanged -= OnZoneChanged;
        }

        private void OnZoneChanged(ZoneData zone)
        {
            _canCashOut = zone != null && zone.CanCashOut;
            UpdateCashOutButton();
        }

        private void OnGameStateChanged(GameState state)
        {
            SetManualScrollEnabled(_isIdle = state == GameState.Idle);
            UpdateCashOutButton();
        }

        private void UpdateCashOutButton()
        {
            if (ui_button_cashout != null)
                ui_button_cashout.interactable = _isIdle && _canCashOut;
        }

        public void SetManualScrollEnabled(bool isEnabled)
        {
            if (ui_scroll_rect == null) return;

            ui_scroll_rect.vertical = isEnabled;
            if (!isEnabled)
                ui_scroll_rect.StopMovement();
        }

        public InventoryRewardSlotView GetOrCreateSlot(IRewardData reward, int initialAmount)
        {
            if (reward == null) return null;

            if (!_spawnedSlots.TryGetValue(reward.Id, out var slotView) || slotView == null)
            {
                if (slotPrefab == null || ui_container_slots == null) return null;

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
            return slotView;
        }

        public bool IsSlotVisible(InventoryRewardSlotView slotView, float tolerance = 15f)
        {
            if (ui_scroll_rect == null || slotView == null) return false;

            RectTransform viewportRT = ViewportRT;
            if (viewportRT == null) return false;

            ((RectTransform)slotView.transform).GetWorldCorners(_slotCorners);

            float slotMinY = float.MaxValue;
            float slotMaxY = float.MinValue;

            for (int i = 0; i < 4; i++)
            {
                float localY = viewportRT.InverseTransformPoint(_slotCorners[i]).y;
                if (localY < slotMinY) slotMinY = localY;
                if (localY > slotMaxY) slotMaxY = localY;
            }

            Rect viewRect = viewportRT.rect;
            return slotMinY >= viewRect.yMin + tolerance && slotMaxY <= viewRect.yMax - tolerance;
        }

        public Tween ScrollToSlot(InventoryRewardSlotView slotView, bool smooth = true, float duration = 0.35f)
        {
            if (ui_scroll_rect == null || slotView == null || ui_container_slots is not RectTransform contentRT)
                return null;

            RectTransform viewportRT = ViewportRT;
            if (viewportRT == null) return null;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);

            float contentHeight = contentRT.rect.height;
            float viewportHeight = viewportRT.rect.height;

            if (contentHeight <= viewportHeight)
            {
                ui_scroll_rect.verticalNormalizedPosition = 1f;
                return null;
            }

            Vector3 slotLocalInContent = contentRT.InverseTransformPoint(slotView.transform.position);
            float targetY = -slotLocalInContent.y - (viewportHeight * 0.5f);
            float targetNorm = 1f - Mathf.Clamp01(targetY / (contentHeight - viewportHeight));

            return ScrollToNormalizedPosition(targetNorm, smooth, duration);
        }

        public void ScrollToTop(bool smooth = true) => ScrollToNormalizedPosition(1f, smooth);

        public void ScrollToBottom(bool smooth = true) => ScrollToNormalizedPosition(0f, smooth);

        private Tween ScrollToNormalizedPosition(float targetNorm, bool smooth, float duration = 0.3f)
        {
            if (ui_scroll_rect == null) return null;

            ui_scroll_rect.DOKill();
            if (!smooth)
            {
                ui_scroll_rect.verticalNormalizedPosition = targetNorm;
                return null;
            }

            return DOTween.To(
                () => ui_scroll_rect.verticalNormalizedPosition,
                x => ui_scroll_rect.verticalNormalizedPosition = x,
                targetNorm,
                duration
            ).SetEase(Ease.OutQuad);
        }

        private void OnInventoryUpdated(IReadOnlyDictionary<string, (IRewardData reward, int amount)> items)
        {
            if (items == null || items.Count == 0)
            {
                ClearAllSlots();
                return;
            }

            foreach (var (_, (reward, amount)) in items)
            {
                var slotView = GetOrCreateSlot(reward, amount);
                if (slotView != null)
                    slotView.Setup(reward.Icon, amount);
            }
        }

        private void ClearAllSlots()
        {
            if (ui_scroll_rect != null)
            {
                ui_scroll_rect.DOKill();
                ui_scroll_rect.verticalNormalizedPosition = 1f;
            }

            foreach (var slot in _spawnedSlots.Values)
            {
                if (slot != null) Destroy(slot.gameObject);
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
                ui_scroll_rect = GetComponentInChildren<ScrollRect>(true);
        }
    }
}
