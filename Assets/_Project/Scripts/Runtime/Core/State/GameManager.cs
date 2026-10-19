using System.Collections;
using DG.Tweening;
using UnityEngine;
using Vertigo.Audio.Controllers;
using Vertigo.Core.Enums;
using Vertigo.Core.Events;
using Vertigo.Rewards.Controllers;
using Vertigo.Rewards.Data;
using Vertigo.Rewards.Views;
using Vertigo.UI.Panels;
using Vertigo.Wheel.Controllers;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Views;
using Vertigo.Zone.Controllers;
using Vertigo.Zone.Data;

namespace Vertigo.Core.State
{
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        [Header("Sub-Managers")]
        [SerializeField] private ZoneManager zoneManager;
        [SerializeField] private WheelController wheelController;
        [SerializeField] private SessionInventoryManager inventoryManager;
        [SerializeField] private AudioManager audioManager;

        [Header("Views")]
        [SerializeField] private WheelView wheelView;
        [SerializeField] private InventoryView inventoryView;
        [SerializeField] private RewardCollectEffectView rewardCollectEffect;
        [SerializeField] private RewardShowcaseView rewardShowcase;
        [SerializeField] private BombPopupView bombPopupView;
        [SerializeField] private CashOutPopupView cashOutPopupView;
        [SerializeField] private CashOutRewardSummaryView cashOutSummaryView;
        [SerializeField] private MissionCompletePopupView missionCompletePopupView;

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.Initialize;

        [Header("Timing Settings")]
        [SerializeField] private float rewardAwardDelay = 1.2f;
        [SerializeField] private float zoneTransitionDelay = 0.8f;

        public GameState CurrentState => currentState;
        public ZoneManager ZoneManager => zoneManager;
        public WheelController WheelController => wheelController;
        public SessionInventoryManager InventoryManager => inventoryManager;
        public AudioManager AudioManager => audioManager;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            if (audioManager == null) audioManager = FindFirstObjectByType<AudioManager>(FindObjectsInactive.Include);
            if (audioManager == null)
            {
                GameObject audioObj = new GameObject("[AudioManager]");
                audioObj.transform.SetParent(transform, false);
                audioManager = audioObj.AddComponent<AudioManager>();
            }
            if (wheelView == null) wheelView = FindFirstObjectByType<WheelView>();
            if (inventoryView == null) inventoryView = FindFirstObjectByType<InventoryView>();
            if (rewardCollectEffect == null) rewardCollectEffect = FindFirstObjectByType<RewardCollectEffectView>();
            if (rewardShowcase == null) rewardShowcase = FindFirstObjectByType<RewardShowcaseView>(FindObjectsInactive.Include);
            if (bombPopupView == null) bombPopupView = FindFirstObjectByType<BombPopupView>(FindObjectsInactive.Include);
            if (cashOutPopupView == null) cashOutPopupView = FindFirstObjectByType<CashOutPopupView>(FindObjectsInactive.Include);
            if (cashOutSummaryView == null) cashOutSummaryView = FindFirstObjectByType<CashOutRewardSummaryView>(FindObjectsInactive.Include);
            if (missionCompletePopupView == null) missionCompletePopupView = FindFirstObjectByType<MissionCompletePopupView>(FindObjectsInactive.Include);

            if (bombPopupView != null && !bombPopupView.gameObject.activeSelf)
            {
                bombPopupView.HideImmediate();
            }

            if (cashOutPopupView != null && !cashOutPopupView.gameObject.activeSelf)
            {
                cashOutPopupView.HideImmediate();
            }

            if (cashOutSummaryView != null && !cashOutSummaryView.gameObject.activeSelf)
            {
                cashOutSummaryView.HideImmediate();
            }

            if (missionCompletePopupView != null && !missionCompletePopupView.gameObject.activeSelf)
            {
                missionCompletePopupView.HideImmediate();
            }
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

