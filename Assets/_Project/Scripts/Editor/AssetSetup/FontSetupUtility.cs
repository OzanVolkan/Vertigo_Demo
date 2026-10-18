using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vertigo.Editor
{
    public static class FontSetupUtility
    {
        private const string PrefKey = "Vertigo_RobotoFontMigration_V1";
        public const string SemiBoldFontPath = "Assets/_Project/Font/Roboto/Roboto-SemiBold SDF.asset";
        public const string RegularFontPath = "Assets/_Project/Font/Roboto/Roboto-Regular SDF.asset";

        private static readonly HashSet<string> RegularFontTargetNames = new HashSet<string>
        {
            "ui_text_bomb_desc",
            "ui_text_wheel_subtitle_value",
            "ui_text_cashout_prompt"
        };

        [MenuItem("Vertigo/UI/Apply Roboto Fonts (SemiBold & Regular)")]
        public static void ApplyRobotoFonts()
        {
            TMP_FontAsset semiBoldFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SemiBoldFontPath);
            TMP_FontAsset regularFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(RegularFontPath);

            if (semiBoldFont == null)
            {
                Debug.LogError($"[FontSetupUtility] SemiBold font not found at: {SemiBoldFontPath}");
                return;
            }

            if (regularFont == null)
            {
                Debug.LogError($"[FontSetupUtility] Regular font not found at: {RegularFontPath}");
                return;
            }

            // 1. Update TMP_Settings default font
            ApplyDefaultFontToTMPSETTINGS(semiBoldFont);

            // 2. Update prefabs
            ApplyFontToPrefabs(semiBoldFont, regularFont);

            // 3. Update current open scene
            ApplyFontToScene(semiBoldFont, regularFont);

            AssetDatabase.SaveAssets();
            Debug.Log("[FontSetupUtility] Successfully applied Roboto fonts (SemiBold by default, Regular for descriptions/subtitles) to TMP Settings, prefabs, and scene!");
        }

        [InitializeOnLoadMethod]
        private static void AutoApplyOnLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorPrefs.GetBool(PrefKey, false)) return;
            EditorPrefs.SetBool(PrefKey, true);

            EditorApplication.delayCall += () =>
            {
                ApplyRobotoFonts();
            };
        }

        private static void ApplyDefaultFontToTMPSETTINGS(TMP_FontAsset defaultFont)
        {
            if (TMP_Settings.instance != null)
            {
                SerializedObject settingsSO = new SerializedObject(TMP_Settings.instance);
                SerializedProperty prop = settingsSO.FindProperty("m_defaultFontAsset");
                if (prop != null)
                {
                    prop.objectReferenceValue = defaultFont;
                    settingsSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(TMP_Settings.instance);
                    Debug.Log($"[FontSetupUtility] Set TMP_Settings.defaultFontAsset to {defaultFont.name}");
                }
            }
        }

        private static void ApplyFontToPrefabs(TMP_FontAsset semiBoldFont, TMP_FontAsset regularFont)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs" });
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
                if (prefabRoot == null) continue;

                bool modified = false;
                TextMeshProUGUI[] tmps = prefabRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI tmp in tmps)
                {
                    TMP_FontAsset targetFont = RegularFontTargetNames.Contains(tmp.gameObject.name) ? regularFont : semiBoldFont;
                    if (tmp.font != targetFont)
                    {
                        tmp.font = targetFont;
                        tmp.fontSharedMaterial = targetFont.material;
                        tmp.maskable = true;
                        modified = true;
                    }
                }

                if (modified)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                    Debug.Log($"[FontSetupUtility] Updated fonts in prefab: {path}");
                }

                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void ApplyFontToScene(TMP_FontAsset semiBoldFont, TMP_FontAsset regularFont)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.isLoaded) return;

            TextMeshProUGUI[] sceneTmps = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int count = 0;

            foreach (TextMeshProUGUI tmp in sceneTmps)
            {
                TMP_FontAsset targetFont = RegularFontTargetNames.Contains(tmp.gameObject.name) ? regularFont : semiBoldFont;
                if (tmp.font != targetFont)
                {
                    tmp.font = targetFont;
                    tmp.fontSharedMaterial = targetFont.material;
                    tmp.maskable = true;
                    EditorUtility.SetDirty(tmp);
                    count++;
                }
            }

            if (count > 0)
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
                Debug.Log($"[FontSetupUtility] Updated {count} TextMeshPro components in scene '{activeScene.name}' to Roboto fonts!");
            }
        }
    }
}
