using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Vertigo.Core.Enums;
using Vertigo.Wheel.Views;

namespace Vertigo.Editor
{
    public static class WheelLabelsSetupUtility
    {
        private const string WheelLabelsPrefKey = "Vertigo_Wheel_HeaderFooterLabels_V1";

        [MenuItem("Vertigo/UI/Apply Wheel Header & Footer Labels")]
        public static void ApplyWheelLabelsMenu()
        {
            ApplyWheelLabelsToExistingScene();
        }

        [InitializeOnLoadMethod]
        private static void AutoSetupOnLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            if (!EditorPrefs.GetBool(WheelLabelsPrefKey, false))
            {
                EditorPrefs.SetBool(WheelLabelsPrefKey, true);
                EditorApplication.delayCall += ApplyWheelLabelsToExistingScene;
            }
        }

        /// <summary>
        /// Surgically adds 'ui_text_wheel_title_value' (above the wheel) and
        /// 'ui_text_wheel_subtitle_value' (below the wheel) inside 'ui_panel_center_wheel'
        /// without modifying any existing wheel or scene positions.
        /// </summary>
        public static void ApplyWheelLabelsToExistingScene()
        {
            GameObject canvasObj = GameObject.Find("ui_canvas_main");
            if (canvasObj == null) return;

            Transform wheelCenterTr = canvasObj.transform.Find("ui_panel_center_wheel");
            if (wheelCenterTr == null) return;

            WheelView wheelView = wheelCenterTr.GetComponent<WheelView>();
            if (wheelView == null) return;

            // 1. Top Title ("BRONZE SPIN" / "SILVER SPIN" / "GOLD SPIN")
            Transform titleTr = wheelCenterTr.Find("ui_text_wheel_title_value");
            GameObject titleObj = titleTr != null ? titleTr.gameObject : CreateUIObject("ui_text_wheel_title_value", wheelCenterTr);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.anchoredPosition = new Vector2(0f, 306f);
            titleRT.sizeDelta = new Vector2(500f, 48f);

            TextMeshProUGUI titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
            if (titleTmp == null) titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "BRONZE SPIN";
            titleTmp.fontSize = 32f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.96f, 0.68f, 0.32f, 1f);
            titleTmp.raycastTarget = false;
            titleTmp.maskable = false;

            // 2. Bottom Subtitle ("Up To x1 Rewards", dynamically updated per zone)
            Transform subTr = wheelCenterTr.Find("ui_text_wheel_subtitle_value");
            GameObject subObj = subTr != null ? subTr.gameObject : CreateUIObject("ui_text_wheel_subtitle_value", wheelCenterTr);
            RectTransform subRT = subObj.GetComponent<RectTransform>();
            subRT.anchorMin = new Vector2(0.5f, 0.5f);
            subRT.anchorMax = new Vector2(0.5f, 0.5f);
            subRT.pivot = new Vector2(0.5f, 0.5f);
            subRT.anchoredPosition = new Vector2(0f, -296f);
            subRT.sizeDelta = new Vector2(520f, 44f);

            TextMeshProUGUI subTmp = subObj.GetComponent<TextMeshProUGUI>();
            if (subTmp == null) subTmp = subObj.AddComponent<TextMeshProUGUI>();
            subTmp.text = "Up To x1 Rewards";
            subTmp.fontSize = 27f;
            subTmp.fontStyle = FontStyles.Bold;
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.color = new Color(0.96f, 0.68f, 0.32f, 1f);
            subTmp.raycastTarget = false;
            subTmp.maskable = false;

            // Wire references on WheelView
            SerializedObject wvSO = new SerializedObject(wheelView);
            wvSO.FindProperty("ui_text_wheel_title_value").objectReferenceValue = titleTmp;
            wvSO.FindProperty("ui_text_wheel_subtitle_value").objectReferenceValue = subTmp;
            wvSO.ApplyModifiedProperties();

            wheelView.UpdateWheelLabels(WheelType.BronzeNormal, 1f);

            EditorUtility.SetDirty(titleObj);
            EditorUtility.SetDirty(subObj);
            EditorUtility.SetDirty(wheelView);
            EditorSceneManager.MarkSceneDirty(canvasObj.scene);
            EditorSceneManager.SaveScene(canvasObj.scene);

            Debug.Log("[WheelLabelsSetup] Added 'ui_text_wheel_title_value' and 'ui_text_wheel_subtitle_value' to 'ui_panel_center_wheel'!");
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }
    }
}
