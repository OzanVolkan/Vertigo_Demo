using System;
using UnityEngine;
using Vertigo.Core.Enums;

namespace Vertigo.Audio.Data
{
    [Serializable]
    public struct SoundEffectEntry
    {
        [Tooltip("Identifier of the sound effect.")]
        public SoundEffectType type;

        [Tooltip("AudioClip to play when this sound effect is triggered.")]
        public AudioClip clip;

        [Range(0f, 1f)]
        [Tooltip("Base playback volume (0 to 1).")]
        public float volume;

        [Tooltip("Min (x) and Max (y) random pitch range for natural variation.")]
        public Vector2 pitchRange;

        [Min(0f)]
        [Tooltip("Minimum time (seconds) between consecutive plays of this sound to prevent audio stacking/clipping.")]
        public float minInterval;

        [Tooltip("If true, this sound cannot be interrupted by low-priority rapid sounds if all pool channels are busy.")]
        public bool highPriority;

        public static SoundEffectEntry CreateDefault(
            SoundEffectType type,
            AudioClip clip = null,
            float volume = 0.85f,
            float minPitch = 0.98f,
            float maxPitch = 1.02f,
            float minInterval = 0.03f,
            bool highPriority = false)
        {
            return new SoundEffectEntry
            {
                type = type,
                clip = clip,
                volume = volume,
                pitchRange = new Vector2(minPitch, maxPitch),
                minInterval = minInterval,
                highPriority = highPriority
            };
        }
    }
}
