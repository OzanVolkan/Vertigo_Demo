using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.State;
using Vertigo.UI.Panels;

namespace Vertigo.Editor
{
    public static class CashOutPopupSetupUtility
    {
        private const string SetupPrefKey = "Vertigo_CashOutPopupSetup_V1";

        [MenuItem("Vertigo/UI/Setup Cash Out Popup View")]
        public static void SetupCashOutPopup()
        {
            SetupCashOutPopupInternal();
        }

        [InitializeOnLoadMethod]
        private static void AutoSetupOnLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorPrefs.GetBool(SetupPrefKey, false)) return;
            EditorPrefs.SetBool(SetupPrefKey, true);

            EditorApplication.delayCall += () =>
            {
                SetupCashOutPopupInternal();
            };
        }

        private static void SetupCashOutPopupInternal()
        {
            GameObject canvasObj = GameObject.Find("ui_canvas_main");
            if (canvasObj == null)
            {
                Debug.LogWarning("[CashOutPopupSetup] 'ui_canvas_main' not found!");
                return;
            }

            Sprite greyButtonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/2D/demo_content/UI_button_grey_standard.png");
            Sprite orangeButtonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/2D/demo_content/UI_button_orange_standard.png");
            Sprite glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/2D/Custom_UI/ui_glow_circle.png");

            Transform existingTr = canvasObj.transform.Find("ui_panel_cashout_popup");
            if (existingTr != null)
            {
                Object.DestroyImmediate(existingTr.gameObject);
            }

            GameObject rootObj = CreateUIObject("ui_panel_cashout_popup", canvasObj.transform);
            RectTransform rootRT = rootObj.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.pivot = new Vector2(0.5f, 0.5f);
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // Place at top of canvas hierarchy
            rootObj.transform.SetAsLastSibling();

            CanvasGroup canvasGroup = rootObj.AddComponent<CanvasGroup>();

            // 1. Semi-transparent Dark Overlay
            Image dimmerImg = rootObj.AddComponent<Image>();
            dimmerImg.color = new Color(0f, 0f, 0f, 0.72f);
            dimmerImg.raycastTarget = true;

            float centerX = 75f; // Aligned with center wheel

            // 2. Dialog Container (Prompt Text + Side-by-Side Buttons)
            GameObject dialogObj = CreateUIObject("ui_transform_dialog", rootObj.transform);
            RectTransform dialogRT = dialogObj.GetComponent<RectTransform>();
            dialogRT.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRT.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRT.pivot = new Vector2(0.5f, 0.5f);
            dialogRT.anchoredPosition = new Vector2(centerX, -10f);
            dialogRT.sizeDelta = new Vector2(760f, 240f);

            // Soft radial dark shadow behind text & buttons for extra contrast over wheel
            GameObject backdropObj = CreateUIObject("ui_image_dialog_backdrop", dialogObj.transform);
            RectTransform backdropRT = backdropObj.GetComponent<RectTransform>();
            backdropRT.anchoredPosition = Vector2.zero;
            backdropRT.sizeDelta = new Vector2(860f, 360f);
            Image backdropImg = backdropObj.AddComponent<Image>();
            backdropImg.sprite = glowSprite;
            backdropImg.color = new Color(0f, 0f, 0f, 0.65f);
            backdropImg.raycastTarget = false;
            backdropImg.maskable = false;

            // 3. Top Prompt Text
            TMP_Text promptText = CreateTMPText(
                "ui_text_cashout_prompt",
                dialogObj.transform,
                "Want to exit and collect your rewards? The best one are\nsaved for the last!",
                26f,
                FontStyles.Bold,
                TextAlignmentOptions.Center);
            promptText.color = Color.white;
            promptText.rectTransform.anchoredPosition = new Vector2(0f, 54f);
            promptText.rectTransform.sizeDelta = new Vector2(740f, 84f);

            // 4a. Left Button: "COLLECT REWARDS" (Green)
            GameObject collectBtnObj = CreateUIObject("ui_button_collect_rewards", dialogObj.transform);
            RectTransform collectRT = collectBtnObj.GetComponent<RectTransform>();
            collectRT.anchoredPosition = new Vector2(-148f, -42f);
            collectRT.sizeDelta = new Vector2(268f, 68f);
            Image collectImg = collectBtnObj.AddComponent<Image>();
            collectImg.sprite = greyButtonSprite;
            collectImg.type = Image.Type.Sliced;
            collectImg.color = new Color(0.36f, 0.76f, 0.10f, 1f);
            Button collectBtn = collectBtnObj.AddComponent<Button>();
            collectBtn.targetGraphic = collectImg;

            TMP_Text collectLabel = CreateTMPText(
                "ui_text_label",
                collectBtnObj.transform,
                "COLLECT REWARDS",
                21f,
                FontStyles.Bold,
                TextAlignmentOptions.Center);
            collectLabel.color = Color.white;
            collectLabel.rectTransform.anchorMin = Vector2.zero;
            collectLabel.rectTransform.anchorMax = Vector2.one;
            collectLabel.rectTransform.offsetMin = new Vector2(8f, 0f);
            collectLabel.rectTransform.offsetMax = new Vector2(-8f, 0f);

            // 4b. Right Button: "GO BACK" (Orange)
            GameObject goBackBtnObj = CreateUIObject("ui_button_go_back", dialogObj.transform);
            RectTransform goBackRT = goBackBtnObj.GetComponent<RectTransform>();
            goBackRT.anchoredPosition = new Vector2(148f, -42f);
            goBackRT.sizeDelta = new Vector2(268f, 68f);
            Image goBackImg = goBackBtnObj.AddComponent<Image>();
            goBackImg.sprite = orangeButtonSprite;
            goBackImg.type = Image.Type.Sliced;
            goBackImg.color = Color.white;
            Button goBackBtn = goBackBtnObj.AddComponent<Button>();
            goBackBtn.targetGraphic = goBackImg;

            TMP_Text goBackLabel = CreateTMPText(
                "ui_text_label",
                goBackBtnObj.transform,
                "GO BACK",
                22f,
                FontStyles.Bold,
                TextAlignmentOptions.Center);
            goBackLabel.color = Color.white;
            goBackLabel.rectTransform.anchorMin = Vector2.zero;
            goBackLabel.rectTransform.anchorMax = Vector2.one;
            goBackLabel.rectTransform.offsetMin = new Vector2(8f, 0f);
            goBackLabel.rectTransform.offsetMax = new Vector2(-8f, 0f);

            // 5. Wire CashOutPopupView & GameManager
            CashOutPopupView popupView = rootObj.AddComponent<CashOutPopupView>();
            SerializedObject cpSO = new SerializedObject(popupView);
            cpSO.FindProperty("ui_panel_popup_root").objectReferenceValue = rootObj;
            cpSO.FindProperty("ui_canvas_group").objectReferenceValue = canvasGroup;
            cpSO.FindProperty("ui_transform_dialog").objectReferenceValue = dialogRT;
            cpSO.FindProperty("ui_text_prompt").objectReferenceValue = promptText;
            cpSO.FindProperty("ui_button_collect_rewards").objectReferenceValue = collectBtn;
            cpSO.FindProperty("ui_button_go_back").objectReferenceValue = goBackBtn;
            cpSO.ApplyModifiedProperties();

            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                SerializedObject gmSO = new SerializedObject(gm);
                gmSO.FindProperty("cashOutPopupView").objectReferenceValue = popupView;
                gmSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(gm);
            }

            // Keep GameObject active in scene so Awake/OnEnable always register events, hidden via CanvasGroup
            rootObj.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            EditorUtility.SetDirty(rootObj);
            EditorSceneManager.MarkSceneDirty(canvasObj.scene);
            EditorSceneManager.SaveScene(canvasObj.scene);

            Debug.Log("[CashOutPopupSetup] Successfully created & configured CashOutPopupView in MainGame scene!");
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static TMP_Text CreateTMPText(
            string name,
            Transform parent,
            string content,
            float fontSize,
            FontStyles style,
            TextAlignmentOptions alignment)
        {
            GameObject obj = CreateUIObject(name, parent);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            tmp.maskable = false;
            return tmp;
        }
    }
}
