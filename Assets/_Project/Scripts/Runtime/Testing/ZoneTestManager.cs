using System.Collections.Generic;
using UnityEngine;
using Vertigo.Core.Enums;
using Vertigo.Core.State;
using Vertigo.Rewards.Controllers;
using Vertigo.Rewards.Data;
using Vertigo.Rewards.Views;
using Vertigo.Wheel.Controllers;
using Vertigo.Zone.Controllers;
using Vertigo.Zone.Data;

namespace Vertigo.Testing
{
    public class ZoneTestManager : MonoBehaviour
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
        [SerializeField] private RewardDataSO[] availableRewards;

        public int TargetZone
        {
            get => targetZone;
            set => targetZone = Mathf.Max(1, value);
        }

        public bool DisableBomb
        {
            get => disableBomb;
            set
            {
                disableBomb = value;
                if (disableBomb) forceBomb = false;
                ApplyBombSetting();
            }
        }

        public bool ForceBomb
        {
            get => forceBomb;
            set
            {
                forceBomb = value;
                if (forceBomb) disableBomb = false;
                ApplyBombSetting();
            }
        }

        public bool ClearInventoryOnJump
        {
            get => clearInventoryOnJump;
            set => clearInventoryOnJump = value;
        }

        private void Awake()
        {
            EnsureDependencies();
            ApplyBombSetting();
        }

        private void Update()
        {
            ApplyBombSetting();
        }

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

#if UNITY_EDITOR
            if (availableRewards == null || availableRewards.Length == 0)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:RewardDataSO");
                var list = new List<RewardDataSO>();
                foreach (var g in guids)
                {
                    string p = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                    var r = UnityEditor.AssetDatabase.LoadAssetAtPath<RewardDataSO>(p);
                    if (r != null && !r.IsBomb) list.Add(r);
                }
                availableRewards = list.ToArray();
            }
#endif
        }

        public void ApplyBombSetting()
        {
            if (wheelController != null)
            {
                wheelController.DisableBombChance = disableBomb;
                wheelController.ForceBombChance = forceBomb;
            }
        }

        [ContextMenu("Jump To Target Zone")]
        public void JumpToTargetZone()
        {
            JumpToZone(targetZone);
        }

        public void JumpToZone(int zoneNumber)
        {
            targetZone = Mathf.Max(1, zoneNumber);
            EnsureDependencies();

            if (clearInventoryOnJump && gameManager != null && gameManager.InventoryManager != null)
            {
                gameManager.InventoryManager.ClearSession();
            }

            if (gameManager != null)
            {
                gameManager.JumpToZone(targetZone);
            }
            else if (zoneManager != null)
            {
                zoneManager.JumpToZone(targetZone);
            }
            else
            {
                Debug.LogWarning("[ZoneTestManager] No ZoneManager or GameManager found in scene!");
            }
        }

        [ContextMenu("Next Zone (+1)")]
        public void NextZone()
        {
            int current = GetCurrentZone();
            JumpToZone(current + 1);
        }

        [ContextMenu("Previous Zone (-1)")]
        public void PreviousZone()
        {
            int current = GetCurrentZone();
            JumpToZone(Mathf.Max(1, current - 1));
        }

        [ContextMenu("Jump: Zone 1 (Normal Start)")]
        public void JumpToZone1() => JumpToZone(1);

        [ContextMenu("Jump: Zone 5 (Safe Zone - Silver)")]
        public void JumpToZone5() => JumpToZone(5);

        [ContextMenu("Jump: Zone 10 (Safe Zone - Silver)")]
        public void JumpToZone10() => JumpToZone(10);

        [ContextMenu("Jump: Zone 15 (Safe Zone - Silver)")]
        public void JumpToZone15() => JumpToZone(15);

        [ContextMenu("Jump: Zone 20 (Safe Zone - Silver)")]
        public void JumpToZone20() => JumpToZone(20);

        [ContextMenu("Jump: Zone 25 (Safe Zone - Silver)")]
        public void JumpToZone25() => JumpToZone(25);

        [ContextMenu("Jump: Zone 30 (Super Zone - Golden)")]
        public void JumpToZone30() => JumpToZone(30);

        #region Inventory & Scrolling Test API

        [ContextMenu("Inventory: Add Random Reward")]
        public void AddRandomTestReward()
        {
            EnsureDependencies();
            if (inventoryManager == null)
            {
                Debug.LogWarning("[ZoneTestManager] No SessionInventoryManager found in scene!");
                return;
            }

            if (availableRewards != null && availableRewards.Length > 0)
            {
                var randomReward = availableRewards[Random.Range(0, availableRewards.Length)];
                int amount = Random.Range(10, 250);
                inventoryManager.AddReward(randomReward, amount);
            }
            else
            {
                var dummy = ScriptableObject.CreateInstance<RewardDataSO>();
                dummy.name = "Test_Reward_" + Random.Range(1, 9999);
                inventoryManager.AddReward(dummy, Random.Range(10, 500));
            }
        }

        [ContextMenu("Inventory: Fill (12 Items) - Test Scroll Overflow")]
        public void FillInventoryForScrollTest(int count = 12)
        {
            EnsureDependencies();
            if (inventoryManager == null)
            {
                Debug.LogWarning("[ZoneTestManager] No SessionInventoryManager found in scene!");
                return;
            }

            if (availableRewards != null && availableRewards.Length > 0)
            {
                int toAdd = Mathf.Min(count, availableRewards.Length);
                for (int i = 0; i < toAdd; i++)
                {
                    inventoryManager.AddReward(availableRewards[i], (i + 1) * 20);
                }

                for (int i = toAdd; i < count; i++)
                {
                    var dummy = ScriptableObject.CreateInstance<RewardDataSO>();
                    dummy.name = $"Bonus_Reward_{i + 1}";
                    inventoryManager.AddReward(dummy, (i + 1) * 35);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    var dummy = ScriptableObject.CreateInstance<RewardDataSO>();
                    dummy.name = $"Reward_{i + 1}";
                    inventoryManager.AddReward(dummy, (i + 1) * 25);
                }
            }

            Debug.Log($"[ZoneTestManager] Filled inventory with {count} items for scroll testing.");
        }

        [ContextMenu("Inventory: Clear All")]
        public void ClearInventory()
        {
            EnsureDependencies();
            if (inventoryManager != null)
            {
                inventoryManager.ClearSession();
            }
        }

        [ContextMenu("Inventory: Scroll To Top")]
        public void ScrollToTop()
        {
            EnsureDependencies();
            if (inventoryView != null)
            {
                inventoryView.ScrollToTop();
            }
        }

        [ContextMenu("Inventory: Scroll To Bottom")]
        public void ScrollToBottom()
        {
            EnsureDependencies();
            if (inventoryView != null)
            {
                inventoryView.ScrollToBottom();
            }
        }

        #endregion

        public int GetCurrentZone()
        {
            if (zoneManager != null) return zoneManager.CurrentZoneNumber;
            return targetZone;
        }

        public ZoneType GetCurrentZoneType()
        {
            if (zoneManager != null && zoneManager.CurrentZoneData != null)
                return zoneManager.CurrentZoneData.ZoneType;
            return ZoneType.Normal;
        }

        public float GetCurrentMultiplier()
        {
            if (zoneManager != null && zoneManager.CurrentZoneData != null)
                return zoneManager.CurrentZoneData.RewardMultiplier;
            return 1f;
        }
    }
}
