using UnityEngine;
using Vertigo.Core.Enums;
using Vertigo.Core.Interfaces;

namespace Vertigo.Rewards.Data
{
    [CreateAssetMenu(fileName = "Reward_", menuName = "Vertigo/Rewards/Reward Data")]
    public class RewardDataSO : ScriptableObject, IRewardData
    {
        [Header("General Info")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField] private RewardType type;

        [Header("Display Options")]
        [SerializeField] private string amountPrefix = "x";
        [SerializeField] private bool useAbbreviatedAmount = true;
        [Tooltip("If true, card uses prestigious golden frame and flare in summary; otherwise sleek silver frame.")]
        [SerializeField] private bool isValuable;

        public string Id => id;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public RewardType Type => type;
        public bool IsBomb => type == RewardType.Bomb;
        public bool IsValuable => isValuable || type == RewardType.Weapon || (id != null && (id.Contains("Gold") || id.Contains("Super")));

        public string FormatAmount(int amount)
        {
            if (IsBomb) return string.Empty;
            if (amount <= 0) return string.Empty;

            if (useAbbreviatedAmount)
            {
                if (amount >= 1_000_000)
                {
                    float val = amount / 1_000_000f;
                    return $"{amountPrefix}{val:0.#}M";
                }
                if (amount >= 1_000)
                {
                    float val = amount / 1_000f;
                    return $"{amountPrefix}{val:0.#}K";
                }
            }

            return $"{amountPrefix}{amount:N0}";
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = name;
            }
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = name.Replace("Reward_", "").Replace("_", " ");
            }
        }
    }
}
