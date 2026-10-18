#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Vertigo.Audio.Controllers;
using Vertigo.Audio.Data;
using Vertigo.Core.Enums;
using Vertigo.Core.State;

namespace Vertigo.Editor.AssetSetup
{
    [InitializeOnLoad]
    public static class AudioSetupUtility
    {
        private const string PlaceholderFolder = "Assets/_Project/Audio/Placeholders";
        private const string DataFolder = "Assets/_Project/Data/Audio";
        private const string ConfigAssetPath = "Assets/_Project/Data/Audio/AudioConfig_Default.asset";
        private const int SampleRate = 44100;

        static AudioSetupUtility()
        {
            EditorApplication.delayCall += AutoSetupIfNeeded;
        }

        private static void AutoSetupIfNeeded()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            AudioConfigSO existingConfig = AssetDatabase.LoadAssetAtPath<AudioConfigSO>(ConfigAssetPath);
            AudioManager sceneAudioMgr = UnityEngine.Object.FindFirstObjectByType<AudioManager>(FindObjectsInactive.Include);
            AudioListener sceneListener = UnityEngine.Object.FindFirstObjectByType<AudioListener>(FindObjectsInactive.Include);

            if (existingConfig == null || sceneAudioMgr == null || sceneListener == null)
            {
                SetupAudioSystemAndPlaceholders();
            }
        }

