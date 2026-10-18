using UnityEngine;
using Vertigo.Core.Enums;

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

        public bool TryGetEntry(SoundEffectType type, out SoundEffectEntry entry)
        {
            if (entries != null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i].type == type)
                    {
                        entry = entries[i];
                        return true;
                    }
                }
            }

            entry = SoundEffectEntry.CreateDefault(type);
            return false;
        }
    }
}
