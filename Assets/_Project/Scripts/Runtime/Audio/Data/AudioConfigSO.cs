using UnityEngine;

namespace Vertigo.Audio.Data
{
    [CreateAssetMenu(fileName = "AudioConfig_Default", menuName = "Vertigo/Audio/Audio Config")]
    public class AudioConfigSO : ScriptableObject
    {
        [Header("Global Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float masterSfxVolume = 1f;

        [Header("Sound Effect Entries")]
        [SerializeField] private SoundEffectEntry[] entries = new SoundEffectEntry[0];

        public float MasterSfxVolume => masterSfxVolume;
        public SoundEffectEntry[] Entries => entries;

        public void SetEntries(SoundEffectEntry[] newEntries)
        {
            entries = newEntries;
        }
    }
}