        [MenuItem("Vertigo/Audio/Setup Audio System & Placeholders")]
        public static void SetupAudioSystemAndPlaceholders()
        {
            EnsureFolders();

            // 1. Generate all 20 procedural placeholder .wav files if missing
            bool anyGenerated = GenerateAllPlaceholderWavs();
            if (anyGenerated)
            {
                AssetDatabase.Refresh();
            }

            // 2. Load AudioClips & optimize AudioImporter settings
            AudioClip clipButtonClick = LoadAndConfigureClip("sfx_button_click.wav");
            AudioClip clipSpinStart = LoadAndConfigureClip("sfx_wheel_spin_start.wav");
            AudioClip clipWheelTick = LoadAndConfigureClip("sfx_wheel_tick.wav");
            AudioClip clipStopReward = LoadAndConfigureClip("sfx_wheel_stop_reward.wav");
            AudioClip clipStopBomb = LoadAndConfigureClip("sfx_wheel_stop_bomb.wav");
            AudioClip clipShowcasePop = LoadAndConfigureClip("sfx_reward_showcase_pop.wav");
            AudioClip clipParticleBurst = LoadAndConfigureClip("sfx_reward_particle_burst.wav");
            AudioClip clipSlotHit = LoadAndConfigureClip("sfx_reward_slot_hit.wav");
            AudioClip clipZoneAdvance = LoadAndConfigureClip("sfx_zone_advance.wav");
            AudioClip clipSafeZone = LoadAndConfigureClip("sfx_safe_zone_enter.wav");
            AudioClip clipSuperZone = LoadAndConfigureClip("sfx_super_zone_enter.wav");
            AudioClip clipBombPopup = LoadAndConfigureClip("sfx_bomb_popup_open.wav");
            AudioClip clipRevive = LoadAndConfigureClip("sfx_revive_success.wav");
            AudioClip clipGiveUp = LoadAndConfigureClip("sfx_give_up.wav");
            AudioClip clipCashOutPopup = LoadAndConfigureClip("sfx_cashout_popup_open.wav");
            AudioClip clipChestAppear = LoadAndConfigureClip("sfx_chest_appear.wav");
            AudioClip clipCardSilver = LoadAndConfigureClip("sfx_card_reveal_silver.wav");
            AudioClip clipCardGold = LoadAndConfigureClip("sfx_card_reveal_gold.wav");
            AudioClip clipSummaryComplete = LoadAndConfigureClip("sfx_cashout_summary_complete.wav");
            AudioClip clipMissionComplete = LoadAndConfigureClip("sfx_mission_complete_victory.wav");

            // 3. Create or update AudioConfigSO
            AudioConfigSO config = AssetDatabase.LoadAssetAtPath<AudioConfigSO>(ConfigAssetPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<AudioConfigSO>();
                AssetDatabase.CreateAsset(config, ConfigAssetPath);
            }

            SoundEffectEntry[] entries = new SoundEffectEntry[]
            {
                SoundEffectEntry.CreateDefault(SoundEffectType.ButtonClick, clipButtonClick, 0.75f, 0.97f, 1.03f, 0.04f, false),
                SoundEffectEntry.CreateDefault(SoundEffectType.WheelSpinStart, clipSpinStart, 0.85f, 0.98f, 1.02f, 0.10f, true),
                SoundEffectEntry.CreateDefault(SoundEffectType.WheelTick, clipWheelTick, 0.65f, 0.95f, 1.05f, 0.032f, false),
                SoundEffectEntry.CreateDefault(SoundEffectType.WheelStopReward, clipStopReward, 0.85f, 0.98f, 1.02f, 0.10f, true),
                SoundEffectEntry.CreateDefault(SoundEffectType.WheelStopBomb, clipStopBomb, 0.95f, 0.96f, 1.02f, 0.15f, true),
                SoundEffectEntry.CreateDefault(SoundEffectType.RewardShowcasePop, clipShowcasePop, 0.85f, 0.98f, 1.03f, 0.10f, true),
                SoundEffectEntry.CreateDefault(SoundEffectType.RewardParticleBurst, clipParticleBurst, 0.75f, 0.96f, 1.04f, 0.08f, false),
                SoundEffectEntry.CreateDefault(SoundEffectType.RewardSlotHit, clipSlotHit, 0.80f, 0.98f, 1.06f, 0.045f, false),
                SoundEffectEntry.CreateDefault(SoundEffectType.ZoneAdvance, clipZoneAdvance, 0.75f, 0.98f, 1.02f, 0.10f, false),
                SoundEffectEntry.CreateDefault(SoundEffectType.SafeZoneEnter, clipSafeZone, 0.90f, 0.99f, 1.01f, 0.15f, true),
                SoundEffectEntry.CreateDefault(SoundEffectType.SuperZoneEnter, clipSuperZone, 0.95f, 0.99f, 1.01f, 0.15f, true),
                SoundEffectEntry.CreateDefault(SoundEffectType.BombPopupOpen, clipBombPopup, 0.85f, 0.98f, 1.02f, 0.15f, true),
                SoundEffectEntry.CreateDefault(SoundEffectType.ReviveSuccess, clipRevive, 0.90f, 0.99f, 1.02f, 0.15f, true),
                SoundEffectEntry.CreateDefault(SoundEffectType.GiveUp, clipGiveUp, 0.80f, 0.98f, 1.02f, 0.15f, true),
                SoundEffectEntry.CreateDefault(SoundEffectType.CashOutPopupOpen, clipCashOutPopup, 0.80f, 0.98f, 1.02f, 0.10f, false),
                SoundEffectEntry.CreateDefault(SoundEffectType.ChestAppear, clipChestAppear, 0.85f, 0.98f, 1.02f, 0.10f, true),
                SoundEffectEntry.CreateDefault(SoundEffectType.CardRevealSilver, clipCardSilver, 0.85f, 0.97f, 1.03f, 0.06f, false),
                SoundEffectEntry.CreateDefault(SoundEffectType.CardRevealGold, clipCardGold, 0.95f, 0.98f, 1.02f, 0.06f, true),
                SoundEffectEntry.CreateDefault(SoundEffectType.CashOutSummaryComplete, clipSummaryComplete, 0.90f, 0.99f, 1.01f, 0.15f, true),
                SoundEffectEntry.CreateDefault(SoundEffectType.MissionCompleteVictory, clipMissionComplete, 1.00f, 1.00f, 1.00f, 0.25f, true)
            };

            config.SetEntries(entries);
            EditorUtility.SetDirty(config);

            // 4. Create or wire AudioManager in active scene
            GameManager gameManager = UnityEngine.Object.FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
            AudioManager audioManager = UnityEngine.Object.FindFirstObjectByType<AudioManager>(FindObjectsInactive.Include);

            if (audioManager == null)
            {
                GameObject audioObj = new GameObject("[AudioManager]");
                if (gameManager != null)
                {
                    audioObj.transform.SetParent(gameManager.transform, false);
                }
                audioManager = audioObj.AddComponent<AudioManager>();
            }

            // Ensure Game View Mute Audio is not accidentally enabled
            if (EditorUtility.audioMasterMute)
            {
                EditorUtility.audioMasterMute = false;
            }

            // Ensure an AudioListener exists in the scene (on Main Camera or AudioManager)
            AudioListener audioListener = UnityEngine.Object.FindFirstObjectByType<AudioListener>(FindObjectsInactive.Include);
            if (audioListener == null)
            {
                Camera mainCam = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);
                GameObject listenerTarget = mainCam != null ? mainCam.gameObject : audioManager.gameObject;
                audioListener = listenerTarget.AddComponent<AudioListener>();
                EditorUtility.SetDirty(listenerTarget);
            }
            else if (!audioListener.enabled)
            {
                audioListener.enabled = true;
                EditorUtility.SetDirty(audioListener);
            }

