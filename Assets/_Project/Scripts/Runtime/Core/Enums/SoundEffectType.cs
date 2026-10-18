namespace Vertigo.Core.Enums
{
    /// <summary>
    /// Identifiers for all sound effects in the game.
    /// Indexed sequentially starting at 0 for O(1) flat-array lookup in AudioManager without boxing or GC allocations.
    /// </summary>
    public enum SoundEffectType
    {
        None = 0,

        // UI & General
        ButtonClick = 1,

        // Wheel & Spin
        WheelSpinStart = 2,
        WheelTick = 3,
        WheelStopReward = 4,
        WheelStopBomb = 5,

        // Reward Showcase & Inventory Collection
        RewardShowcasePop = 6,
        RewardParticleBurst = 7,
        RewardSlotHit = 8,

        // Zone Progression
        ZoneAdvance = 9,
        SafeZoneEnter = 10,
        SuperZoneEnter = 11,

        // Bomb & Revive
        BombPopupOpen = 12,
        ReviveSuccess = 13,
        GiveUp = 14,

        // Cash Out & Mission Complete
        CashOutPopupOpen = 15,
        ChestAppear = 16,
        CardRevealSilver = 17,
        CardRevealGold = 18,
        CashOutSummaryComplete = 19,
        MissionCompleteVictory = 20
    }
}
