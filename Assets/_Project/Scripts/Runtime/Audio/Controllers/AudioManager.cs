using UnityEngine;
using Vertigo.Audio.Data;
using Vertigo.Core.Enums;
using Vertigo.Core.Events;
using Vertigo.Wheel.Data;
using Vertigo.Zone.Data;

namespace Vertigo.Audio.Controllers
{
    /// <summary>
    /// High-performance, zero-allocation Sound Effect Manager.
    /// Features:
    /// - Direct Inspector AudioClip slots grouped by category for instant drag-and-drop replacement.
    /// - Optional ScriptableObject (AudioConfigSO) fallback/preset support.
    /// - Pre-warmed ring-buffer AudioSource pool (zero runtime AddComponent / Instantiate / GC allocations).
    /// - O(1) flat array lookup indexed by SoundEffectType (zero Dictionary hashing or enum boxing).
    /// - Per-sound unscaled cooldown throttling to prevent audio clipping on rapid wheel ticks or particle bursts.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private const int SfxTypeCount = 21; // Covers SoundEffectType.None (0) through MissionCompleteVictory (20)

        [Header("General & Pre-Placed Scene Channels")]
        [Tooltip("Optional ScriptableObject preset config. Direct Inspector clip fields below take priority when assigned.")]
        [SerializeField] private AudioConfigSO audioConfig;
        [Range(0f, 1f)]
        [SerializeField] private float masterSfxVolume = 1f;
        [SerializeField] private bool isMuted = false;
        [Tooltip("Dedicated pre-placed AudioSource exclusively for WheelSpinStart so it can never be interrupted by WheelTick or other SFX.")]
        [SerializeField] private AudioSource dedicatedWheelSpinSource;
        [Tooltip("Pre-placed 2D AudioSources serialized in the scene (zero runtime GameObject/Component creation overhead).")]
        [SerializeField] private AudioSource[] sfxSources;
        [Range(4, 24)]
        [Tooltip("Fallback pool size if sfxSources is not pre-populated in Edit Mode.")]
        [SerializeField] private int poolSize = 10;

        [Header("1. UI Clips (Inspector Assignable)")]
        [Tooltip("General UI button click sound (Spin, Cash Out, Continue, Skip, etc.)")]
        [SerializeField] private AudioClip buttonClickClip;

        [Header("2. Wheel & Spin Clips (Inspector Assignable)")]
        [Tooltip("Played when the wheel starts spinning")]
        [SerializeField] private AudioClip wheelSpinStartClip;
        [Tooltip("Played each time the wheel indicator crosses a slice boundary")]
        [SerializeField] private AudioClip wheelTickClip;
        [Tooltip("Played when the wheel stops on a reward slice")]
        [SerializeField] private AudioClip wheelStopRewardClip;
        [Tooltip("Played when the wheel stops on a bomb slice (explosion impact)")]
        [SerializeField] private AudioClip wheelStopBombClip;

        [Header("3. Reward & Inventory Clips (Inspector Assignable)")]
        [Tooltip("Played when the won reward pops up in the center showcase")]
        [SerializeField] private AudioClip rewardShowcasePopClip;
        [Tooltip("Played when flying reward particles burst outward")]
        [SerializeField] private AudioClip rewardParticleBurstClip;
        [Tooltip("Played when flying reward particles hit the left inventory slot and count up")]
        [SerializeField] private AudioClip rewardSlotHitClip;

        [Header("4. Zone Progression Clips (Inspector Assignable)")]
        [Tooltip("Played when advancing to a normal zone")]
        [SerializeField] private AudioClip zoneAdvanceClip;
        [Tooltip("Played when entering a Safe Zone (Silver tier)")]
        [SerializeField] private AudioClip safeZoneEnterClip;
        [Tooltip("Played when entering a Super Zone (Gold tier)")]
        [SerializeField] private AudioClip superZoneEnterClip;

        [Header("5. Bomb & Revive Clips (Inspector Assignable)")]
        [Tooltip("Played when the Bomb popup screen opens")]
        [SerializeField] private AudioClip bombPopupOpenClip;
        [Tooltip("Played when the player revives (Gold or Ad)")]
        [SerializeField] private AudioClip reviveSuccessClip;
        [Tooltip("Played when the player clicks Give Up")]
        [SerializeField] private AudioClip giveUpClip;

