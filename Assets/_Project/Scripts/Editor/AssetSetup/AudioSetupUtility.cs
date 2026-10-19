#if UNITY_EDITOR
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
        private const string AudioFolder = "Assets/_Project/Audio";
        private const string DataFolder = "Assets/_Project/Data/Audio";
        private const string ConfigAssetPath = "Assets/_Project/Data/Audio/AudioConfig_Default.asset";

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

            bool needsPrePlacedSources = true;
            if (sceneAudioMgr != null)
            {
                SerializedObject checkSo = new SerializedObject(sceneAudioMgr);
                SerializedProperty spinSrcProp = checkSo.FindProperty("dedicatedWheelSpinSource");
                SerializedProperty sfxArrProp = checkSo.FindProperty("sfxSources");
                if (spinSrcProp != null && spinSrcProp.objectReferenceValue != null &&
                    sfxArrProp != null && sfxArrProp.arraySize >= 10 &&
                    sfxArrProp.GetArrayElementAtIndex(0).objectReferenceValue != null)
                {
                    needsPrePlacedSources = false;
                }
            }

            if (existingConfig == null || sceneAudioMgr == null || sceneListener == null || needsPrePlacedSources)
            {
                SetupAudioSystem();
            }
        }

        [MenuItem("Vertigo/Audio/Setup Audio System")]
        public static void SetupAudioSystem()
        {
            EnsureFolders();

            // 1. Load AudioClips from Assets/_Project/Audio
            AudioClip clipButtonClick = LoadClip("ClickyButton1a.wav");
            AudioClip clipSpinStart = LoadClip("SnappyButton5.wav");
            AudioClip clipWheelTick = LoadClip("Popup3.wav");
            AudioClip clipStopBomb = LoadClip("Error4.wav");
            AudioClip clipShowcasePop = LoadClip("Success9.wav");
            AudioClip clipParticleBurst = LoadClip("SnappyButton2.wav");
            AudioClip clipSlotHit = LoadClip("ClickyButton4.wav");
            AudioClip clipZoneAdvance = LoadClip("GenericNotification5.wav");
            AudioClip clipSafeZone = LoadClip("GenericNotification11.wav");
            AudioClip clipSuperZone = LoadClip("Success10.wav");
            AudioClip clipBombPopup = LoadClip("BombOpen.mp3");
            AudioClip clipRevive = LoadClip("OpenOrEnable1.wav");
            AudioClip clipGiveUp = LoadClip("Error5.wav");
            AudioClip clipCashOutPopup = LoadClip("Maximize4.wav");
            AudioClip clipChestAppear = LoadClip("OpenOrEnable4a.wav");
            AudioClip clipCardSilver = LoadClip("OpenOrEnable3.wav");
            AudioClip clipCardGold = LoadClip("OpenOrEnable5.wav");
            AudioClip clipSummaryComplete = LoadClip("SwooshSlide1a.wav");
            AudioClip clipMissionComplete = LoadClip("Success5.wav");

            // 2. Create or update AudioConfigSO
            AudioConfigSO config = AssetDatabase.LoadAssetAtPath<AudioConfigSO>(ConfigAssetPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<AudioConfigSO>();
                AssetDatabase.CreateAsset(config, ConfigAssetPath);
            }

            SoundEffectEntry[] entries = new SoundEffectEntry[]
            {
                SoundEffectEntry.CreateDefault(SoundEffectType.ButtonClick, clipButtonClick, 0.75f, 0.97f, 1.03f, 0.04f, false),
                SoundEffectEntry.CreateDefault(SoundEffectType.WheelSpinStart, clipSpinStart, 0.90f, 0.99f, 1.01f, 0.05f, true),
                SoundEffectEntry.CreateDefault(SoundEffectType.WheelTick, clipWheelTick, 0.65f, 0.95f, 1.05f, 0.032f, false),
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

            // 3. Create or wire AudioManager in active scene
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

            // 4. Pre-place and configure Dedicated WheelSpin AudioSource & 10 SFX AudioSources directly in Scene
            Transform spinSrcTr = audioManager.transform.Find("AudioSource_WheelSpin");
            GameObject spinSrcObj = spinSrcTr != null ? spinSrcTr.gameObject : new GameObject("AudioSource_WheelSpin");
            if (spinSrcTr == null) spinSrcObj.transform.SetParent(audioManager.transform, false);
            AudioSource dedicatedSpinSource = spinSrcObj.GetComponent<AudioSource>();
            if (dedicatedSpinSource == null) dedicatedSpinSource = spinSrcObj.AddComponent<AudioSource>();
            ConfigurePrePlaced2DSource(dedicatedSpinSource);

            Transform poolRoot = audioManager.transform.Find("AudioSource_Pool");
            if (poolRoot == null)
            {
                GameObject poolObj = new GameObject("AudioSource_Pool");
                poolObj.transform.SetParent(audioManager.transform, false);
                poolRoot = poolObj.transform;
            }

            const int PrePlacedChannelCount = 10;
            AudioSource[] prePlacedSources = new AudioSource[PrePlacedChannelCount];
            for (int i = 0; i < PrePlacedChannelCount; i++)
            {
                string chName = $"SFX_Channel_{i:D2}";
                Transform child = i < poolRoot.childCount ? poolRoot.GetChild(i) : poolRoot.Find(chName);
                GameObject chObj;
                if (child != null)
                {
                    chObj = child.gameObject;
                    chObj.name = chName;
                }
                else
                {
                    chObj = new GameObject(chName);
                    chObj.transform.SetParent(poolRoot, false);
                }

                AudioSource src = chObj.GetComponent<AudioSource>();
                if (src == null) src = chObj.AddComponent<AudioSource>();
                ConfigurePrePlaced2DSource(src);
                prePlacedSources[i] = src;
            }

            if (EditorUtility.audioMasterMute)
            {
                EditorUtility.audioMasterMute = false;
            }

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
            so.FindProperty("dedicatedWheelSpinSource").objectReferenceValue = dedicatedSpinSource;

            SerializedProperty sfxSourcesProp = so.FindProperty("sfxSources");
            if (sfxSourcesProp != null)
            {
                sfxSourcesProp.arraySize = PrePlacedChannelCount;
                for (int i = 0; i < PrePlacedChannelCount; i++)
                {
                    sfxSourcesProp.GetArrayElementAtIndex(i).objectReferenceValue = prePlacedSources[i];
                }
            }

            AssignIfNull(so, "buttonClickClip", clipButtonClick);
            AssignIfNull(so, "wheelSpinStartClip", clipSpinStart);
            AssignIfNull(so, "wheelTickClip", clipWheelTick);
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

            Debug.Log("[AudioSetupUtility] Pre-placed AudioSources & 19 Audio Clips configured in Assets/_Project/Audio!");
        }

        private static void ConfigurePrePlaced2DSource(AudioSource source)
        {
            if (source == null) return;
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.reverbZoneMix = 0f;
            EditorUtility.SetDirty(source);
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
            if (!AssetDatabase.IsValidFolder(AudioFolder))
                AssetDatabase.CreateFolder("Assets/_Project", "Audio");
            if (!AssetDatabase.IsValidFolder(DataFolder))
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Audio");
        }

        private static AudioClip LoadClip(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/{fileName}");
        }
    }
}
#endif
