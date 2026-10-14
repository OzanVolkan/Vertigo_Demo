using System.Collections;
using UnityEngine;
using Vertigo.Core.Enums;
using Vertigo.Core.Events;
using Vertigo.Rewards.Controllers;
using Vertigo.Rewards.Data;
using Vertigo.Wheel.Controllers;
using Vertigo.Wheel.Data;
using Vertigo.Zone.Controllers;
using Vertigo.Zone.Data;

namespace Vertigo.Core.State
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Sub-Managers")]
        [SerializeField] private ZoneManager zoneManager;
        [SerializeField] private WheelController wheelController;
        [SerializeField] private SessionInventoryManager inventoryManager;

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.Initialize;

        [Header("Timing Settings")]
        [SerializeField] private float rewardAwardDelay = 1.2f;
        [SerializeField] private float zoneTransitionDelay = 0.8f;

        public GameState CurrentState => currentState;
        public ZoneManager ZoneManager => zoneManager;
        public WheelController WheelController => wheelController;
        public SessionInventoryManager InventoryManager => inventoryManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeGame();
        }

        private void OnEnable()
        {
            GameEvents.OnSpinStarted += HandleSpinStarted;
            GameEvents.OnSpinCompleted += HandleSpinCompleted;
            GameEvents.OnReviveRequested += HandleReviveRequested;
            GameEvents.OnCashOutRequested += HandleCashOutRequested;
            GameEvents.OnGameRestartRequested += HandleRestartRequested;
        }

        private void OnDisable()
        {
            GameEvents.OnSpinStarted -= HandleSpinStarted;
            GameEvents.OnSpinCompleted -= HandleSpinCompleted;
            GameEvents.OnReviveRequested -= HandleReviveRequested;
            GameEvents.OnCashOutRequested -= HandleCashOutRequested;
            GameEvents.OnGameRestartRequested -= HandleRestartRequested;
        }

        public void InitializeGame()
        {
            SetState(GameState.Initialize);

            if (zoneManager != null)
            {
                zoneManager.Initialize(1);
            }

            SetState(GameState.Idle);
        }

        private void SetState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            Debug.Log($"[GameManager] State changed to: {newState}");
            GameEvents.FireGameStateChanged(newState);
        }

        private void HandleSpinStarted(int targetIndex, float targetAngle, float duration)
        {
            SetState(GameState.Spinning);
        }

        private void HandleSpinCompleted(WheelSliceData landedSlice)
        {
            SetState(GameState.Evaluating);

            if (landedSlice == null || landedSlice.Reward == null)
            {
                Debug.LogError("[GameManager] Landed slice or reward is null!");
                SetState(GameState.Idle);
                return;
            }

            if (landedSlice.Reward.IsBomb)
            {
                StartCoroutine(TriggerBombRoutine());
            }
            else
            {
                StartCoroutine(AwardRewardRoutine(landedSlice));
            }
        }

        private IEnumerator TriggerBombRoutine()
        {
            yield return new WaitForSeconds(0.4f);
            SetState(GameState.BombTriggered);
            GameEvents.FireBombTriggered();
        }

        private IEnumerator AwardRewardRoutine(WheelSliceData slice)
        {
            SetState(GameState.RewardAwarded);

            // Calculate scaled reward amount based on zone multiplier
            float multiplier = zoneManager != null ? zoneManager.CurrentZoneData.RewardMultiplier : 1f;
            int scaledAmount = Mathf.Max(1, Mathf.RoundToInt(slice.Amount * multiplier));

            if (inventoryManager != null)
            {
                inventoryManager.AddReward(slice.Reward, scaledAmount);
            }

            yield return new WaitForSeconds(rewardAwardDelay);

            // Advance to next zone
            SetState(GameState.ZoneTransition);
            if (zoneManager != null)
            {
                zoneManager.AdvanceToNextZone();
            }

            yield return new WaitForSeconds(zoneTransitionDelay);
            SetState(GameState.Idle);
        }

        private void HandleReviveRequested()
        {
            Debug.Log("[GameManager] Revive requested. Preserving rewards and continuing...");
            GameEvents.FireReviveSuccess();

            // After reviving, advance to next zone
            StartCoroutine(PostReviveRoutine());
        }

        private IEnumerator PostReviveRoutine()
        {
            SetState(GameState.ZoneTransition);
            if (zoneManager != null)
            {
                zoneManager.AdvanceToNextZone();
            }
            yield return new WaitForSeconds(zoneTransitionDelay);
            SetState(GameState.Idle);
        }

        private void HandleCashOutRequested()
        {
            SetState(GameState.CashOut);
            StartCoroutine(PostCashOutRoutine());
        }

        private IEnumerator PostCashOutRoutine()
        {
            yield return new WaitForSeconds(1.5f);
            if (zoneManager != null)
            {
                zoneManager.ResetToStart();
            }
            SetState(GameState.Idle);
        }

        private void HandleRestartRequested()
        {
            SetState(GameState.GameOver);
            if (zoneManager != null)
            {
                zoneManager.ResetToStart();
            }
            SetState(GameState.Idle);
        }

        private void OnValidate()
        {
            if (zoneManager == null) zoneManager = GetComponentInChildren<ZoneManager>();
            if (wheelController == null) wheelController = GetComponentInChildren<WheelController>();
            if (inventoryManager == null) inventoryManager = GetComponentInChildren<SessionInventoryManager>();
        }
    }
}