        [Header("6. Cash Out & Victory Clips (Inspector Assignable)")]
        [Tooltip("Played when the Cash Out confirmation dialog opens")]
        [SerializeField] private AudioClip cashOutPopupOpenClip;
        [Tooltip("Played when the reward chest appears in the Cash Out summary screen")]
        [SerializeField] private AudioClip chestAppearClip;
        [Tooltip("Played when a standard (Silver) reward card flips out of the chest")]
        [SerializeField] private AudioClip cardRevealSilverClip;
        [Tooltip("Played when a valuable (Gold) reward card flips out of the chest")]
        [SerializeField] private AudioClip cardRevealGoldClip;
        [Tooltip("Played when the final 'YOU GOT:' summary grid is presented")]
        [SerializeField] private AudioClip cashOutSummaryCompleteClip;
        [Tooltip("Played when Zone 60 is completed and the Mission Complete celebration panel appears")]
        [SerializeField] private AudioClip missionCompleteVictoryClip;

        // Pre-allocated O(1) flat lookup tables (Zero GC)
        private readonly SoundEffectEntry[] _sfxTable = new SoundEffectEntry[SfxTypeCount];
        private readonly float[] _lastPlayedTime = new float[SfxTypeCount];
        private readonly bool[] _channelHighPriority = new bool[24];

        // Pre-warmed AudioSource pool
        private AudioSource[] _sourcePool;
        private int _poolCursor;
        private int _lastKnownZoneNumber = -1;
        private int _consecutiveWheelTicks;

        public bool IsMuted
        {
            get => isMuted;
            set => isMuted = value;
        }

        public float MasterSfxVolume
        {
            get => masterSfxVolume;
            set => masterSfxVolume = Mathf.Clamp01(value);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

#if UNITY_EDITOR
            EnsurePlaceholderClipsInEditor();
#endif

            EnsureAudioListener();
            InitializePool();
            RebuildLookupTable();
        }

        private void EnsureAudioListener()
        {
            AudioListener.pause = false;
            if (AudioListener.volume <= 0.01f)
            {
                AudioListener.volume = 1f;
            }

            AudioListener listener = FindFirstObjectByType<AudioListener>(FindObjectsInactive.Include);
            if (listener == null)
            {
                Camera cam = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
                GameObject targetObj = cam != null ? cam.gameObject : gameObject;
                listener = targetObj.AddComponent<AudioListener>();
            }
            else if (!listener.enabled)
            {
                listener.enabled = true;
            }
        }

        private void OnEnable()
        {
            // Core GameEvents hooks
            GameEvents.OnSfxRequested += HandleSfxRequested;
            GameEvents.OnSpinStarted += HandleSpinStarted;
            GameEvents.OnSliceTick += HandleSliceTick;
            GameEvents.OnSpinCompleted += HandleSpinCompleted;
            GameEvents.OnZoneChanged += HandleZoneChanged;
            GameEvents.OnBombTriggered += HandleBombTriggered;
            GameEvents.OnReviveSuccess += HandleReviveSuccess;
            GameEvents.OnCashOutPromptRequested += HandleCashOutPromptRequested;
        }

        private void OnDisable()
        {
            GameEvents.OnSfxRequested -= HandleSfxRequested;
            GameEvents.OnSpinStarted -= HandleSpinStarted;
            GameEvents.OnSliceTick -= HandleSliceTick;
            GameEvents.OnSpinCompleted -= HandleSpinCompleted;
            GameEvents.OnZoneChanged -= HandleZoneChanged;
            GameEvents.OnBombTriggered -= HandleBombTriggered;
            GameEvents.OnReviveSuccess -= HandleReviveSuccess;
            GameEvents.OnCashOutPromptRequested -= HandleCashOutPromptRequested;
        }

