using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.State;
using Vertigo.UI.Panels;

namespace Vertigo.Editor
{
    public static class MissionCompleteSetupUtility
    {
        private const string SetupPrefKey = "Vertigo_MissionCompleteSetup_V2";
        public const string SemiBoldFontPath = "Assets/_Project/Font/Roboto/Roboto-SemiBold SDF.asset";

        [MenuItem("Vertigo/UI/Setup Mission Complete Popup View")]
        public static void SetupMissionCompletePopup()
        {
            SetupMissionCompletePopupInternal();
        }

        [InitializeOnLoadMethod]
        private static void AutoSetupOnLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorPrefs.GetBool(SetupPrefKey, false)) return;
            EditorPrefs.SetBool(SetupPrefKey, true);

            EditorApplication.delayCall += () =>
            {
                SetupMissionCompletePopupInternal();
            };
        }

        private static void SetupMissionCompletePopupInternal()
        {
            GameObject canvasObj = GameObject.Find("ui_canvas_main");
            if (canvasObj == null)
            {
                Debug.LogWarning("[MissionCompleteSetup] 'ui_canvas_main' not found!");
                return;
            }

            Sprite orangeButtonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/2D/demo_content/UI_button_orange_standard.png");
            Sprite glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/2D/Custom_UI/ui_glow_circle.png");
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SemiBoldFontPath);

            Transform existingTr = canvasObj.transform.Find("ui_panel_mission_complete");
            if (existingTr != null)
            {
                Object.DestroyImmediate(existingTr.gameObject);
            }

            GameObject rootObj = CreateUIObject("ui_panel_mission_complete", canvasObj.transform);
            RectTransform rootRT = rootObj.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.pivot = new Vector2(0.5f, 0.5f);
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // 1. Dark Vignetted Backdrop
            Image dimmerImg = rootObj.AddComponent<Image>();
            dimmerImg.color = new Color(0.04f, 0.04f, 0.06f, 0.88f);
            dimmerImg.raycastTarget = true;

            // Soft crimson ambient glow in center
            if (glowSprite != null)
            {
                GameObject glowObj = CreateUIObject("ui_image_ambient_glow", rootObj.transform);
                RectTransform glowRT = glowObj.GetComponent<RectTransform>();
                glowRT.anchorMin = new Vector2(0.5f, 0.5f);
                glowRT.anchorMax = new Vector2(0.5f, 0.5f);
                glowRT.pivot = new Vector2(0.5f, 0.5f);
                glowRT.anchoredPosition = Vector2.zero;
                glowRT.sizeDelta = new Vector2(900f, 500f);

                Image glowImg = glowObj.AddComponent<Image>();
                glowImg.sprite = glowSprite;
                glowImg.color = new Color(0.55f, 0.10f, 0.15f, 0.28f);
                glowImg.raycastTarget = false;
            }

            CanvasGroup canvasGroup = rootObj.AddComponent<CanvasGroup>();

            // 2. Dialog Container
            GameObject dialogObj = CreateUIObject("ui_transform_dialog", rootObj.transform);
            RectTransform dialogRT = dialogObj.GetComponent<RectTransform>();
            dialogRT.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRT.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRT.pivot = new Vector2(0.5f, 0.5f);
            dialogRT.anchoredPosition = Vector2.zero;
            dialogRT.sizeDelta = new Vector2(800f, 280f);

            // Title: CONGRATULATIONS!
            TMP_Text titleText = CreateTMPText(
                "ui_text_title",
                dialogObj.transform,
                "CONGRATULATIONS!",
                42f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                fontAsset);
            titleText.characterSpacing = 2f;
            titleText.color = Color.white;
            titleText.rectTransform.anchoredPosition = new Vector2(0f, 65f);
            titleText.rectTransform.sizeDelta = new Vector2(750f, 55f);

            // Subtitle: YOU COMPLETED THE MISSION!
            TMP_Text subtitleText = CreateTMPText(
                "ui_text_subtitle",
                dialogObj.transform,
                "YOU COMPLETED THE MISSION!",
                20f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                fontAsset);
            subtitleText.characterSpacing = 2f;
            subtitleText.color = new Color(0.85f, 0.88f, 0.92f, 1f);
            subtitleText.rectTransform.anchoredPosition = new Vector2(0f, 12f);
            subtitleText.rectTransform.sizeDelta = new Vector2(750f, 40f);

            // Button: COLLECT REWARDS (orange glowing button at bottom)
            GameObject collectBtnObj = CreateButton(
                "ui_button_collect_rewards",
                dialogObj.transform,
                orangeButtonSprite,
                "COLLECT REWARDS",
                20f,
                fontAsset);
            RectTransform collectBtnRT = collectBtnObj.GetComponent<RectTransform>();
            collectBtnRT.anchoredPosition = new Vector2(0f, -65f);
            collectBtnRT.sizeDelta = new Vector2(280f, 58f);
            Button collectBtn = collectBtnObj.GetComponent<Button>();

            // Setup MissionCompletePopupView component
            MissionCompletePopupView popupView = rootObj.AddComponent<MissionCompletePopupView>();
            SerializedObject viewSO = new SerializedObject(popupView);
            viewSO.FindProperty("ui_panel_popup_root").objectReferenceValue = rootObj;
            viewSO.FindProperty("ui_canvas_group").objectReferenceValue = canvasGroup;
            viewSO.FindProperty("ui_transform_dialog").objectReferenceValue = dialogRT;
            viewSO.FindProperty("ui_text_title").objectReferenceValue = titleText;
            viewSO.FindProperty("ui_text_subtitle").objectReferenceValue = subtitleText;
            viewSO.FindProperty("ui_button_collect_rewards").objectReferenceValue = collectBtn;
            viewSO.ApplyModifiedProperties();

            // Link in GameManager
            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                SerializedObject gmSO = new SerializedObject(gm);
                SerializedProperty prop = gmSO.FindProperty("missionCompletePopupView");
                if (prop != null)
                {
                    prop.objectReferenceValue = popupView;
                    gmSO.ApplyModifiedProperties();
                }
            }

            // Normal sibling order: place right before ui_panel_cashout_summary
            Transform cashoutSummary = canvasObj.transform.Find("ui_panel_cashout_summary");
            if (cashoutSummary != null)
            {
                rootObj.transform.SetSiblingIndex(cashoutSummary.GetSiblingIndex());
                cashoutSummary.SetAsLastSibling();
            }

            // Start in hidden state
            popupView.HideImmediate();

            EditorUtility.SetDirty(rootObj);
            EditorSceneManager.MarkSceneDirty(canvasObj.scene);
            EditorSceneManager.SaveScene(canvasObj.scene);

            Debug.Log("[MissionCompleteSetup] Successfully configured 'ui_panel_mission_complete'!");
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            obj.layer = LayerMask.NameToLayer("UI");
            return obj;
        }

        private static TMP_Text CreateTMPText(
            string name,
            Transform parent,
            string content,
            float fontSize,
            FontStyles style,
            TextAlignmentOptions alignment,
            TMP_FontAsset fontAsset)
        {
            GameObject obj = CreateUIObject(name, parent);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null)
            {
                tmp.font = fontAsset;
                tmp.fontSharedMaterial = fontAsset.material;
            }
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            tmp.maskable = false;
            return tmp;
        }

        private static GameObject CreateButton(
            string name,
            Transform parent,
            Sprite bgSprite,
            string label,
            float fontSize,
            TMP_FontAsset fontAsset)
        {
            GameObject btnObj = CreateUIObject(name, parent);
            Image img = btnObj.AddComponent<Image>();
            if (bgSprite != null)
            {
                img.sprite = bgSprite;
                img.type = Image.Type.Sliced;
            }
            img.color = Color.white;
            img.raycastTarget = true;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(1f, 1f, 1f, 0.95f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = cb;

            TMP_Text txt = CreateTMPText("ui_text_label", btnObj.transform, label, fontSize, FontStyles.Bold, TextAlignmentOptions.Center, fontAsset);
            RectTransform txtRT = txt.rectTransform;
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = Vector2.zero;
            txtRT.offsetMax = Vector2.zero;
            txt.color = Color.white;

            return btnObj;
        }
    }
}
