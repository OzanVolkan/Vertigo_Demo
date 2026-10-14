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
            _isSpinning = (state == GameState.Spinning || state == GameState.Evaluating);
            UpdateCashOutButtonState();
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

                if (!_spawnedSlots.TryGetValue(id, out var slotView))
                {
                    if (slotPrefab != null && ui_container_slots != null)
                    {
                        slotView = Instantiate(slotPrefab, ui_container_slots);
                        slotView.gameObject.SetActive(true);
                        _spawnedSlots[id] = slotView;
                    }
                }

                if (slotView != null)
                {
                    slotView.Setup(reward.Icon, amount, $"{amount:N0}");
                    // Subtle punch scale animation when reward is updated
                    slotView.transform.DOKill(true);
                    slotView.transform.localScale = Vector3.one;
                    slotView.transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 2, 0.5f);
                }
            }
        }

        private void ClearAllSlots()
        {
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
        }
    }
}