        private void InitializePool()
        {
            // 1. Ensure Dedicated Wheel Spin AudioSource is ready (uses pre-placed scene reference if present)
            if (dedicatedWheelSpinSource == null)
            {
                Transform spinTr = transform.Find("AudioSource_WheelSpin");
                if (spinTr != null)
                {
                    dedicatedWheelSpinSource = spinTr.GetComponent<AudioSource>();
                }
                if (dedicatedWheelSpinSource == null)
                {
                    GameObject spinObj = new GameObject("AudioSource_WheelSpin");
                    spinObj.transform.SetParent(transform, false);
                    dedicatedWheelSpinSource = spinObj.AddComponent<AudioSource>();
                }
            }
            Configure2DSource(dedicatedWheelSpinSource);

            // 2. Use pre-placed serialized sfxSources if already populated in the scene (Zero Awake creation cost!)
            if (sfxSources != null && sfxSources.Length > 0)
            {
                bool allValid = true;
                for (int i = 0; i < sfxSources.Length; i++)
                {
                    if (sfxSources[i] == null)
                    {
                        allValid = false;
                        break;
                    }
                    Configure2DSource(sfxSources[i]);
                }

                if (allValid)
                {
                    _sourcePool = sfxSources;
                    return;
                }
            }

            // Fallback only if scene wasn't pre-populated in Edit Mode
            int count = Mathf.Clamp(poolSize, 4, 24);
            _sourcePool = new AudioSource[count];

            Transform poolRoot = transform.Find("AudioSource_Pool");
            if (poolRoot == null)
            {
                GameObject rootObj = new GameObject("AudioSource_Pool");
                rootObj.transform.SetParent(transform, false);
                poolRoot = rootObj.transform;
            }

            for (int i = 0; i < count; i++)
            {
                Transform child = i < poolRoot.childCount ? poolRoot.GetChild(i) : null;
                GameObject srcObj = child != null ? child.gameObject : new GameObject($"SFX_Channel_{i:D2}");
                if (child == null)
                {
                    srcObj.transform.SetParent(poolRoot, false);
                }

                AudioSource source = srcObj.GetComponent<AudioSource>();
                if (source == null)
                {
                    source = srcObj.AddComponent<AudioSource>();
                }

                Configure2DSource(source);
                _sourcePool[i] = source;
            }

            sfxSources = _sourcePool;
        }

        private static void Configure2DSource(AudioSource source)
        {
            if (source == null) return;
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.reverbZoneMix = 0f;
        }

        /// <summary>
        /// Builds the flat O(1) lookup array combining Inspector clip assignments with default/config parameters.
        /// Can be called in Editor or runtime if clips are swapped dynamically.
        /// </summary>
        public void RebuildLookupTable()
        {
            for (int i = 0; i < SfxTypeCount; i++)
            {
                _lastPlayedTime[i] = -999f;
            }

            // Default tuned parameters for each SoundEffectType (overridden by AudioConfigSO if present, while Inspector clip always takes priority)
            RegisterEntry(SoundEffectType.ButtonClick, buttonClickClip, 0.75f, 0.97f, 1.03f, 0.04f, false);

            RegisterEntry(SoundEffectType.WheelSpinStart, wheelSpinStartClip, 0.90f, 0.99f, 1.01f, 0.05f, true);
            RegisterEntry(SoundEffectType.WheelTick, wheelTickClip, 0.65f, 0.95f, 1.05f, 0.032f, false);
            RegisterEntry(SoundEffectType.WheelStopReward, wheelStopRewardClip, 0.85f, 0.98f, 1.02f, 0.10f, true);
            RegisterEntry(SoundEffectType.WheelStopBomb, wheelStopBombClip, 0.95f, 0.96f, 1.02f, 0.15f, true);

            RegisterEntry(SoundEffectType.RewardShowcasePop, rewardShowcasePopClip, 0.85f, 0.98f, 1.03f, 0.10f, true);
            RegisterEntry(SoundEffectType.RewardParticleBurst, rewardParticleBurstClip, 0.75f, 0.96f, 1.04f, 0.08f, false);
            RegisterEntry(SoundEffectType.RewardSlotHit, rewardSlotHitClip, 0.80f, 0.98f, 1.06f, 0.045f, false);

            RegisterEntry(SoundEffectType.ZoneAdvance, zoneAdvanceClip, 0.75f, 0.98f, 1.02f, 0.10f, false);
            RegisterEntry(SoundEffectType.SafeZoneEnter, safeZoneEnterClip, 0.90f, 0.99f, 1.01f, 0.15f, true);
            RegisterEntry(SoundEffectType.SuperZoneEnter, superZoneEnterClip, 0.95f, 0.99f, 1.01f, 0.15f, true);

            RegisterEntry(SoundEffectType.BombPopupOpen, bombPopupOpenClip, 0.85f, 0.98f, 1.02f, 0.15f, true);
            RegisterEntry(SoundEffectType.ReviveSuccess, reviveSuccessClip, 0.90f, 0.99f, 1.02f, 0.15f, true);
            RegisterEntry(SoundEffectType.GiveUp, giveUpClip, 0.80f, 0.98f, 1.02f, 0.15f, true);

            RegisterEntry(SoundEffectType.CashOutPopupOpen, cashOutPopupOpenClip, 0.80f, 0.98f, 1.02f, 0.10f, false);
            RegisterEntry(SoundEffectType.ChestAppear, chestAppearClip, 0.85f, 0.98f, 1.02f, 0.10f, true);
            RegisterEntry(SoundEffectType.CardRevealSilver, cardRevealSilverClip, 0.85f, 0.97f, 1.03f, 0.06f, false);
            RegisterEntry(SoundEffectType.CardRevealGold, cardRevealGoldClip, 0.95f, 0.98f, 1.02f, 0.06f, true);
            RegisterEntry(SoundEffectType.CashOutSummaryComplete, cashOutSummaryCompleteClip, 0.90f, 0.99f, 1.01f, 0.15f, true);
            RegisterEntry(SoundEffectType.MissionCompleteVictory, missionCompleteVictoryClip, 1.00f, 1.00f, 1.00f, 0.25f, true);
        }

