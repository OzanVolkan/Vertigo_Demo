using System;
using System.Collections.Generic;

namespace Vertigo.Core.Save
{
    [Serializable]
    public class PlayerSaveData
    {
        public const int DefaultStartingGold = 100;

        public int gold = DefaultStartingGold;
        public int cash;
        public List<RewardSaveEntry> items = new();
    }

    [Serializable]
    public struct RewardSaveEntry
    {
        public string id;
        public int amount;

        public RewardSaveEntry(string id, int amount)
        {
            this.id = id;
            this.amount = amount;
        }
    }
}
