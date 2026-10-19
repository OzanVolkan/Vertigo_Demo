using UnityEngine;
using Vertigo.Audio.Data;
using Vertigo.Core;
using Vertigo.Core.Enums;
using Vertigo.Core.Events;
using Vertigo.Wheel.Data;
using Vertigo.Zone.Data;

namespace Vertigo.Audio.Controllers
{
    public class AudioManager : MonoBehaviourSingleton<AudioManager>
    {
        private const int SfxTypeCount = 21;

        [Header("General & Pre-Placed Scene Channels")]
        [SerializeField] private AudioConfigSO audioConfig;
        [Range(0f, 1f)]
        [SerializeField] private float masterSfxVolume = 1f;
        [SerializeField] private bool isMuted = false;
        [SerializeField] private AudioSource dedicatedWheelSpinSource;
        [SerializeField] private AudioSource[] sfxSources;

        [Header("1. UI Clips")]
        [SerializeField] private AudioClip buttonClickClip;

        [Header("2. Wheel & Spin Clips")]
        [SerializeField] private AudioClip wheelSpinStartClip;
        [SerializeField] private AudioClip wheelTickClip;
        [SerializeField] private AudioClip wheelStopBombClip;

        [Header("3. Reward & Inventory Clips")]
        [SerializeField] private AudioClip rewardShowcasePopClip;
        [SerializeField] private AudioClip rewardParticleBurstClip;
        [SerializeField] private AudioClip rewardSlotHitClip;

        [Header("4. Zone Progression Clips")]
        [SerializeField] private AudioClip zoneAdvanceClip;
        [SerializeField] private AudioClip safeZoneEnterClip;
        [SerializeField] private AudioClip superZoneEnterClip;

        [Header("5. Bomb & Revive Clips")]
        [SerializeField] private AudioClip bombPopupOpenClip;
        [SerializeField] private AudioClip reviveSuccessClip;
        [SerializeField] private AudioClip giveUpClip;

        [Header("6. Cash Out & Victory Clips")]
        [SerializeField] private AudioClip cashOutPopupOpenClip;
        [SerializeField] private AudioClip chestAppearClip;
        [SerializeField] private AudioClip cardRevealSilverClip;
        [SerializeField] private AudioClip cardRevealGoldClip;
        [SerializeField] private AudioClip cashOutSummaryCompleteClip;
        [SerializeField] private AudioClip missionCompleteVictoryClip;

        private readonly SoundEffectEntry[] _sfxTable = new SoundEffectEntry[SfxTypeCount];
        private readonly float[] _lastPlayedTime = new float[SfxTypeCount];
        private readonly AudioSource[] _lastUsedSourceByType = new AudioSource[SfxTypeCount];
        private readonly bool[] _channelHighPriority = new bool[24];

        private int _poolCursor;
        private int _lastKnownZoneNumber = -1;
        private int _consecutiveWheelTicks;
        private bool _suppressNextZoneSfx;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            InitializeSources();
            InitializeLookupTable();
        }

        private void OnEnable()
        {
            GameEvents.OnSfxRequested += HandleSfxRequested;
            GameEvents.OnSpinStarted += HandleSpinStarted;
            GameEvents.OnSliceTick += HandleSliceTick;
            GameEvents.OnSpinCompleted += HandleSpinCompleted;
            GameEvents.OnZoneChanged += HandleZoneChanged;
            GameEvents.OnBombTriggered += HandleBombTriggered;
            GameEvents.OnReviveSuccess += HandleReviveSuccess;
            GameEvents.OnGameRestartRequested += StopBombSounds;
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
            GameEvents.OnGameRestartRequested -= StopBombSounds;
            GameEvents.OnCashOutPromptRequested -= HandleCashOutPromptRequested;
        }