        private void RegisterEntry(
            SoundEffectType type,
            AudioClip inspectorClip,
            float defaultVolume,
            float minPitch,
            float maxPitch,
            float minInterval,
            bool highPriority)
        {
            int index = (int)type;
            if (index <= 0 || index >= SfxTypeCount) return;

            if (audioConfig != null && audioConfig.TryGetEntry(type, out SoundEffectEntry configEntry))
            {
                if (inspectorClip != null)
                {
                    configEntry.clip = inspectorClip;
                }
                _sfxTable[index] = configEntry;
                return;
            }

            _sfxTable[index] = SoundEffectEntry.CreateDefault(
                type,
                inspectorClip,
                defaultVolume,
                minPitch,
                maxPitch,
                minInterval,
                highPriority
            );
        }

        /// <summary>
        /// Plays a sound effect by type with zero GC allocation and automatic throttling.
        /// </summary>
        public void PlaySfx(SoundEffectType type, float pitchMultiplier = 1f)
        {
            if (isMuted) return;

            int index = (int)type;
            if (index <= 0 || index >= SfxTypeCount) return;

            SoundEffectEntry entry = _sfxTable[index];

            // Fallback check in case the user dragged a new clip into the Inspector at runtime in Play Mode
            AudioClip clipToPlay = GetDirectInspectorClip(type);
            if (clipToPlay == null)
            {
                clipToPlay = entry.clip;
            }
            if (clipToPlay == null) return;

            float now = Time.unscaledTime;
            if (now - _lastPlayedTime[index] < entry.minInterval)
            {
                return; // Throttled to prevent audio stacking/clipping
            }

            _lastPlayedTime[index] = now;

            // WheelSpinStart always uses its own dedicated AudioSource so rapid WheelTicks or other SFX never cut it off
            AudioSource source = (type == SoundEffectType.WheelSpinStart && dedicatedWheelSpinSource != null)
                ? dedicatedWheelSpinSource
                : AcquirePooledSource(entry.highPriority);

            if (source == null) return;

            float configMaster = audioConfig != null ? audioConfig.MasterSfxVolume : 1f;
            float finalVolume = Mathf.Clamp01(entry.volume * masterSfxVolume * configMaster);

            float randomPitch = entry.pitchRange.x < entry.pitchRange.y
                ? Random.Range(entry.pitchRange.x, entry.pitchRange.y)
                : entry.pitchRange.x;
            if (randomPitch <= 0.01f) randomPitch = 1f;

            source.pitch = Mathf.Clamp(randomPitch * pitchMultiplier, 0.5f, 2.0f);
            source.volume = finalVolume;
            source.clip = clipToPlay;
            source.Play();
        }

