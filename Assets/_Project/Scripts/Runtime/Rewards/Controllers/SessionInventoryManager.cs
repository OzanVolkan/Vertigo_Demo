using System.Collections.Generic;
using UnityEngine;
using Vertigo.Core.Events;
using Vertigo.Core.Interfaces;

namespace Vertigo.Rewards.Controllers
{
    public class SessionInventoryManager : MonoBehaviour, ISessionInventory
    {
        public const string CashRewardId = "Reward_Cash";
        public const string GoldRewardId = "Reward_Gold";
        private const string BankKeyPrefix = "PlayerBank_";

        private readonly Dictionary<string, (IRewardData reward, int amount)> _sessionItems = new();

        public IReadOnlyDictionary<string, (IRewardData reward, int amount)> Items => _sessionItems;

        private void OnEnable()
        {
            GameEvents.OnGameRestartRequested += ClearSession;
            GameEvents.OnCashOutRequested += BankRewards;
        }

        private void OnDisable()
        {
            GameEvents.OnGameRestartRequested -= ClearSession;
            GameEvents.OnCashOutRequested -= BankRewards;
        }

        public int GetAmount(string rewardId) =>
            !string.IsNullOrEmpty(rewardId) && _sessionItems.TryGetValue(rewardId, out var item) ? item.amount : 0;

        public void AddReward(IRewardData reward, int amount, bool suppressInventoryUpdateEvent = false)
        {
            if (reward == null || reward.IsBomb || amount <= 0) return;

            _sessionItems[reward.Id] = (reward, GetAmount(reward.Id) + amount);

            GameEvents.FireRewardCollected(reward, amount);
            if (!suppressInventoryUpdateEvent)
            {
                GameEvents.FireInventoryUpdated(_sessionItems);
            }
        }

        public void ClearSession()
        {
            _sessionItems.Clear();
            GameEvents.FireInventoryCleared();
            GameEvents.FireInventoryUpdated(_sessionItems);
        }

        public void BankRewards()
        {
            foreach (var kvp in _sessionItems)
            {
                string key = $"{BankKeyPrefix}{kvp.Key}";
                PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key, 0) + kvp.Value.amount);
            }
            PlayerPrefs.Save();

            GameEvents.FireCashOutSuccess(new Dictionary<string, (IRewardData, int)>(_sessionItems));
            ClearSession();
        }

        public static int GetBankedAmount(string rewardId) =>
            string.IsNullOrEmpty(rewardId) ? 0 : PlayerPrefs.GetInt($"{BankKeyPrefix}{rewardId}", 0);

        public static int GetBankedCash() => GetBankedAmount(CashRewardId);

        public static int GetBankedGold() => GetBankedAmount(GoldRewardId);

        public static void AddBankedGold(int amount)
        {
            if (amount <= 0) return;
            PlayerPrefs.SetInt($"{BankKeyPrefix}{GoldRewardId}", GetBankedGold() + amount);
            PlayerPrefs.Save();
        }

        public static void ResetBankedWallet()
        {
            PlayerPrefs.DeleteKey($"{BankKeyPrefix}{CashRewardId}");
            PlayerPrefs.DeleteKey($"{BankKeyPrefix}{GoldRewardId}");
            PlayerPrefs.Save();
        }

        public static bool TrySpendBankedGold(int cost)
        {
            if (cost <= 0) return true;

            int currentGold = GetBankedGold();
            if (currentGold < cost) return false;

            PlayerPrefs.SetInt($"{BankKeyPrefix}{GoldRewardId}", currentGold - cost);
            PlayerPrefs.Save();
            return true;
        }
    }
}
