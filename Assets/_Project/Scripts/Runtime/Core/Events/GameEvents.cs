using System;
using System.Collections.Generic;
using Vertigo.Core.Enums;
using Vertigo.Core.Interfaces;
using Vertigo.Rewards.Data;
using Vertigo.Wheel.Data;
using Vertigo.Zone.Data;

namespace Vertigo.Core.Events
{
    public static class GameEvents
    {
        // Game State
        public static event Action<GameState> OnGameStateChanged;

        // Zone Events
        public static event Action<ZoneData> OnZoneChanged;

        // Wheel & Spin Events
        public static event Action OnSpinRequested;
        public static event Action<int, float, float> OnSpinStarted; // targetSliceIndex, targetAngle, duration
        public static event Action<WheelSliceData> OnSpinCompleted;
        public static event Action OnSliceTick; // Triggered when indicator hits a slice peg

        // Reward & Inventory Events
        public static event Action<IRewardData, int> OnRewardCollected;
        public static event Action<IReadOnlyDictionary<string, (IRewardData reward, int amount)>> OnInventoryUpdated;
        public static event Action OnInventoryCleared;

        // Bomb & Revive & Cash Out Events
        public static event Action OnBombTriggered;
        public static event Action OnReviveRequested;
        public static event Action OnReviveSuccess;
        public static event Action OnCashOutRequested;
        public static event Action<IReadOnlyDictionary<string, (IRewardData reward, int amount)>> OnCashOutSuccess;
        public static event Action OnGameRestartRequested;

        // Dispatchers
        public static void FireGameStateChanged(GameState state) => OnGameStateChanged?.Invoke(state);
        public static void FireZoneChanged(ZoneData zone) => OnZoneChanged?.Invoke(zone);
        public static void FireSpinRequested() => OnSpinRequested?.Invoke();
        public static void FireSpinStarted(int targetIndex, float targetAngle, float duration) => OnSpinStarted?.Invoke(targetIndex, targetAngle, duration);
        public static void FireSpinCompleted(WheelSliceData slice) => OnSpinCompleted?.Invoke(slice);
        public static void FireSliceTick() => OnSliceTick?.Invoke();
        public static void FireRewardCollected(IRewardData reward, int amount) => OnRewardCollected?.Invoke(reward, amount);
        public static void FireInventoryUpdated(IReadOnlyDictionary<string, (IRewardData reward, int amount)> inv) => OnInventoryUpdated?.Invoke(inv);
        public static void FireInventoryCleared() => OnInventoryCleared?.Invoke();
        public static void FireBombTriggered() => OnBombTriggered?.Invoke();
        public static void FireReviveRequested() => OnReviveRequested?.Invoke();
        public static void FireReviveSuccess() => OnReviveSuccess?.Invoke();
        public static void FireCashOutRequested() => OnCashOutRequested?.Invoke();
        public static void FireCashOutSuccess(IReadOnlyDictionary<string, (IRewardData reward, int amount)> banked) => OnCashOutSuccess?.Invoke(banked);
        public static void FireGameRestartRequested() => OnGameRestartRequested?.Invoke();

        public static void ClearAllListeners()
        {
            OnGameStateChanged = null;
            OnZoneChanged = null;
            OnSpinRequested = null;
            OnSpinStarted = null;
            OnSpinCompleted = null;
            OnSliceTick = null;
            OnRewardCollected = null;
            OnInventoryUpdated = null;
            OnInventoryCleared = null;
            OnBombTriggered = null;
            OnReviveRequested = null;
            OnReviveSuccess = null;
            OnCashOutRequested = null;
            OnCashOutSuccess = null;
            OnGameRestartRequested = null;
        }
    }
}