        private AudioSource AcquirePooledSource(bool isHighPriority)
        {
            if (_sourcePool == null || _sourcePool.Length == 0) return null;

            int len = _sourcePool.Length;

            // 1st pass: Find a completely idle AudioSource
            for (int i = 0; i < len; i++)
            {
                int idx = (_poolCursor + i) % len;
                AudioSource candidate = _sourcePool[idx];
                if (candidate != null && !candidate.isPlaying)
                {
                    _poolCursor = (idx + 1) % len;
                    _channelHighPriority[idx] = isHighPriority;
                    return candidate;
                }
            }

            // 2nd pass: Voice stealing — prefer stealing a non-high-priority channel
            for (int i = 0; i < len; i++)
            {
                int idx = (_poolCursor + i) % len;
                if (!_channelHighPriority[idx])
                {
                    AudioSource candidate = _sourcePool[idx];
                    if (candidate != null)
                    {
                        _poolCursor = (idx + 1) % len;
                        _channelHighPriority[idx] = isHighPriority;
                        return candidate;
                    }
                }
            }

            // Fallback: Steal oldest only if incoming sound is high priority
            if (isHighPriority)
            {
                int idx = _poolCursor;
                _poolCursor = (_poolCursor + 1) % len;
                _channelHighPriority[idx] = true;
                return _sourcePool[idx];
            }

            return null;
        }

        private AudioClip GetDirectInspectorClip(SoundEffectType type)
        {
            switch (type)
            {
                case SoundEffectType.ButtonClick: return buttonClickClip;
                case SoundEffectType.WheelSpinStart: return wheelSpinStartClip;
                case SoundEffectType.WheelTick: return wheelTickClip;
                case SoundEffectType.WheelStopReward: return wheelStopRewardClip;
                case SoundEffectType.WheelStopBomb: return wheelStopBombClip;
                case SoundEffectType.RewardShowcasePop: return rewardShowcasePopClip;
                case SoundEffectType.RewardParticleBurst: return rewardParticleBurstClip;
                case SoundEffectType.RewardSlotHit: return rewardSlotHitClip;
                case SoundEffectType.ZoneAdvance: return zoneAdvanceClip;
                case SoundEffectType.SafeZoneEnter: return safeZoneEnterClip;
                case SoundEffectType.SuperZoneEnter: return superZoneEnterClip;
                case SoundEffectType.BombPopupOpen: return bombPopupOpenClip;
                case SoundEffectType.ReviveSuccess: return reviveSuccessClip;
                case SoundEffectType.GiveUp: return giveUpClip;
                case SoundEffectType.CashOutPopupOpen: return cashOutPopupOpenClip;
                case SoundEffectType.ChestAppear: return chestAppearClip;
                case SoundEffectType.CardRevealSilver: return cardRevealSilverClip;
                case SoundEffectType.CardRevealGold: return cardRevealGoldClip;
                case SoundEffectType.CashOutSummaryComplete: return cashOutSummaryCompleteClip;
                case SoundEffectType.MissionCompleteVictory: return missionCompleteVictoryClip;
                default: return null;
            }
        }

        // =====================================================================
        // Event Listeners
        // =====================================================================

        private void HandleSfxRequested(SoundEffectType type, float pitchMultiplier)
        {
            PlaySfx(type, pitchMultiplier);
        }

        private void HandleSpinStarted(int targetSliceIndex, float targetAngle, float exactCenterAngle, float duration)
        {
            _consecutiveWheelTicks = 0;
            PlaySfx(SoundEffectType.WheelSpinStart);
        }

        private void HandleSliceTick()
        {
            _consecutiveWheelTicks++;
            // Subtle progressive pitch rise on ticks capped smoothly so wheel ticks feel lively
            float tickPitchMod = Mathf.Lerp(0.95f, 1.08f, Mathf.Clamp01(_consecutiveWheelTicks / 28f));
            PlaySfx(SoundEffectType.WheelTick, tickPitchMod);
        }

