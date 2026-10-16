using UnityEngine;
using Vertigo.Core.Enums;

namespace Vertigo.Core.Interfaces
{
    public interface IRewardData
    {
        string Id { get; }
        string DisplayName { get; }
        Sprite Icon { get; }
        RewardType Type { get; }
        bool IsBomb { get; }
        bool IsValuable { get; }
        string FormatAmount(int amount);
    }
}
