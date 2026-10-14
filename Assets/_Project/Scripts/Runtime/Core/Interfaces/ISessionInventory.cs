using System.Collections.Generic;
using Vertigo.Core.Interfaces;

namespace Vertigo.Core.Interfaces
{
    public interface ISessionInventory
    {
        IReadOnlyDictionary<string, (IRewardData reward, int amount)> Items { get; }
        int TotalItemCount { get; }
        void AddReward(IRewardData reward, int amount);
        void ClearSession();
        void BankRewards();
    }
}