        private void HandleSpinCompleted(WheelSliceData slice)
        {
            if (slice == null || slice.Reward == null) return;

            if (slice.Reward.IsBomb)
            {
                PlaySfx(SoundEffectType.WheelStopBomb);
            }
            else
            {
                PlaySfx(SoundEffectType.WheelStopReward);
            }
        }

        private void HandleZoneChanged(ZoneData zone)
        {
            if (zone == null) return;

            // Avoid playing zone transition sound on initial startup load of Zone 1
            if (_lastKnownZoneNumber < 0)
            {
                _lastKnownZoneNumber = zone.ZoneNumber;
                return;
            }

            // If resetting back to Zone 1 after CashOut or GameOver, just update state quietly
            if (zone.ZoneNumber == 1 && _lastKnownZoneNumber > 1)
            {
                _lastKnownZoneNumber = zone.ZoneNumber;
                return;
            }

            _lastKnownZoneNumber = zone.ZoneNumber;

            switch (zone.ZoneType)
            {
                case ZoneType.Super:
                    PlaySfx(SoundEffectType.SuperZoneEnter);
                    break;
                case ZoneType.Safe:
                    PlaySfx(SoundEffectType.SafeZoneEnter);
                    break;
                default:
                    PlaySfx(SoundEffectType.ZoneAdvance);
                    break;
            }
        }

        private void HandleBombTriggered()
        {
            PlaySfx(SoundEffectType.BombPopupOpen);
        }

        private void HandleReviveSuccess()
        {
            PlaySfx(SoundEffectType.ReviveSuccess);
        }

        private void HandleCashOutPromptRequested()
        {
            PlaySfx(SoundEffectType.CashOutPopupOpen);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsurePlaceholderClipsInEditor();
            if (Application.isPlaying)
            {
                RebuildLookupTable();
            }
        }

        private void EnsurePlaceholderClipsInEditor()
        {
            const string folder = "Assets/_Project/Audio/Placeholders/";
            if (audioConfig == null)
                audioConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioConfigSO>("Assets/_Project/Data/Audio/AudioConfig_Default.asset");

            if (buttonClickClip == null) buttonClickClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_button_click.wav");
            if (wheelSpinStartClip == null) wheelSpinStartClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_wheel_spin_start.wav");
            if (wheelTickClip == null) wheelTickClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_wheel_tick.wav");
            if (wheelStopRewardClip == null) wheelStopRewardClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_wheel_stop_reward.wav");
            if (wheelStopBombClip == null) wheelStopBombClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_wheel_stop_bomb.wav");
            if (rewardShowcasePopClip == null) rewardShowcasePopClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_reward_showcase_pop.wav");
            if (rewardParticleBurstClip == null) rewardParticleBurstClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_reward_particle_burst.wav");
            if (rewardSlotHitClip == null) rewardSlotHitClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_reward_slot_hit.wav");
            if (zoneAdvanceClip == null) zoneAdvanceClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_zone_advance.wav");
            if (safeZoneEnterClip == null) safeZoneEnterClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_safe_zone_enter.wav");
            if (superZoneEnterClip == null) superZoneEnterClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_super_zone_enter.wav");
            if (bombPopupOpenClip == null) bombPopupOpenClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_bomb_popup_open.wav");
            if (reviveSuccessClip == null) reviveSuccessClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_revive_success.wav");
            if (giveUpClip == null) giveUpClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_give_up.wav");
            if (cashOutPopupOpenClip == null) cashOutPopupOpenClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_cashout_popup_open.wav");
            if (chestAppearClip == null) chestAppearClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_chest_appear.wav");
            if (cardRevealSilverClip == null) cardRevealSilverClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_card_reveal_silver.wav");
            if (cardRevealGoldClip == null) cardRevealGoldClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_card_reveal_gold.wav");
            if (cashOutSummaryCompleteClip == null) cashOutSummaryCompleteClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_cashout_summary_complete.wav");
            if (missionCompleteVictoryClip == null) missionCompleteVictoryClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "sfx_mission_complete_victory.wav");
        }
#endif
    }
}