            SerializedObject so = new SerializedObject(audioManager);
            so.FindProperty("audioConfig").objectReferenceValue = config;
            AssignIfNull(so, "buttonClickClip", clipButtonClick);
            AssignIfNull(so, "wheelSpinStartClip", clipSpinStart);
            AssignIfNull(so, "wheelTickClip", clipWheelTick);
            AssignIfNull(so, "wheelStopRewardClip", clipStopReward);
            AssignIfNull(so, "wheelStopBombClip", clipStopBomb);
            AssignIfNull(so, "rewardShowcasePopClip", clipShowcasePop);
            AssignIfNull(so, "rewardParticleBurstClip", clipParticleBurst);
            AssignIfNull(so, "rewardSlotHitClip", clipSlotHit);
            AssignIfNull(so, "zoneAdvanceClip", clipZoneAdvance);
            AssignIfNull(so, "safeZoneEnterClip", clipSafeZone);
            AssignIfNull(so, "superZoneEnterClip", clipSuperZone);
            AssignIfNull(so, "bombPopupOpenClip", clipBombPopup);
            AssignIfNull(so, "reviveSuccessClip", clipRevive);
            AssignIfNull(so, "giveUpClip", clipGiveUp);
            AssignIfNull(so, "cashOutPopupOpenClip", clipCashOutPopup);
            AssignIfNull(so, "chestAppearClip", clipChestAppear);
            AssignIfNull(so, "cardRevealSilverClip", clipCardSilver);
            AssignIfNull(so, "cardRevealGoldClip", clipCardGold);
            AssignIfNull(so, "cashOutSummaryCompleteClip", clipSummaryComplete);
            AssignIfNull(so, "missionCompleteVictoryClip", clipMissionComplete);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(audioManager);

            if (gameManager != null)
            {
                SerializedObject gmSo = new SerializedObject(gameManager);
                gmSo.FindProperty("audioManager").objectReferenceValue = audioManager;
                gmSo.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(gameManager);
            }

