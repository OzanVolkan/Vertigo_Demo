using System;
using System.Collections.Generic;
using Vertigo.Core.Enums;
using Vertigo.Core.Interfaces;
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
        public static event Action<int, float, float, float> OnSpinStarted; // targetSliceIndex, targetAngle, exactCenterAngle, duration
        public static event Action<WheelSliceData> OnSpinCompleted;
        public static event Action OnSliceTick; // Triggered when indicator hits a slice peg

        // Reward & Inventory Events
        public static event Action<IReadOnlyDictionary<string, (IRewardData reward, int amount)>> OnInventoryUpdated;

        // Bomb & Revive & Cash Out Events
        public static event Action OnBombTriggered;
        public static event Action OnReviveRequested;
        public static event Action OnReviveSuccess;
        public static event Action OnCashOutPromptRequested;
        public static event Action OnCashOutRequested;
        public static event Action<IReadOnlyDictionary<string, (IRewardData reward, int amount)>> OnCashOutSuccess;
        public static event Action OnGameRestartRequested;

        // Audio / SFX Events
        public static event Action<SoundEffectType, float> OnSfxRequested;

        // Dispatchers
        public static void FireGameStateChanged(GameState state) => OnGameStateChanged?.Invoke(state);
        public static void FireZoneChanged(ZoneData zone) => OnZoneChanged?.Invoke(zone);
        public static void FireSpinRequested() => OnSpinRequested?.Invoke();
        public static void FireSpinStarted(int targetIndex, float targetAngle, float exactCenterAngle, float duration) => OnSpinStarted?.Invoke(targetIndex, targetAngle, exactCenterAngle, duration);
        public static void FireSpinCompleted(WheelSliceData slice) => OnSpinCompleted?.Invoke(slice);
        public static void FireSliceTick() => OnSliceTick?.Invoke();
        public static void FireInventoryUpdated(IReadOnlyDictionary<string, (IRewardData reward, int amount)> inv) => OnInventoryUpdated?.Invoke(inv);
        public static void FireBombTriggered() => OnBombTriggered?.Invoke();
        public static void FireReviveRequested() => OnReviveRequested?.Invoke();
        public static void FireReviveSuccess() => OnReviveSuccess?.Invoke();
        public static void FireCashOutPromptRequested() => OnCashOutPromptRequested?.Invoke();
        public static void FireCashOutRequested() => OnCashOutRequested?.Invoke();
        public static void FireCashOutSuccess(IReadOnlyDictionary<string, (IRewardData reward, int amount)> banked) => OnCashOutSuccess?.Invoke(banked);
        public static void FireGameRestartRequested() => OnGameRestartRequested?.Invoke();
        public static void FireSfx(SoundEffectType sfxType, float pitchMultiplier = 1f) => OnSfxRequested?.Invoke(sfxType, pitchMultiplier);
    }
}
