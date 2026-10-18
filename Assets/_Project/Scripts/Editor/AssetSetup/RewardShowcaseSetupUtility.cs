using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.State;
using Vertigo.Rewards.Views;

namespace Vertigo.Editor
{
    public static class RewardShowcaseSetupUtility
    {
        [MenuItem("Vertigo/UI/Setup Reward Showcase View")]
        public static void SetupRewardShowcase()
        {
            SetupRewardShowcaseInternal();
        }

        [InitializeOnLoadMethod]
        private static void AutoSetupOnLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorPrefs.GetBool("RewardShowcaseSetup_V3", false)) return;
            EditorPrefs.SetBool("RewardShowcaseSetup_V3", true);

            EditorApplication.delayCall += () =>
            {
                SetupRewardShowcaseInternal();
            };
        }

        private static void SetupRewardShowcaseInternal()
        {
            GameObject canvasObj = GameObject.Find("ui_canvas_main");
            if (canvasObj == null)
            {
                Debug.LogWarning("[RewardShowcaseSetup] 'ui_canvas_main' not found!");
                return;
            }

            Transform existingTr = canvasObj.transform.Find("ui_panel_reward_showcase");
            GameObject rootObj;
            if (existingTr == null)
            {
                rootObj = new GameObject("ui_panel_reward_showcase", typeof(RectTransform));
                rootObj.transform.SetParent(canvasObj.transform, false);
            }
            else
            {
                rootObj = existingTr.gameObject;
            }

            RectTransform rootRT = rootObj.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.pivot = new Vector2(0.5f, 0.5f);
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // Put it right before bomb popup
            Transform bombPopup = canvasObj.transform.Find("ui_panel_bomb_popup");
            if (bombPopup != null)
            {
                rootObj.transform.SetSiblingIndex(bombPopup.GetSiblingIndex());
            }

            CanvasGroup dimmerGroup = rootObj.GetComponent<CanvasGroup>();
            if (dimmerGroup == null) dimmerGroup = rootObj.AddComponent<CanvasGroup>();

            // Dimmer Background Overlay
            Transform dimmerTr = rootObj.transform.Find("ui_image_dimmer");
            GameObject dimmerObj;
            if (dimmerTr == null)
            {
                dimmerObj = new GameObject("ui_image_dimmer", typeof(RectTransform), typeof(Image));
                dimmerObj.transform.SetParent(rootObj.transform, false);
            }
            else
            {
                dimmerObj = dimmerTr.gameObject;
            }
            RectTransform dimmerRT = dimmerObj.GetComponent<RectTransform>();
            dimmerRT.anchorMin = Vector2.zero;
            dimmerRT.anchorMax = Vector2.one;
            dimmerRT.offsetMin = Vector2.zero;
            dimmerRT.offsetMax = Vector2.zero;
            Image dimmerImg = dimmerObj.GetComponent<Image>();
            dimmerImg.color = new Color(0f, 0f, 0f, 0.65f);
            dimmerImg.raycastTarget = false;

            // Center position (aligned over center wheel at x=75, y=-20)
            Vector2 wheelCenterPos = new Vector2(75f, -20f);

            // Sunburst Rays
            Sprite sunburstSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/2D/demo_content/star_flash_alpha.png");
            Transform raysTr = rootObj.transform.Find("ui_transform_sunburst");
            GameObject raysObj;
            if (raysTr == null)
            {
                raysObj = new GameObject("ui_transform_sunburst", typeof(RectTransform), typeof(Image));
                raysObj.transform.SetParent(rootObj.transform, false);
            }
            else
            {
                raysObj = raysTr.gameObject;
            }
            RectTransform raysRT = raysObj.GetComponent<RectTransform>();
            raysRT.anchorMin = new Vector2(0.5f, 0.5f);
            raysRT.anchorMax = new Vector2(0.5f, 0.5f);
            raysRT.pivot = new Vector2(0.5f, 0.5f);
            raysRT.anchoredPosition = wheelCenterPos;
            raysRT.sizeDelta = new Vector2(680f, 680f);
            Image raysImg = raysObj.GetComponent<Image>();
            raysImg.sprite = sunburstSprite;
            raysImg.color = new Color(1f, 0.82f, 0.35f, 0.35f);
            raysImg.preserveAspect = true;
            raysImg.raycastTarget = false;

            // Glow Circle
            Sprite glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/2D/Custom_UI/ui_glow_circle.png");
            Transform glowTr = rootObj.transform.Find("ui_image_glow");
            GameObject glowObj;
            if (glowTr == null)
            {
                glowObj = new GameObject("ui_image_glow", typeof(RectTransform), typeof(Image));
                glowObj.transform.SetParent(rootObj.transform, false);
            }
            else
            {
                glowObj = glowTr.gameObject;
            }
            RectTransform glowRT = glowObj.GetComponent<RectTransform>();
            glowRT.anchorMin = new Vector2(0.5f, 0.5f);
            glowRT.anchorMax = new Vector2(0.5f, 0.5f);
            glowRT.pivot = new Vector2(0.5f, 0.5f);
            glowRT.anchoredPosition = wheelCenterPos;
            glowRT.sizeDelta = new Vector2(360f, 360f);
            Image glowImg = glowObj.GetComponent<Image>();
            glowImg.sprite = glowSprite;
            glowImg.color = new Color(1f, 0.75f, 0.2f, 0.7f);
            glowImg.preserveAspect = true;
            glowImg.raycastTarget = false;

            // Card Container Box
            Transform cardBoxTr = rootObj.transform.Find("ui_transform_card_box");
            GameObject cardBoxObj;
            if (cardBoxTr == null)
            {
                cardBoxObj = new GameObject("ui_transform_card_box", typeof(RectTransform));
                cardBoxObj.transform.SetParent(rootObj.transform, false);
            }
            else
            {
                cardBoxObj = cardBoxTr.gameObject;
            }
            RectTransform cardBoxRT = cardBoxObj.GetComponent<RectTransform>();
            cardBoxRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardBoxRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardBoxRT.pivot = new Vector2(0.5f, 0.5f);
            cardBoxRT.anchoredPosition = wheelCenterPos;
            cardBoxRT.sizeDelta = new Vector2(250f, 250f);

            // Card Background Image is disabled as requested (only clean reward icon + glow + text)
            Image cardBgImg = cardBoxObj.GetComponent<Image>();
            if (cardBgImg != null)
            {
                cardBgImg.sprite = null;
                cardBgImg.color = Color.clear;
                cardBgImg.enabled = false;
            }

            // Card Icon (Large, prominent and unobstructed)
            Transform iconTr = cardBoxObj.transform.Find("ui_image_reward_icon");
            GameObject iconObj;
            if (iconTr == null)
            {
                iconObj = new GameObject("ui_image_reward_icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(cardBoxObj.transform, false);
            }
            else
            {
                iconObj = iconTr.gameObject;
            }
            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = Vector2.zero;
            iconRT.sizeDelta = new Vector2(230f, 230f);
            Image iconImg = iconObj.GetComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Amount Text (Below icon, e.g. "x3")
            Transform amtTr = cardBoxObj.transform.Find("ui_text_amount_value");
            GameObject amtObj;
            if (amtTr == null)
            {
                amtObj = new GameObject("ui_text_amount_value", typeof(RectTransform), typeof(TextMeshProUGUI));
                amtObj.transform.SetParent(cardBoxObj.transform, false);
            }
            else
            {
                amtObj = amtTr.gameObject;
            }
            RectTransform amtRT = amtObj.GetComponent<RectTransform>();
            amtRT.anchorMin = new Vector2(0.5f, 0f);
            amtRT.anchorMax = new Vector2(0.5f, 0f);
            amtRT.pivot = new Vector2(0.5f, 1f);
            amtRT.anchoredPosition = new Vector2(0f, -14f);
            amtRT.sizeDelta = new Vector2(260f, 60f);

            TextMeshProUGUI amtTMP = amtObj.GetComponent<TextMeshProUGUI>();
            amtTMP.text = "x3";
            amtTMP.fontSize = 44f;
            amtTMP.fontStyle = FontStyles.Bold;
            amtTMP.alignment = TextAlignmentOptions.Top;
            amtTMP.color = Color.white;
            amtTMP.raycastTarget = false;

            // Component Setup on root
            RewardShowcaseView showcaseView = rootObj.GetComponent<RewardShowcaseView>();
            if (showcaseView == null) showcaseView = rootObj.AddComponent<RewardShowcaseView>();

            SerializedObject so = new SerializedObject(showcaseView);
            so.FindProperty("ui_root").objectReferenceValue = rootObj;
            so.FindProperty("ui_dimmer_group").objectReferenceValue = dimmerGroup;
            so.FindProperty("ui_transform_sunburst").objectReferenceValue = raysRT;
            so.FindProperty("ui_image_sunburst").objectReferenceValue = raysImg;
            so.FindProperty("ui_image_glow").objectReferenceValue = glowImg;
            so.FindProperty("ui_transform_card_box").objectReferenceValue = cardBoxRT;
            so.FindProperty("ui_image_card_bg").objectReferenceValue = cardBgImg;
            so.FindProperty("ui_image_reward_icon").objectReferenceValue = iconImg;
            so.FindProperty("ui_text_amount_value").objectReferenceValue = amtTMP;
            so.ApplyModifiedProperties();

            // Connect to GameManager
            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                SerializedObject gmSO = new SerializedObject(gm);
                gmSO.FindProperty("rewardShowcase").objectReferenceValue = showcaseView;
                gmSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(gm);
            }

            rootObj.SetActive(true);
            dimmerGroup.alpha = 0f;
            dimmerGroup.blocksRaycasts = false;
            cardBoxRT.localScale = Vector3.zero;

            EditorUtility.SetDirty(rootObj);
            EditorSceneManager.MarkSceneDirty(canvasObj.scene);
            EditorSceneManager.SaveScene(canvasObj.scene);

            Debug.Log("[RewardShowcaseSetup] Successfully created & configured RewardShowcaseView in scene!");
        }
    }
}