            AssetDatabase.SaveAssets();
            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(audioManager.gameObject.scene);
                EditorSceneManager.SaveScene(audioManager.gameObject.scene);
            }

            Debug.Log("[AudioSetupUtility] Audio System & 20 Placeholder Clips configured and wired to AudioManager in scene!");
        }

        private static void AssignIfNull(SerializedObject so, string propName, AudioClip clip)
        {
            SerializedProperty prop = so.FindProperty(propName);
            if (prop != null && prop.objectReferenceValue == null)
            {
                prop.objectReferenceValue = clip;
            }
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Audio"))
                AssetDatabase.CreateFolder("Assets/_Project", "Audio");
            if (!AssetDatabase.IsValidFolder(PlaceholderFolder))
                AssetDatabase.CreateFolder("Assets/_Project/Audio", "Placeholders");
            if (!AssetDatabase.IsValidFolder(DataFolder))
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Audio");
        }

        private static AudioClip LoadAndConfigureClip(string fileName)
        {
            string assetPath = $"{PlaceholderFolder}/{fileName}";
            AudioImporter importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
            if (importer != null && !importer.forceToMono)
            {
                importer.forceToMono = true;
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.PCM;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        }

        private static bool GenerateAllPlaceholderWavs()
        {
            bool generated = false;

            // 1. UI Button Click: short crisp dual-tone click
            generated |= WriteWavIfMissing("sfx_button_click.wav", 0.055f, (t, d) =>
                Envelope(t, d, 0.002f, 0.045f) * (Mathf.Sin(2f * Mathf.PI * 1150f * t) * 0.6f + Mathf.Sin(2f * Mathf.PI * 2300f * t) * 0.3f));

            // 2. Wheel Spin Start: rising mechanical sweep
            generated |= WriteWavIfMissing("sfx_wheel_spin_start.wav", 0.28f, (t, d) =>
            {
                float freq = Mathf.Lerp(260f, 680f, t / d);
                return Envelope(t, d, 0.01f, 0.22f) * Mathf.Sin(2f * Mathf.PI * freq * t) * 0.65f;
            });

            // 3. Wheel Tick: ultra-crisp wooden/metallic peg tick
            generated |= WriteWavIfMissing("sfx_wheel_tick.wav", 0.032f, (t, d) =>
            {
                float freq = Mathf.Lerp(1650f, 750f, t / d);
                return Mathf.Exp(-t * 110f) * Mathf.Sin(2f * Mathf.PI * freq * t) * 0.75f;
            });

            // 4. Wheel Stop Reward: bright two-note major third chime (C6 -> E6)
            generated |= WriteWavIfMissing("sfx_wheel_stop_reward.wav", 0.32f, (t, d) =>
            {
                float freq = t < 0.11f ? 1046.5f : 1318.5f;
                return Envelope(t, d, 0.005f, 0.25f) * (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f + Mathf.Sin(4f * Mathf.PI * freq * t) * 0.2f);
            });

            // 5. Wheel Stop Bomb: deep impact punch + rumble
            generated |= WriteWavIfMissing("sfx_wheel_stop_bomb.wav", 0.55f, (t, d) =>
            {
                float subFreq = Mathf.Lerp(140f, 38f, Mathf.Pow(t / d, 0.6f));
                float noise = ( Mathf.Sin(t * 12457.3f) * Mathf.Cos(t * 7919.1f) );
                return Envelope(t, d, 0.005f, 0.50f) * (Mathf.Sin(2f * Mathf.PI * subFreq * t) * 0.7f + noise * 0.35f * Mathf.Exp(-t * 6f));
            });

            // 6. Reward Showcase Pop: sparkly 3-note pop arpeggio (C5 - E5 - G5)
            generated |= WriteWavIfMissing("sfx_reward_showcase_pop.wav", 0.28f, (t, d) =>
            {
                float freq = t < 0.07f ? 523.25f : (t < 0.14f ? 659.25f : 783.99f);
                return Envelope(t, d, 0.005f, 0.22f) * (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.65f + Mathf.Sin(4f * Mathf.PI * freq * t) * 0.2f);
            });

            // 7. Reward Particle Burst: light upward shimmer
            generated |= WriteWavIfMissing("sfx_reward_particle_burst.wav", 0.20f, (t, d) =>
            {
                float freq = Mathf.Lerp(600f, 1400f, t / d);
                return Envelope(t, d, 0.01f, 0.16f) * Mathf.Sin(2f * Mathf.PI * freq * t) * 0.55f;
            });

            // 8. Reward Slot Hit: crisp coin/counter ding
            generated |= WriteWavIfMissing("sfx_reward_slot_hit.wav", 0.12f, (t, d) =>
                Mathf.Exp(-t * 32f) * (Mathf.Sin(2f * Mathf.PI * 1567.98f * t) * 0.6f + Mathf.Sin(2f * Mathf.PI * 2349.32f * t) * 0.25f));

            // 9. Zone Advance: clean step-up tone
            generated |= WriteWavIfMissing("sfx_zone_advance.wav", 0.20f, (t, d) =>
            {
                float freq = t < 0.08f ? 440f : 587.33f;
                return Envelope(t, d, 0.008f, 0.16f) * Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f;
            });

            // 10. Safe Zone Enter: warm silver chord (A4 + C#5 + E5)
            generated |= WriteWavIfMissing("sfx_safe_zone_enter.wav", 0.45f, (t, d) =>
                Envelope(t, d, 0.01f, 0.38f) * (
                    Mathf.Sin(2f * Mathf.PI * 440f * t) * 0.35f +
                    Mathf.Sin(2f * Mathf.PI * 554.37f * t) * 0.30f +
                    Mathf.Sin(2f * Mathf.PI * 659.25f * t) * 0.30f));

            // 11. Super Zone Enter: radiant golden chord (D5 + F#5 + A5 + D6)
            generated |= WriteWavIfMissing("sfx_super_zone_enter.wav", 0.60f, (t, d) =>
                Envelope(t, d, 0.01f, 0.52f) * (
                    Mathf.Sin(2f * Mathf.PI * 587.33f * t) * 0.30f +
                    Mathf.Sin(2f * Mathf.PI * 739.99f * t) * 0.25f +
                    Mathf.Sin(2f * Mathf.PI * 880.00f * t) * 0.25f +
                    Mathf.Sin(2f * Mathf.PI * 1174.66f * t) * 0.20f));

            // 12. Bomb Popup Open: dramatic low danger sting
            generated |= WriteWavIfMissing("sfx_bomb_popup_open.wav", 0.42f, (t, d) =>
            {
                float freq = t < 0.16f ? 220f : 196f;
                return Envelope(t, d, 0.01f, 0.35f) * (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.55f + Mathf.Sin(2f * Mathf.PI * (freq * 1.414f) * t) * 0.25f);
            });

            // 13. Revive Success: uplifting rising power-up chime
            generated |= WriteWavIfMissing("sfx_revive_success.wav", 0.45f, (t, d) =>
            {
                float freq = t < 0.12f ? 523.25f : (t < 0.24f ? 659.25f : 1046.50f);
                return Envelope(t, d, 0.01f, 0.38f) * (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.65f + Mathf.Sin(4f * Mathf.PI * freq * t) * 0.2f);
            });

            // 14. Give Up: descending two-note tone
            generated |= WriteWavIfMissing("sfx_give_up.wav", 0.35f, (t, d) =>
            {
                float freq = t < 0.15f ? 392f : 293.66f;
                return Envelope(t, d, 0.01f, 0.28f) * Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f;
            });

            // 15. CashOut Popup Open: clean dialog chime
            generated |= WriteWavIfMissing("sfx_cashout_popup_open.wav", 0.22f, (t, d) =>
            {
                float freq = t < 0.09f ? 587.33f : 880f;
                return Envelope(t, d, 0.008f, 0.18f) * Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f;
            });

            // 16. Chest Appear: resonant chest thud + sparkle
            generated |= WriteWavIfMissing("sfx_chest_appear.wav", 0.30f, (t, d) =>
            {
                float freq = Mathf.Lerp(180f, 520f, t / d);
                return Envelope(t, d, 0.008f, 0.24f) * (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f + Mathf.Sin(2f * Mathf.PI * 1046.5f * t) * 0.25f);
            });

            // 17. Card Reveal Silver: crisp card flip + silver bell
            generated |= WriteWavIfMissing("sfx_card_reveal_silver.wav", 0.26f, (t, d) =>
            {
                float freq = t < 0.06f ? 659.25f : 987.77f;
                return Envelope(t, d, 0.005f, 0.22f) * (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.65f + Mathf.Sin(4f * Mathf.PI * freq * t) * 0.2f);
            });

            // 18. Card Reveal Gold: rich golden arpeggio shimmer
            generated |= WriteWavIfMissing("sfx_card_reveal_gold.wav", 0.42f, (t, d) =>
            {
                float freq = t < 0.08f ? 587.33f : (t < 0.16f ? 880f : (t < 0.24f ? 1174.66f : 1479.98f));
                return Envelope(t, d, 0.005f, 0.36f) * (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.65f + Mathf.Sin(4f * Mathf.PI * freq * t) * 0.25f);
            });

            // 19. CashOut Summary Complete ("YOU GOT!"): celebratory chord
            generated |= WriteWavIfMissing("sfx_cashout_summary_complete.wav", 0.55f, (t, d) =>
                Envelope(t, d, 0.01f, 0.48f) * (
                    Mathf.Sin(2f * Mathf.PI * 523.25f * t) * 0.30f +
                    Mathf.Sin(2f * Mathf.PI * 659.25f * t) * 0.25f +
                    Mathf.Sin(2f * Mathf.PI * 783.99f * t) * 0.25f +
                    Mathf.Sin(2f * Mathf.PI * 1046.50f * t) * 0.20f));

            // 20. Mission Complete Victory (Zone 60): grand 4-note fanfare
            generated |= WriteWavIfMissing("sfx_mission_complete_victory.wav", 0.85f, (t, d) =>
            {
                float freq = t < 0.14f ? 523.25f : (t < 0.28f ? 659.25f : (t < 0.42f ? 783.99f : 1046.50f));
                return Envelope(t, d, 0.01f, 0.75f) * (
                    Mathf.Sin(2f * Mathf.PI * freq * t) * 0.55f +
                    Mathf.Sin(4f * Mathf.PI * freq * t) * 0.25f +
                    Mathf.Sin(6f * Mathf.PI * freq * t) * 0.12f);
            });

            return generated;
        }

        private static float Envelope(float t, float duration, float attack, float release)
        {
            if (t < attack) return Mathf.Clamp01(t / Mathf.Max(0.0001f, attack));
            float releaseStart = Mathf.Max(attack, duration - release);
            if (t > releaseStart) return Mathf.Clamp01(1f - ((t - releaseStart) / Mathf.Max(0.0001f, release)));
            return 1f;
        }

        private static bool WriteWavIfMissing(string fileName, float durationSeconds, Func<float, float, float> synthesizer)
        {
            string fullPath = Path.Combine(Application.dataPath, "_Project/Audio/Placeholders", fileName);
            if (File.Exists(fullPath)) return false;

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * durationSeconds));
            short[] pcm = new short[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / SampleRate;
                float sample = Mathf.Clamp(synthesizer(t, durationSeconds), -1f, 1f);
                pcm[i] = (short)Mathf.RoundToInt(sample * 32000f);
            }

            using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                int byteRate = SampleRate * 2;
                int dataChunkSize = sampleCount * 2;

                // RIFF header
                bw.Write(new char[] { 'R', 'I', 'F', 'F' });
                bw.Write(36 + dataChunkSize);
                bw.Write(new char[] { 'W', 'A', 'V', 'E' });

                // fmt subchunk
                bw.Write(new char[] { 'f', 'm', 't', ' ' });
                bw.Write(16);
                bw.Write((short)1); // PCM
                bw.Write((short)1); // Mono
                bw.Write(SampleRate);
                bw.Write(byteRate);
                bw.Write((short)2); // BlockAlign
                bw.Write((short)16); // BitsPerSample

                // data subchunk
                bw.Write(new char[] { 'd', 'a', 't', 'a' });
                bw.Write(dataChunkSize);
                for (int i = 0; i < sampleCount; i++)
                {
                    bw.Write(pcm[i]);
                }
            }

            return true;
        }
    }
}
#endif