        private void HandleSpinStarted(int targetIndex, float targetAngle, float exactCenterAngle, float duration)
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
            if (wheelView != null)
            {
                wheelView.PlayBombImpactShake();
            }

            yield return new WaitForSeconds(0.45f);
            SetState(GameState.BombTriggered);

            if (bombPopupView == null)
            {
                bombPopupView = FindFirstObjectByType<BombPopupView>(FindObjectsInactive.Include);
            }
            if (bombPopupView != null && !bombPopupView.gameObject.activeSelf)
            {
                bombPopupView.gameObject.SetActive(true);
            }

            GameEvents.FireBombTriggered();
        }

        private IEnumerator AwardRewardRoutine(WheelSliceData slice)
        {
            SetState(GameState.RewardAwarded);

            // Calculate scaled reward amount based on zone multiplier
            float multiplier = zoneManager != null ? zoneManager.CurrentZoneData.RewardMultiplier : 1f;
            int scaledAmount = Mathf.Max(1, Mathf.RoundToInt(slice.Amount * multiplier));

            int oldAmount = inventoryManager != null ? inventoryManager.GetAmount(slice.Reward.Id) : 0;
            int newAmount = oldAmount + scaledAmount;

            InventoryRewardSlotView targetSlot = null;
            if (inventoryView != null)
            {
                targetSlot = inventoryView.GetOrCreateSlot(slice.Reward, oldAmount);
            }

            // Suppress immediate inventory update event so slot text won't instantly snap before the animation
            bool suppressUpdate = (targetSlot != null && rewardCollectEffect != null);
            if (inventoryManager != null)
            {
                inventoryManager.AddReward(slice.Reward, scaledAmount, suppressInventoryUpdateEvent: suppressUpdate);
            }

            WheelType currentWheelType = WheelType.BronzeNormal;
            if (wheelController != null && wheelController.CurrentConfig != null)
            {
                currentWheelType = wheelController.CurrentConfig.WheelType;
            }
            else if (zoneManager != null && zoneManager.CurrentZoneData != null)
            {
                switch (zoneManager.CurrentZoneData.ZoneType)
                {
                    case ZoneType.Safe:
                        currentWheelType = WheelType.SilverSafe;
                        break;
                    case ZoneType.Super:
                        currentWheelType = WheelType.GoldenSuper;
                        break;
                    default:
                        currentWheelType = WheelType.BronzeNormal;
                        break;
                }
            }

            if (rewardShowcase == null)
            {
                rewardShowcase = FindFirstObjectByType<RewardShowcaseView>(FindObjectsInactive.Include);
            }

            // 1. Showcase the won reward prominently on screen (Polish)
            if (rewardShowcase != null)
            {
                bool showcaseFinished = false;
                string formattedAmount = slice.Reward.FormatAmount(scaledAmount);
                rewardShowcase.ShowReward(
                    slice.Reward.Icon,
                    formattedAmount,
                    currentWheelType,
                    () => showcaseFinished = true
                );

                while (!showcaseFinished)
                {
                    yield return null;
                }
            }

            // 2. Normal flow: If the target slot is outside the visible scrollview viewport, smoothly scroll to bring it into view!
            if (inventoryView != null && targetSlot != null && !inventoryView.IsSlotVisible(targetSlot))
            {
                Tween scrollTween = inventoryView.ScrollToSlot(targetSlot, smooth: true, duration: 0.32f);
                if (scrollTween != null && scrollTween.IsActive())
                {
                    yield return scrollTween.WaitForCompletion();
                }
                else
                {
                    yield return new WaitForSeconds(0.15f);
                }
            }

            Vector3 startPos = wheelView != null ? wheelView.GetWinningSlicePosition() : transform.position;

            if (rewardCollectEffect != null && targetSlot != null)
            {
                bool effectFinished = false;
                rewardCollectEffect.PlayCollectEffect(
                    slice.Reward.Icon,
                    startPos,
                    targetSlot,
                    oldAmount,
                    newAmount,
                    currentWheelType,
                    () => effectFinished = true
                );

                while (!effectFinished)
                {
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(rewardAwardDelay);
            }

            // Short breath after collection before zone advances
            yield return new WaitForSeconds(0.25f);

            // Final Zone (Zone 60) completed: show mission complete celebration panel first, then cash out!
            if (zoneManager != null && zoneManager.IsFinalZone)
            {
                Debug.Log($"[GameManager] Final Zone ({zoneManager.CurrentZoneNumber}) completed! Showing Mission Complete celebration panel.");
                if (missionCompletePopupView != null)
                {
                    missionCompletePopupView.Show(() =>
                    {
                        GameEvents.FireCashOutRequested();
                    });
                }
                else
                {
                    GameEvents.FireCashOutRequested();
                }
                yield break;
            }

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
            if (zoneManager != null && zoneManager.IsFinalZone)
            {
                if (missionCompletePopupView != null)
                {
                    missionCompletePopupView.Show(() =>
                    {
                        GameEvents.FireCashOutRequested();
                    });
                }
                else
                {
                    GameEvents.FireCashOutRequested();
                }
                yield break;
            }

            SetState(GameState.ZoneTransition);
            if (zoneManager != null)
            {
                zoneManager.AdvanceToNextZone();
            }
            yield return new WaitForSeconds(zoneTransitionDelay);
            SetState(GameState.Idle);
        }

        public void JumpToZone(int targetZone)
        {
            StopAllCoroutines();

            // Dismiss any active popups and effects cleanly
            if (bombPopupView != null) bombPopupView.HideImmediate();
            if (cashOutPopupView != null) cashOutPopupView.HideImmediate();
            if (cashOutSummaryView != null) cashOutSummaryView.HideImmediate();
            if (missionCompletePopupView != null) missionCompletePopupView.HideImmediate();
            if (rewardShowcase != null) rewardShowcase.HideImmediate();

            // Reset wheel if it was spinning
            if (wheelController != null) wheelController.ResetSpinState();
            if (wheelView != null) wheelView.ResetWheel();

            if (zoneManager != null)
            {
                zoneManager.JumpToZone(targetZone);
            }
            SetState(GameState.Idle);
        }

        private void HandleCashOutRequested()
        {
            SetState(GameState.CashOut);
            StartCoroutine(PostCashOutRoutine());
        }

        private IEnumerator PostCashOutRoutine()
        {
            if (cashOutSummaryView != null)
            {
                yield return new WaitForSeconds(0.05f);
                while (cashOutSummaryView.IsPresenting)
                {
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(0.25f);
            }

            if (zoneManager != null)
            {
                zoneManager.ResetToStart();
            }
            SetState(GameState.Idle);
        }

        private void HandleRestartRequested()
        {
            SetState(GameState.GameOver);
            if (missionCompletePopupView != null)
            {
                missionCompletePopupView.HideImmediate();
            }
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
            if (wheelView == null) wheelView = FindFirstObjectByType<WheelView>();
            if (inventoryView == null) inventoryView = FindFirstObjectByType<InventoryView>();
            if (rewardCollectEffect == null) rewardCollectEffect = FindFirstObjectByType<RewardCollectEffectView>();
            if (rewardShowcase == null) rewardShowcase = FindFirstObjectByType<RewardShowcaseView>(FindObjectsInactive.Include);
            if (bombPopupView == null) bombPopupView = FindFirstObjectByType<BombPopupView>(FindObjectsInactive.Include);
            if (cashOutPopupView == null) cashOutPopupView = FindFirstObjectByType<CashOutPopupView>(FindObjectsInactive.Include);
            if (cashOutSummaryView == null) cashOutSummaryView = FindFirstObjectByType<CashOutRewardSummaryView>(FindObjectsInactive.Include);
            if (missionCompletePopupView == null) missionCompletePopupView = FindFirstObjectByType<MissionCompletePopupView>(FindObjectsInactive.Include);
        }
    }
}
