using System.Collections.Generic;
using UnityEngine;
using Vertigo.Core.Events;
using Vertigo.Core.Interfaces;
using Vertigo.Core.Save;

namespace Vertigo.Rewards.Controllers
{
    public class SessionInventoryManager : MonoBehaviour, ISessionInventory
    {
        public const string CashRewardId = SaveService.CashId;
        public const string GoldRewardId = SaveService.GoldId;

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

            if (!suppressInventoryUpdateEvent)
            {
                GameEvents.FireInventoryUpdated(_sessionItems);
            }
        }

        public void ClearSession()
        {
            _sessionItems.Clear();
            GameEvents.FireInventoryUpdated(_sessionItems);
        }

        public void BankRewards()
        {
            SaveService.BankRewards(_sessionItems);
            GameEvents.FireCashOutSuccess(new Dictionary<string, (IRewardData, int)>(_sessionItems));
            ClearSession();
        }

        public static int GetBankedAmount(string rewardId) => SaveService.GetAmount(rewardId);

        public static int GetBankedCash() => SaveService.GetAmount(CashRewardId);

        public static int GetBankedGold() => SaveService.GetAmount(GoldRewardId);

        public static void AddBankedGold(int amount) => SaveService.AddAmount(GoldRewardId, amount);

        public static void ResetBankedWallet() => SaveService.Reset();

        public static bool TrySpendBankedGold(int cost) => SaveService.TrySpendGold(cost);
    }
}
