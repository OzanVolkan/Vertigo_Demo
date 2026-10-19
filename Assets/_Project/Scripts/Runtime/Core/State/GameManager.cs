using System.Collections;
using DG.Tweening;
using UnityEngine;
using Vertigo.Audio.Controllers;
using Vertigo.Core.Enums;
using Vertigo.Core.Events;
using Vertigo.Rewards.Controllers;
using Vertigo.Rewards.Views;
using Vertigo.UI.Panels;
using Vertigo.Wheel.Controllers;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Views;
using Vertigo.Zone.Controllers;

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

            EnsureAudioManager();
            ResolveReferences();
            HideInactivePopups();
        }

        private void Start() => InitializeGame();

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
            zoneManager?.Initialize(1);
            SetState(GameState.Idle);
        }

        private void SetState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            Debug.Log($"[GameManager] State changed to: {newState}");
            GameEvents.FireGameStateChanged(newState);
        }

        private void HandleSpinStarted(int targetIndex, float targetAngle, float exactCenterAngle, float duration) =>
            SetState(GameState.Spinning);

        private void HandleSpinCompleted(WheelSliceData landedSlice)
        {
            SetState(GameState.Evaluating);

            if (landedSlice?.Reward == null)
            {
                Debug.LogError("[GameManager] Landed slice or reward is null!");
                SetState(GameState.Idle);
                return;
            }

            StartCoroutine(landedSlice.Reward.IsBomb ? TriggerBombRoutine() : AwardRewardRoutine(landedSlice));
        }

        private IEnumerator TriggerBombRoutine()
        {
            wheelView?.PlayBombImpactShake();
            yield return new WaitForSeconds(0.45f);

            SetState(GameState.BombTriggered);

            if (bombPopupView != null && !bombPopupView.gameObject.activeSelf)
                bombPopupView.gameObject.SetActive(true);

            GameEvents.FireBombTriggered();
        }

        private IEnumerator AwardRewardRoutine(WheelSliceData slice)
        {
            SetState(GameState.RewardAwarded);

            float multiplier = zoneManager?.CurrentZoneData != null ? zoneManager.CurrentZoneData.RewardMultiplier : 1f;
            int scaledAmount = Mathf.Max(1, Mathf.RoundToInt(slice.Amount * multiplier));
            int oldAmount = inventoryManager != null ? inventoryManager.GetAmount(slice.Reward.Id) : 0;
            int newAmount = oldAmount + scaledAmount;

            InventoryRewardSlotView targetSlot = inventoryView != null
                ? inventoryView.GetOrCreateSlot(slice.Reward, oldAmount)
                : null;

            bool suppressUpdate = targetSlot != null && rewardCollectEffect != null;
            inventoryManager?.AddReward(slice.Reward, scaledAmount, suppressInventoryUpdateEvent: suppressUpdate);

            WheelType wheelType = GetCurrentWheelType();

            yield return PlayShowcaseRoutine(slice, scaledAmount, wheelType);
            yield return ScrollToSlotIfNeededRoutine(targetSlot);
            yield return PlayCollectEffectRoutine(slice, targetSlot, oldAmount, newAmount, wheelType);
            yield return new WaitForSeconds(0.25f);
            yield return AdvanceOrCompleteRoutine();
        }

        private IEnumerator PlayShowcaseRoutine(WheelSliceData slice, int scaledAmount, WheelType wheelType)
        {
            if (rewardShowcase == null) yield break;

            bool finished = false;
            rewardShowcase.ShowReward(
                slice.Reward.Icon,
                slice.Reward.FormatAmount(scaledAmount),
                wheelType,
                () => finished = true);

            while (!finished)
                yield return null;
        }

        private IEnumerator ScrollToSlotIfNeededRoutine(InventoryRewardSlotView targetSlot)
        {
            if (inventoryView == null || targetSlot == null || inventoryView.IsSlotVisible(targetSlot))
                yield break;

            Tween scrollTween = inventoryView.ScrollToSlot(targetSlot, smooth: true, duration: 0.32f);
            if (scrollTween != null && scrollTween.IsActive())
                yield return scrollTween.WaitForCompletion();
            else
                yield return new WaitForSeconds(0.15f);
        }

        private IEnumerator PlayCollectEffectRoutine(
            WheelSliceData slice,
            InventoryRewardSlotView targetSlot,
            int oldAmount,
            int newAmount,
            WheelType wheelType)
        {
            if (rewardCollectEffect == null || targetSlot == null)
            {
                yield return new WaitForSeconds(rewardAwardDelay);
                yield break;
            }

            Vector3 startPos = wheelView != null ? wheelView.GetWinningSlicePosition() : transform.position;
            bool finished = false;
            rewardCollectEffect.PlayCollectEffect(
                slice.Reward.Icon,
                startPos,
                targetSlot,
                oldAmount,
                newAmount,
                wheelType,
                () => finished = true);

            while (!finished)
                yield return null;
        }

        private IEnumerator AdvanceOrCompleteRoutine()
        {
            if (zoneManager != null && zoneManager.IsFinalZone)
            {
                if (missionCompletePopupView != null)
                    missionCompletePopupView.Show(GameEvents.FireCashOutRequested);
                else
                    GameEvents.FireCashOutRequested();
                yield break;
            }

            SetState(GameState.ZoneTransition);
            zoneManager?.AdvanceToNextZone();
            yield return new WaitForSeconds(zoneTransitionDelay);
            SetState(GameState.Idle);
        }

        private WheelType GetCurrentWheelType()
        {
            if (wheelController?.CurrentConfig != null)
                return wheelController.CurrentConfig.WheelType;

            return (zoneManager?.CurrentZoneData?.ZoneType) switch
            {
                ZoneType.Safe => WheelType.SilverSafe,
                ZoneType.Super => WheelType.GoldenSuper,
                _ => WheelType.BronzeNormal
            };
        }

        private void HandleReviveRequested()
        {
            Debug.Log("[GameManager] Revive requested. Preserving rewards and continuing...");
            GameEvents.FireReviveSuccess();
            StartCoroutine(AdvanceOrCompleteRoutine());
        }

        public void JumpToZone(int targetZone)
        {
            StopAllCoroutines();
            HideAllPopups();

            wheelController?.ResetSpinState();
            wheelView?.ResetWheel();
            zoneManager?.JumpToZone(targetZone);
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
                    yield return null;
            }
            else
            {
                yield return new WaitForSeconds(0.25f);
            }

            zoneManager?.ResetToStart();
            SetState(GameState.Idle);
        }

        private void HandleRestartRequested()
        {
            SetState(GameState.GameOver);
            missionCompletePopupView?.HideImmediate();
            zoneManager?.ResetToStart();
            SetState(GameState.Idle);
        }

        private void EnsureAudioManager()
        {
            if (audioManager == null)
                audioManager = FindFirstObjectByType<AudioManager>(FindObjectsInactive.Include);

            if (audioManager != null) return;

            var audioObj = new GameObject("[AudioManager]");
            audioObj.transform.SetParent(transform, false);
            audioManager = audioObj.AddComponent<AudioManager>();
        }

        private void ResolveReferences()
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

        private void HideInactivePopups()
        {
            if (bombPopupView != null && !bombPopupView.gameObject.activeSelf) bombPopupView.HideImmediate();
            if (cashOutPopupView != null && !cashOutPopupView.gameObject.activeSelf) cashOutPopupView.HideImmediate();
            if (cashOutSummaryView != null && !cashOutSummaryView.gameObject.activeSelf) cashOutSummaryView.HideImmediate();
            if (missionCompletePopupView != null && !missionCompletePopupView.gameObject.activeSelf) missionCompletePopupView.HideImmediate();
        }

        private void HideAllPopups()
        {
            bombPopupView?.HideImmediate();
            cashOutPopupView?.HideImmediate();
            cashOutSummaryView?.HideImmediate();
            missionCompletePopupView?.HideImmediate();
            rewardShowcase?.HideImmediate();
        }

        private void OnValidate() => ResolveReferences();
    }
}
