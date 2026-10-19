using System.Collections.Generic;

namespace Vertigo.Core.Interfaces
{
    public interface ISessionInventory
    {
        IReadOnlyDictionary<string, (IRewardData reward, int amount)> Items { get; }
        void AddReward(IRewardData reward, int amount, bool suppressInventoryUpdateEvent = false);
        void ClearSession();
        void BankRewards();
    }
}
