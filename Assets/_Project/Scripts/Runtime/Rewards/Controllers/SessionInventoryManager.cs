using System.Collections.Generic;
using UnityEngine;
using Vertigo.Core.Events;
using Vertigo.Core.Interfaces;

namespace Vertigo.Rewards.Controllers
{
    public class SessionInventoryManager : MonoBehaviour, ISessionInventory
    {
        // Dictionary key: Reward Id, Value: (IRewardData, amount)
        private readonly Dictionary<string, (IRewardData reward, int amount)> _sessionItems 
            = new Dictionary<string, (IRewardData reward, int amount)>();

        public IReadOnlyDictionary<string, (IRewardData reward, int amount)> Items => _sessionItems;
        public int TotalItemCount => _sessionItems.Count;

        private void OnEnable()
        {
            GameEvents.OnGameRestartRequested += OnRestartRequested;
            GameEvents.OnCashOutRequested += OnCashOutRequested;
        }

        private void OnDisable()
        {
            GameEvents.OnGameRestartRequested -= OnRestartRequested;
            GameEvents.OnCashOutRequested -= OnCashOutRequested;
        }

        public int GetAmount(string rewardId)
        {
            if (string.IsNullOrEmpty(rewardId)) return 0;
            return _sessionItems.TryGetValue(rewardId, out var item) ? item.amount : 0;
        }

        public void AddReward(IRewardData reward, int amount)
        {
            AddReward(reward, amount, false);
        }

        public void AddReward(IRewardData reward, int amount, bool suppressInventoryUpdateEvent)
        {
            if (reward == null || reward.IsBomb || amount <= 0) return;

            if (_sessionItems.TryGetValue(reward.Id, out var existing))
            {
                _sessionItems[reward.Id] = (reward, existing.amount + amount);
            }
            else
            {
                _sessionItems[reward.Id] = (reward, amount);
            }

            GameEvents.FireRewardCollected(reward, amount);
            if (!suppressInventoryUpdateEvent)
            {
                GameEvents.FireInventoryUpdated(_sessionItems);
            }
            Debug.Log($"[SessionInventory] Added {reward.DisplayName} x{amount}. Total unique items: {_sessionItems.Count}");
        }

        public void ClearSession()
        {
            _sessionItems.Clear();
            GameEvents.FireInventoryCleared();
            GameEvents.FireInventoryUpdated(_sessionItems);
            Debug.Log("[SessionInventory] Session rewards cleared!");
        }

        public void BankRewards()
        {
            // Persist rewards to PlayerPrefs or permanent storage
            foreach (var kvp in _sessionItems)
            {
                string key = $"PlayerBank_{kvp.Key}";
                int currentSaved = PlayerPrefs.GetInt(key, 0);
                PlayerPrefs.SetInt(key, currentSaved + kvp.Value.amount);
            }
            PlayerPrefs.Save();

            GameEvents.FireCashOutSuccess(new Dictionary<string, (IRewardData, int)>(_sessionItems));
            _sessionItems.Clear();
            GameEvents.FireInventoryCleared();
            GameEvents.FireInventoryUpdated(_sessionItems);
            Debug.Log("[SessionInventory] All rewards successfully banked!");
        }

        private void OnRestartRequested()
        {
            ClearSession();
        }

        private void OnCashOutRequested()
        {
            BankRewards();
        }
    }
}
