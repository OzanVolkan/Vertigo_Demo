using System.Collections.Generic;
using UnityEngine;
using Vertigo.Core.Enums;
using Vertigo.Core.State;
using Vertigo.Rewards.Controllers;
using Vertigo.Rewards.Data;
using Vertigo.Rewards.Views;
using Vertigo.Wheel.Controllers;
using Vertigo.Zone.Controllers;
using Vertigo.Zone.Views;

namespace Vertigo.Testing
{
    public class TestManager : MonoBehaviour
    {
        [Header("Target Zone Selection")]
        [Tooltip("The zone you wish to jump to (1 to 60)")]
        [Range(1, 60)]
        [SerializeField] private int targetZone = 1;

        [Header("Cheat / Test Flags")]
        [Tooltip("If checked, bomb probability is 0% (spins will NEVER land on a bomb)")]
        [SerializeField] private bool disableBomb = false;
        [Tooltip("If checked, spins on normal wheels will ALWAYS land on the bomb (100% chance)")]
        [SerializeField] private bool forceBomb = false;

        [Header("Options")]
        [Tooltip("If true, resets session inventory items when jumping to a new zone")]
        [SerializeField] private bool clearInventoryOnJump = false;

        [Header("Dependencies (Auto-found if null)")]
        [SerializeField] private ZoneManager zoneManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private WheelController wheelController;
        [SerializeField] private SessionInventoryManager inventoryManager;
        [SerializeField] private InventoryView inventoryView;
        [SerializeField] private ZoneBarView zoneBarView;
        [SerializeField] private RewardDataSO[] availableRewards;

        private void Awake()
        {
            EnsureDependencies();
            ApplyBombSetting();
        }

        private void Update() => ApplyBombSetting();

        private void OnValidate()
        {
            EnsureDependencies();
            ApplyBombSetting();
        }

        public void EnsureDependencies()
        {
            if (zoneManager == null) zoneManager = FindFirstObjectByType<ZoneManager>();
            if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
            if (wheelController == null) wheelController = FindFirstObjectByType<WheelController>();
            if (inventoryManager == null) inventoryManager = FindFirstObjectByType<SessionInventoryManager>();
            if (inventoryView == null) inventoryView = FindFirstObjectByType<InventoryView>();
            if (zoneBarView == null) zoneBarView = FindFirstObjectByType<ZoneBarView>();

#if UNITY_EDITOR
            if (availableRewards == null || availableRewards.Length == 0)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:RewardDataSO");
                var list = new List<RewardDataSO>();
                foreach (string g in guids)
                {
                    var r = UnityEditor.AssetDatabase.LoadAssetAtPath<RewardDataSO>(UnityEditor.AssetDatabase.GUIDToAssetPath(g));
                    if (r != null && !r.IsBomb) list.Add(r);
                }
                availableRewards = list.ToArray();
            }
#endif
        }

        public void ApplyBombSetting()
        {
            if (wheelController == null) return;
            wheelController.DisableBombChance = disableBomb;
            wheelController.ForceBombChance = forceBomb;
        }

        // =====================================================================
        // Zone Navigation
        // =====================================================================

        [ContextMenu("Zone: Jump To Target Zone")]
        public void JumpToTargetZone() => JumpToZone(targetZone);

        public void JumpToZone(int zoneNumber)
        {
            targetZone = Mathf.Clamp(zoneNumber, 1, 60);
            EnsureDependencies();

            if (clearInventoryOnJump && gameManager != null && gameManager.InventoryManager != null)
            {
                gameManager.InventoryManager.ClearSession();
            }

            if (gameManager != null) gameManager.JumpToZone(targetZone);
            else if (zoneManager != null) zoneManager.JumpToZone(targetZone);

            if (zoneBarView != null) zoneBarView.SnapToZone(targetZone);
        }

        [ContextMenu("Zone: Next Zone (+1)")]
        public void NextZone() => JumpToZone(GetCurrentZone() + 1);

        [ContextMenu("Zone: Previous Zone (-1)")]
        public void PreviousZone() => JumpToZone(Mathf.Max(1, GetCurrentZone() - 1));

        // =====================================================================
        // Wallet (Banked Gold & Cash) Test API
        // =====================================================================

        [ContextMenu("Wallet: Add 100 Banked Gold")]
        public void Add100BankedGold() => AddBankedGold(100);

        public void AddBankedGold(int amount)
        {
            SessionInventoryManager.AddBankedGold(amount);
            Debug.Log($"[TestManager] Added {amount} Banked Gold. Total Gold: {SessionInventoryManager.GetBankedGold():N0}");
        }

        [ContextMenu("Wallet: Reset Banked Cash & Gold")]
        public void ResetWallet()
        {
            SessionInventoryManager.ResetBankedWallet();
            Debug.Log("[TestManager] Reset Banked Cash & Gold to 0.");
        }

        // =====================================================================
        // Inventory & Scroll Test API
        // =====================================================================

        [ContextMenu("Inventory: Add Random Reward")]
        public void AddRandomTestReward()
        {
            EnsureDependencies();
            if (inventoryManager == null) return;

            if (availableRewards != null && availableRewards.Length > 0)
            {
                RewardDataSO randomReward = availableRewards[Random.Range(0, availableRewards.Length)];
                inventoryManager.AddReward(randomReward, Random.Range(10, 250));
            }
            else
            {
                RewardDataSO dummy = ScriptableObject.CreateInstance<RewardDataSO>();
                dummy.name = "Test_Reward_" + Random.Range(1, 9999);
                inventoryManager.AddReward(dummy, Random.Range(10, 500));
            }
        }

        [ContextMenu("Inventory: Fill (12 Items) - Test Scroll Overflow")]
        public void FillInventoryForScrollTest(int count = 12)
        {
            EnsureDependencies();
            if (inventoryManager == null) return;

            int availableCount = availableRewards != null ? availableRewards.Length : 0;
            for (int i = 0; i < count; i++)
            {
                if (i < availableCount)
                {
                    inventoryManager.AddReward(availableRewards[i], (i + 1) * 20);
                }
                else
                {
                    RewardDataSO dummy = ScriptableObject.CreateInstance<RewardDataSO>();
                    dummy.name = $"Bonus_Reward_{i + 1}";
                    inventoryManager.AddReward(dummy, (i + 1) * 35);
                }
            }
        }

        [ContextMenu("Inventory: Clear All")]
        public void ClearInventory()
        {
            EnsureDependencies();
            if (inventoryManager != null) inventoryManager.ClearSession();
        }

        [ContextMenu("Inventory: Scroll To Top")]
        public void ScrollToTop()
        {
            EnsureDependencies();
            if (inventoryView != null) inventoryView.ScrollToTop();
        }

        [ContextMenu("Inventory: Scroll To Bottom")]
        public void ScrollToBottom()
        {
            EnsureDependencies();
            if (inventoryView != null) inventoryView.ScrollToBottom();
        }

        public int GetCurrentZone() => zoneManager != null ? zoneManager.CurrentZoneNumber : targetZone;

        public ZoneType GetCurrentZoneType() =>
            zoneManager != null && zoneManager.CurrentZoneData != null ? zoneManager.CurrentZoneData.ZoneType : ZoneType.Normal;

        public float GetCurrentMultiplier() =>
            zoneManager != null && zoneManager.CurrentZoneData != null ? zoneManager.CurrentZoneData.RewardMultiplier : 1f;
    }
}