        private void InitializeSources()
        {
            Configure2DSource(dedicatedWheelSpinSource);
            if (sfxSources == null) return;

            for (int i = 0; i < sfxSources.Length; i++)
            {
                Configure2DSource(sfxSources[i]);
            }
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

        public void InitializeLookupTable()
        {
            for (int i = 0; i < SfxTypeCount; i++)
            {
                _lastPlayedTime[i] = -999f;
                _sfxTable[i] = SoundEffectEntry.CreateDefault((SoundEffectType)i);
            }

            if (audioConfig != null && audioConfig.Entries != null)
            {
                foreach (SoundEffectEntry entry in audioConfig.Entries)
                {
                    int index = (int)entry.type;
                    if (index > 0 && index < SfxTypeCount)
                    {
                        _sfxTable[index] = entry;
                    }
                }
            }

            for (int i = 1; i < SfxTypeCount; i++)
            {
                AudioClip inspectorClip = GetDirectInspectorClip((SoundEffectType)i);
                if (inspectorClip != null)
                {
                    _sfxTable[i].clip = inspectorClip;
                }
            }
        }

        public void PlaySfx(SoundEffectType type, float pitchMultiplier = 1f)
        {
            int index = (int)type;
            if (isMuted || index <= 0 || index >= SfxTypeCount) return;

            SoundEffectEntry entry = _sfxTable[index];
            AudioClip clip = entry.clip != null ? entry.clip : GetDirectInspectorClip(type);
            if (clip == null || Time.unscaledTime - _lastPlayedTime[index] < entry.minInterval) return;

            AudioSource source = (type == SoundEffectType.WheelSpinStart && dedicatedWheelSpinSource != null)
                ? dedicatedWheelSpinSource
                : AcquireSource(entry.highPriority);

            if (source == null) return;

            _lastPlayedTime[index] = Time.unscaledTime;
            _lastUsedSourceByType[index] = source;

            float configVolume = audioConfig != null ? audioConfig.MasterSfxVolume : 1f;
            float basePitch = Random.Range(entry.pitchRange.x, entry.pitchRange.y);

            source.clip = clip;
            source.volume = Mathf.Clamp01(entry.volume * masterSfxVolume * configVolume);
            source.pitch = Mathf.Clamp((basePitch > 0.01f ? basePitch : 1f) * pitchMultiplier, 0.5f, 2f);
            source.Play();
        }

        public void StopSfx(SoundEffectType type)
        {
            int index = (int)type;
            if (index <= 0 || index >= SfxTypeCount) return;

            AudioSource src = _lastUsedSourceByType[index];
            if (src != null && src.isPlaying)
            {
                src.Stop();
            }
            _lastUsedSourceByType[index] = null;
        }

        private AudioSource AcquireSource(bool isHighPriority)
        {
            if (sfxSources == null || sfxSources.Length == 0) return null;
            int len = sfxSources.Length;

            for (int i = 0; i < len; i++)
            {
                int idx = (_poolCursor + i) % len;
                if (sfxSources[idx] != null && !sfxSources[idx].isPlaying)
                    return SelectChannel(idx, isHighPriority);
            }

            for (int i = 0; i < len; i++)
            {
                int idx = (_poolCursor + i) % len;
                if (!_channelHighPriority[idx] && sfxSources[idx] != null)
                    return SelectChannel(idx, isHighPriority);
            }

            return isHighPriority ? SelectChannel(_poolCursor, true) : null;
        }

        private AudioSource SelectChannel(int index, bool isHighPriority)
        {
            _poolCursor = (index + 1) % sfxSources.Length;
            _channelHighPriority[index] = isHighPriority;
            return sfxSources[index];
        }

        private AudioClip GetDirectInspectorClip(SoundEffectType type) => type switch
        {
            SoundEffectType.ButtonClick => buttonClickClip,
            SoundEffectType.WheelSpinStart => wheelSpinStartClip,
            SoundEffectType.WheelTick => wheelTickClip,
            SoundEffectType.WheelStopBomb => wheelStopBombClip,
            SoundEffectType.RewardShowcasePop => rewardShowcasePopClip,
            SoundEffectType.RewardParticleBurst => rewardParticleBurstClip,
            SoundEffectType.RewardSlotHit => rewardSlotHitClip,
            SoundEffectType.ZoneAdvance => zoneAdvanceClip,
            SoundEffectType.SafeZoneEnter => safeZoneEnterClip,
            SoundEffectType.SuperZoneEnter => superZoneEnterClip,
            SoundEffectType.BombPopupOpen => bombPopupOpenClip,
            SoundEffectType.ReviveSuccess => reviveSuccessClip,
            SoundEffectType.GiveUp => giveUpClip,
            SoundEffectType.CashOutPopupOpen => cashOutPopupOpenClip,
            SoundEffectType.ChestAppear => chestAppearClip,
            SoundEffectType.CardRevealSilver => cardRevealSilverClip,
            SoundEffectType.CardRevealGold => cardRevealGoldClip,
            SoundEffectType.CashOutSummaryComplete => cashOutSummaryCompleteClip,
            SoundEffectType.MissionCompleteVictory => missionCompleteVictoryClip,
            _ => null
        };

        // =====================================================================
        // Event Listeners
        // =====================================================================

        private void HandleSfxRequested(SoundEffectType type, float pitchMultiplier)
        {
            if (type == SoundEffectType.GiveUp) StopBombSounds();
            PlaySfx(type, pitchMultiplier);
        }

        private void HandleSpinStarted(int targetSliceIndex, float targetAngle, float exactCenterAngle, float duration)
        {
            _consecutiveWheelTicks = 0;
            StopSfx(SoundEffectType.BombPopupOpen);
            PlaySfx(SoundEffectType.WheelSpinStart);
        }

        private void HandleSliceTick()
        {
            _consecutiveWheelTicks++;
            float tickPitchMod = Mathf.Lerp(0.95f, 1.08f, Mathf.Clamp01(_consecutiveWheelTicks / 28f));
            PlaySfx(SoundEffectType.WheelTick, tickPitchMod);
        }

        private void HandleSpinCompleted(WheelSliceData slice)
        {
            if (dedicatedWheelSpinSource != null && dedicatedWheelSpinSource.isPlaying)
            {
                dedicatedWheelSpinSource.Stop();
            }

            if (slice?.Reward != null && slice.Reward.IsBomb)
            {
                PlaySfx(SoundEffectType.WheelStopBomb);
            }
        }

        private void HandleZoneChanged(ZoneData zone)
        {
            if (zone == null) return;

            bool isInitialOrReset = _lastKnownZoneNumber < 0 || (zone.ZoneNumber == 1 && _lastKnownZoneNumber > 1);
            _lastKnownZoneNumber = zone.ZoneNumber;

            if (isInitialOrReset || _suppressNextZoneSfx)
            {
                _suppressNextZoneSfx = false;
                return;
            }

            SoundEffectType zoneSfx = zone.ZoneType switch
            {
                ZoneType.Super => SoundEffectType.SuperZoneEnter,
                ZoneType.Safe => SoundEffectType.SafeZoneEnter,
                _ => SoundEffectType.ZoneAdvance
            };
            PlaySfx(zoneSfx);
        }

        private void HandleBombTriggered() => PlaySfx(SoundEffectType.BombPopupOpen);

        private void HandleCashOutPromptRequested() => PlaySfx(SoundEffectType.CashOutPopupOpen);

        private void HandleReviveSuccess()
        {
            _suppressNextZoneSfx = true;
            StopBombSounds();
            PlaySfx(SoundEffectType.ReviveSuccess);
        }

        private void StopBombSounds()
        {
            StopSfx(SoundEffectType.BombPopupOpen);
            StopSfx(SoundEffectType.WheelStopBomb);
        }
    }
}
