using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Vertigo.Core.State;
using Vertigo.Rewards.Controllers;
using Vertigo.Rewards.Views;
using Vertigo.Testing;
using Vertigo.UI.Panels;
using Vertigo.Wheel.Controllers;
using Vertigo.Wheel.Views;
using Vertigo.Zone.Controllers;
using Vertigo.Zone.Data;
using Vertigo.Zone.Views;

namespace Vertigo.Editor
{
    public static class VertigoSceneSetup
    {
        private const string ScenePath = "Assets/_Project/Scenes/MainGame.unity";

        // Badge layout (reference resolution 1920x1080, match width).
        private const float BadgeWidth = 210f;
        private const float BadgeHeight = 62f;
        private const float BadgeSpacing = 8f;
        private const float BadgeLabelFontSize = 18f;
        private const float BadgeValueFontSize = 36f;


        [MenuItem("Tools/Vertigo/Setup MainGame Scene", priority = 1)]
        public static void SetupMainGameScene()
        {
            string dir = Path.GetDirectoryName(ScenePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // Create new scene or open existing
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            GameObject cameraObj = new GameObject("Main Camera");
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.10f, 1f); // Dark Critical Strike backdrop
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cameraObj.tag = "MainCamera";

            // 2. Event System
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>();

            // 3. Core Game Engine Managers
            GameObject coreObj = new GameObject("[GameCore]");
            GameManager gameManager = coreObj.AddComponent<GameManager>();
            ZoneManager zoneManager = coreObj.AddComponent<ZoneManager>();
            WheelController wheelController = coreObj.AddComponent<WheelController>();
            SessionInventoryManager inventoryManager = coreObj.AddComponent<SessionInventoryManager>();
            ZoneTestManager testManager = coreObj.AddComponent<ZoneTestManager>();

            // Load Zone Config
            ZoneManagerConfigSO zoneConfig = AssetDatabase.LoadAssetAtPath<ZoneManagerConfigSO>("Assets/_Project/Data/Zones/ZoneConfig_Default.asset");
            if (zoneConfig != null)
            {
                SerializedObject zmSO = new SerializedObject(zoneManager);
                zmSO.FindProperty("zoneConfig").objectReferenceValue = zoneConfig;
                zmSO.ApplyModifiedProperties();
            }

            SerializedObject gmSO = new SerializedObject(gameManager);
            gmSO.FindProperty("zoneManager").objectReferenceValue = zoneManager;
            gmSO.FindProperty("wheelController").objectReferenceValue = wheelController;
            gmSO.FindProperty("inventoryManager").objectReferenceValue = inventoryManager;
            gmSO.ApplyModifiedProperties();

            // Load Sprites
            Sprite neutralFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_card_frame_12px_neutral.png");
            Sprite zoneMapFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_zonebar_frame.png");
            Sprite zoneBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_card_panel_zone_bg.png");
            Sprite zoneCurrentBadge = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_badge_current_zone.png");
            Sprite zoneSafeBadge = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_badge_current_zone_safe.png");
            Sprite zoneSuperBadge = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_badge_current_zone_super.png");
            Sprite superBadgeFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_badge_frame_super.png");
            Sprite safeBadgeFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_badge_frame_safe.png");
            Sprite superSwordIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_icon_badge_sword.png");
            Sprite safeDeltaIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_icon_badge_delta.png");
            Sprite zoneGreenBadge = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_card_panel_zone_super.png");
            Sprite zoneWhiteBadge = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_card_panel_zone_white.png");
            Sprite greyBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/UI_button_grey_standard.png");
            Sprite orangeBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/UI_button_orange_standard.png");
            Sprite spinBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_spin_generic_button.png");
            Sprite bronzeBase = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_spin_bronze_base.png");
            Sprite bronzeIndicator = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_spin_bronze_indicator.png");
            Sprite bombIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_card_icon_death.png");
            Sprite bayonetIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_icon_mle_bayonet_easter_time.png");
            Sprite starFlash = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/star_flash_alpha.png");
            Sprite bronzeFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_particle_frame_bronze.png");
            Sprite silverFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_particle_frame_silver.png");
            Sprite goldFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_particle_frame_gold.png");

            // 4. Main Canvas Setup (Scale With Screen Size, Match = 0, Responsive 20:9, 16:9, 4:3)
            GameObject canvasObj = new GameObject("ui_canvas_main");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f; // Exactly as requested: Match = 0

            canvasObj.AddComponent<GraphicRaycaster>();

            // =========================================================================
            // A. TOP ZONE BAR (Matching Screenshot, 15% scaled: 748x55)
            // =========================================================================
            GameObject topBarObj = CreateUIObject("ui_panel_top_zonebar", canvasObj.transform);
            RectTransform topBarRT = topBarObj.GetComponent<RectTransform>();
            SetAnchors(topBarRT, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            topBarRT.anchoredPosition = new Vector2(0, -25);
            topBarRT.sizeDelta = new Vector2(748, 55);

            Image topBarBg = topBarObj.AddComponent<Image>();
            topBarBg.sprite = zoneMapFrame;
            topBarBg.type = Image.Type.Sliced;
            topBarBg.color = Color.white;
            topBarBg.raycastTarget = false;

            ZoneBarView zoneBarView = topBarObj.AddComponent<ZoneBarView>();
            SerializedObject zoneBarSO = new SerializedObject(zoneBarView);

            // Single Sliding Badge (child 0, behind texts)
            GameObject badgeObj = CreateUIObject("ui_badge_current_zone", topBarObj.transform);
            RectTransform badgeRT = badgeObj.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0.5f, 0.5f);
            badgeRT.anchorMax = new Vector2(0.5f, 0.5f);
            badgeRT.pivot = new Vector2(0.5f, 0.5f);
            badgeRT.sizeDelta = new Vector2(53, 62);

            Image badgeImg = badgeObj.AddComponent<Image>();
            badgeImg.sprite = zoneCurrentBadge;
            badgeImg.type = Image.Type.Simple;
            badgeImg.preserveAspect = false;
            badgeImg.color = Color.white;
            badgeImg.raycastTarget = false;
            badgeImg.maskable = false;

            zoneBarSO.FindProperty("ui_transform_current_badge").objectReferenceValue = badgeRT;
            zoneBarSO.FindProperty("ui_image_current_badge").objectReferenceValue = badgeImg;
            zoneBarSO.FindProperty("badgeNormalSprite").objectReferenceValue = zoneCurrentBadge;
            zoneBarSO.FindProperty("badgeSafeSprite").objectReferenceValue = zoneSafeBadge;
            zoneBarSO.FindProperty("badgeSuperSprite").objectReferenceValue = zoneSuperBadge;

            // Container for horizontal zone slots
            GameObject slotsContainer = CreateUIObject("ui_container_zone_slots", topBarObj.transform);
            RectTransform slotsRT = slotsContainer.GetComponent<RectTransform>();
            SetAnchors(slotsRT, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            slotsRT.offsetMin = new Vector2(11.5f, 0);
            slotsRT.offsetMax = new Vector2(-11.5f, 0);

            zoneBarSO.FindProperty("ui_container_zone_slots").objectReferenceValue = slotsRT;

            HorizontalLayoutGroup hlg = slotsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 4.5f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Generate 13 clean text slots (matching Image 2 reference)
            const int slotCount = 13;
            SerializedProperty slotTextsProp = zoneBarSO.FindProperty("ui_slot_texts");
            SerializedProperty slotBgsProp = zoneBarSO.FindProperty("ui_slot_backgrounds");
            slotTextsProp.ClearArray();
            slotBgsProp.ClearArray();

            for (int i = 0; i < slotCount; i++)
            {
                GameObject slot = CreateUIObject($"ui_slot_{i}", slotsContainer.transform);

                // Child Text (ui_text_slot_value)
                GameObject slotTextObj = CreateUIObject("ui_text_slot_value", slot.transform);
                RectTransform stRT = slotTextObj.GetComponent<RectTransform>();
                SetAnchors(stRT, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
                stRT.offsetMin = Vector2.zero;
                stRT.offsetMax = Vector2.zero;

                TMP_Text slotTxt = slotTextObj.AddComponent<TextMeshProUGUI>();
                slotTxt.text = (i + 1).ToString();
                slotTxt.fontSize = 28;
                slotTxt.fontStyle = (i == 0) ? FontStyles.Bold : FontStyles.Normal;
                slotTxt.alignment = TextAlignmentOptions.Center;
                slotTxt.color = (i == 0) ? new Color(0.08f, 0.08f, 0.10f, 1f) : Color.white;
                slotTxt.raycastTarget = false;
                slotTxt.maskable = false;

                slotTextsProp.InsertArrayElementAtIndex(i);
                slotTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = slotTxt;
            }

            // =========================================================================
            // B. TOP RIGHT BADGES (Süper Bölge 30, Güvenli Bölge 15 - Matching Screenshot)
            // =========================================================================
            GameObject badgesObj = CreateUIObject("ui_panel_top_right_badges", canvasObj.transform);
            RectTransform badgesRT = badgesObj.GetComponent<RectTransform>();
            SetAnchors(badgesRT, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            badgesRT.anchoredPosition = new Vector2(-40, -82);
            badgesRT.sizeDelta = new Vector2(BadgeWidth, BadgeHeight * 2 + BadgeSpacing);

            // Super Zone Badge (Top) - Custom gold frame & claymore sword
            GameObject superBadgeObj = CreateUIObject("ui_badge_super_zone", badgesObj.transform);
            RectTransform superRT = superBadgeObj.GetComponent<RectTransform>();
            SetAnchors(superRT, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            superRT.anchoredPosition = Vector2.zero;
            superRT.sizeDelta = new Vector2(0, BadgeHeight);
            Image superBg = superBadgeObj.AddComponent<Image>();
            superBg.sprite = superBadgeFrame != null ? superBadgeFrame : zoneWhiteBadge;
            superBg.type = Image.Type.Sliced;
            superBg.color = Color.white;
            superBg.raycastTarget = false;

            TMP_Text superLabel = CreateTMPText("ui_text_label", superBadgeObj.transform, "SÜPER\nBÖLGE", BadgeLabelFontSize, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            superLabel.color = new Color(0.92f, 0.65f, 0.12f, 1f);
            superLabel.lineSpacing = -15f;
            SetAnchors(superLabel.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            superLabel.rectTransform.anchoredPosition = new Vector2(16, 0);
            superLabel.rectTransform.sizeDelta = new Vector2(75, 50);

            TMP_Text superVal = CreateTMPText("ui_text_next_super_zone_value", superBadgeObj.transform, "30", BadgeValueFontSize, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            superVal.color = new Color(1.0f, 0.73f, 0.12f, 1f);
            SetAnchors(superVal.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            superVal.rectTransform.anchoredPosition = new Vector2(92, 0);
            superVal.rectTransform.sizeDelta = new Vector2(55, 52);
            zoneBarSO.FindProperty("ui_text_next_super_zone_value").objectReferenceValue = superVal;

            GameObject superIconObj = CreateUIObject("ui_image_icon", superBadgeObj.transform);
            RectTransform sIconRT = superIconObj.GetComponent<RectTransform>();
            SetAnchors(sIconRT, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f));
            sIconRT.anchoredPosition = new Vector2(-10, 9);
            sIconRT.sizeDelta = new Vector2(82, 82);
            Image sIcon = superIconObj.AddComponent<Image>();
            sIcon.sprite = superSwordIcon != null ? superSwordIcon : bayonetIcon;
            sIcon.preserveAspect = true;
            sIcon.raycastTarget = false;

            // Safe Zone Badge (Bottom) - Custom green frame & Delta triangle
            GameObject safeBadgeObj = CreateUIObject("ui_badge_safe_zone", badgesObj.transform);
            RectTransform safeRT = safeBadgeObj.GetComponent<RectTransform>();
            SetAnchors(safeRT, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            safeRT.anchoredPosition = new Vector2(0, -(BadgeHeight + BadgeSpacing));
            safeRT.sizeDelta = new Vector2(0, BadgeHeight);
            Image safeBg = safeBadgeObj.AddComponent<Image>();
            safeBg.sprite = safeBadgeFrame != null ? safeBadgeFrame : zoneGreenBadge;
            safeBg.type = Image.Type.Sliced;
            safeBg.color = Color.white;
            safeBg.raycastTarget = false;

            TMP_Text safeLabel = CreateTMPText("ui_text_label", safeBadgeObj.transform, "GÜVENLİ\nBÖLGE", 17f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            safeLabel.color = new Color(0.62f, 0.92f, 0.30f, 1f);
            safeLabel.lineSpacing = -15f;
            SetAnchors(safeLabel.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            safeLabel.rectTransform.anchoredPosition = new Vector2(16, 0);
            safeLabel.rectTransform.sizeDelta = new Vector2(75, 50);

            TMP_Text safeVal = CreateTMPText("ui_text_next_safe_zone_value", safeBadgeObj.transform, "15", BadgeValueFontSize, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            safeVal.color = new Color(0.65f, 0.98f, 0.35f, 1f);
            SetAnchors(safeVal.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            safeVal.rectTransform.anchoredPosition = new Vector2(92, 0);
            safeVal.rectTransform.sizeDelta = new Vector2(55, 52);
            zoneBarSO.FindProperty("ui_text_next_safe_zone_value").objectReferenceValue = safeVal;

            GameObject safeIconObj = CreateUIObject("ui_image_icon", safeBadgeObj.transform);
            RectTransform safeIconRT = safeIconObj.GetComponent<RectTransform>();
            SetAnchors(safeIconRT, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f));
            safeIconRT.anchoredPosition = new Vector2(-18, 0);
            safeIconRT.sizeDelta = new Vector2(46, 46);
            Image safeIcon = safeIconObj.AddComponent<Image>();
            safeIcon.sprite = safeDeltaIcon;
            safeIcon.preserveAspect = true;
            safeIcon.raycastTarget = false;

            zoneBarSO.ApplyModifiedProperties();

            // =========================================================================
            // C. LEFT PANEL (ÇIKIŞ Button + Collected Loot List - Matching Screenshot)
            // =========================================================================
            GameObject leftPanelObj = CreateUIObject("ui_panel_left_inventory", canvasObj.transform);
            RectTransform leftRT = leftPanelObj.GetComponent<RectTransform>();
            // Anchor to Left Center: robust for 20:9, 16:9, and 4:3
            SetAnchors(leftRT, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            leftRT.anchoredPosition = new Vector2(40, -10);
            leftRT.sizeDelta = new Vector2(230, 720);

            Image leftBg = leftPanelObj.AddComponent<Image>();
            leftBg.sprite = neutralFrame;
            leftBg.type = Image.Type.Sliced;
            leftBg.color = Color.white; // Keep crisp silver/metallic outer frame
            leftBg.raycastTarget = false;

            InventoryView inventoryView = leftPanelObj.AddComponent<InventoryView>();
            SerializedObject invSO = new SerializedObject(inventoryView);

            // ÇIKIŞ Button at top of left panel
            GameObject cashOutBtnObj = CreateButton("ui_button_cashout", leftPanelObj.transform, greyBtnSprite, "ÇIKIŞ", 24);
            RectTransform cashOutRT = cashOutBtnObj.GetComponent<RectTransform>();
            SetAnchors(cashOutRT, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            cashOutRT.anchoredPosition = new Vector2(0, -36);
            cashOutRT.sizeDelta = new Vector2(195, 52);
            Button cashOutBtn = cashOutBtnObj.GetComponent<Button>();
            invSO.FindProperty("ui_button_cashout").objectReferenceValue = cashOutBtn;

            // Scroll View & Viewport with RectMask2D to prevent overflow
            GameObject scrollViewObj = CreateUIObject("ui_scrollview_inventory", leftPanelObj.transform);
            RectTransform svRT = scrollViewObj.GetComponent<RectTransform>();
            SetAnchors(svRT, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            svRT.offsetMin = new Vector2(0, 16);
            svRT.offsetMax = new Vector2(0, -82); // Below ÇIKIŞ button

            // Background image with raycastTarget = true so the scroll view catches drag and mouse wheel
            Image scrollBg = scrollViewObj.AddComponent<Image>();
            scrollBg.color = new Color(0, 0, 0, 0); // 100% transparent
            scrollBg.raycastTarget = true;
            scrollBg.maskable = true;

            scrollViewObj.AddComponent<RectMask2D>();

            ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.scrollSensitivity = 30f;
            scrollRect.viewport = svRT;

            // Slots Container with Vertical Layout & ContentSizeFitter
            GameObject slotsContainerObj = CreateUIObject("ui_container_slots", scrollViewObj.transform);
            RectTransform scRT = slotsContainerObj.GetComponent<RectTransform>();
            SetAnchors(scRT, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
            scRT.anchoredPosition = Vector2.zero;
            scRT.sizeDelta = Vector2.zero;

            scrollRect.content = scRT;

            VerticalLayoutGroup vlg = slotsContainerObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(0, 0, 4, 12);
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter csf = slotsContainerObj.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Template Slot for instantiating items (Centered horizontally matching reference)
            GameObject slotTemplate = CreateUIObject("ui_item_reward_slot_template", slotsContainerObj.transform);
            RectTransform slotTemplateRT = slotTemplate.GetComponent<RectTransform>();
            SetAnchors(slotTemplateRT, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
            slotTemplateRT.sizeDelta = new Vector2(230, 56);

            LayoutElement le = slotTemplate.AddComponent<LayoutElement>();
            le.minHeight = 56;
            le.preferredHeight = 56;
            le.flexibleHeight = 0;
            le.flexibleWidth = 1;

            // HorizontalLayoutGroup centers the [Icon + Amount] pair perfectly inside the panel
            HorizontalLayoutGroup sHlg = slotTemplate.AddComponent<HorizontalLayoutGroup>();
            sHlg.childAlignment = TextAnchor.MiddleCenter;
            sHlg.spacing = 16;
            sHlg.childControlWidth = false;
            sHlg.childControlHeight = false;
            sHlg.childForceExpandWidth = false;
            sHlg.childForceExpandHeight = false;

            // 1. Icon Image (clean sprite without awkward black square, perfectly centered)
            GameObject slotIconObj = CreateUIObject("ui_image_icon", slotTemplate.transform);
            RectTransform sIconRTransform = slotIconObj.GetComponent<RectTransform>();
            sIconRTransform.sizeDelta = new Vector2(54, 54);
            Image sIconImg = slotIconObj.AddComponent<Image>();
            sIconImg.preserveAspect = true;
            sIconImg.raycastTarget = false;
            sIconImg.maskable = true;

            // 2. Amount Text (Consistent left-aligned column with auto-sizing)
            GameObject textObj = CreateUIObject("ui_text_amount_value", slotTemplate.transform);
            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.sizeDelta = new Vector2(80, 46);

            TMP_Text slotAmt = textObj.AddComponent<TextMeshProUGUI>();
            slotAmt.text = "1";
            slotAmt.fontSize = 26;
            slotAmt.fontStyle = FontStyles.Bold;
            slotAmt.alignment = TextAlignmentOptions.MidlineLeft;
            slotAmt.enableAutoSizing = true;
            slotAmt.fontSizeMin = 18;
            slotAmt.fontSizeMax = 26;
            slotAmt.color = Color.white;
            slotAmt.raycastTarget = false;
            slotAmt.maskable = true;

            InventoryRewardSlotView slotViewComp = slotTemplate.AddComponent<InventoryRewardSlotView>();
            SerializedObject slotViewSO = new SerializedObject(slotViewComp);
            slotViewSO.FindProperty("ui_image_icon").objectReferenceValue = sIconImg;
            slotViewSO.FindProperty("ui_text_amount_value").objectReferenceValue = slotAmt;
            slotViewSO.ApplyModifiedProperties();

            slotTemplate.SetActive(false); // Inactive template

            invSO.FindProperty("ui_scroll_rect").objectReferenceValue = scrollRect;
            invSO.FindProperty("ui_container_slots").objectReferenceValue = slotsContainerObj.transform;
            invSO.FindProperty("slotPrefab").objectReferenceValue = slotViewComp;
            invSO.ApplyModifiedProperties();

            // =========================================================================
            // D. CENTER FORTUNE WHEEL (Replacing Cards in Screenshot)
            // =========================================================================
            GameObject wheelCenter = CreateUIObject("ui_panel_center_wheel", canvasObj.transform);
            RectTransform wheelCenterRT = wheelCenter.GetComponent<RectTransform>();
            SetAnchors(wheelCenterRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            wheelCenterRT.anchoredPosition = new Vector2(75, -20);
            wheelCenterRT.sizeDelta = new Vector2(560, 560);

            WheelView wheelView = wheelCenter.AddComponent<WheelView>();
            SerializedObject wheelViewSO = new SerializedObject(wheelView);
            wheelViewSO.FindProperty("wheelController").objectReferenceValue = wheelController;

            // Rotating Container
            GameObject rotatorObj = CreateUIObject("ui_transform_wheel_rotator", wheelCenter.transform);
            RectTransform rotatorRT = rotatorObj.GetComponent<RectTransform>();
            rotatorRT.sizeDelta = new Vector2(520, 520);
            rotatorRT.anchoredPosition = Vector2.zero;
            wheelViewSO.FindProperty("ui_transform_wheel_rotator").objectReferenceValue = rotatorRT;

            // Wheel Base Image
            Image wheelBaseImg = rotatorObj.AddComponent<Image>();
            wheelBaseImg.sprite = bronzeBase;
            wheelBaseImg.raycastTarget = false;
            wheelViewSO.FindProperty("ui_image_wheel_base").objectReferenceValue = wheelBaseImg;

            // 8 Slices arranged in a circle
            SerializedProperty slicesListProp = wheelViewSO.FindProperty("ui_slice_views");
            slicesListProp.ClearArray();

            for (int i = 0; i < 8; i++)
            {
                // Each slice holder is rotated by -i * 45 degrees
                GameObject sliceHolder = CreateUIObject($"ui_slice_{i}", rotatorObj.transform);
                RectTransform holderRT = sliceHolder.GetComponent<RectTransform>();
                holderRT.sizeDelta = new Vector2(100, 240);
                holderRT.anchoredPosition = Vector2.zero;
                holderRT.localRotation = Quaternion.Euler(0, 0, -i * 45f);

                // Slice item placed along radial direction (upwards in holder space)
                GameObject sliceContent = CreateUIObject("ui_slice_content", sliceHolder.transform);
                RectTransform contentRT = sliceContent.GetComponent<RectTransform>();
                contentRT.sizeDelta = new Vector2(90, 90);
                contentRT.anchoredPosition = new Vector2(0, 160);

                // Win Shine effect
                GameObject shineObj = CreateUIObject("ui_image_shine", sliceContent.transform);
                RectTransform shineRT = shineObj.GetComponent<RectTransform>();
                shineRT.sizeDelta = new Vector2(110, 110);
                Image shineImg = shineObj.AddComponent<Image>();
                shineImg.sprite = starFlash;
                shineImg.raycastTarget = false;
                shineObj.SetActive(false);

                // Slice Icon
                GameObject iconObj = CreateUIObject("ui_image_icon", sliceContent.transform);
                RectTransform iconRT = iconObj.GetComponent<RectTransform>();
                iconRT.sizeDelta = new Vector2(50, 50);
                iconRT.anchoredPosition = new Vector2(0, 12);
                Image iconImg = iconObj.AddComponent<Image>();
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;

                // Amount Text
                TMP_Text amtText = CreateTMPText("ui_text_amount_value", sliceContent.transform, "x10", 18, FontStyles.Bold, TextAlignmentOptions.Center);
                amtText.rectTransform.anchoredPosition = new Vector2(0, -22);
                amtText.rectTransform.sizeDelta = new Vector2(90, 26);

                WheelSliceView sliceViewComp = sliceHolder.AddComponent<WheelSliceView>();
                SerializedObject svSO = new SerializedObject(sliceViewComp);
                svSO.FindProperty("ui_image_icon").objectReferenceValue = iconImg;
                svSO.FindProperty("ui_text_amount_value").objectReferenceValue = amtText;
                svSO.FindProperty("ui_image_shine").objectReferenceValue = shineImg;
                svSO.ApplyModifiedProperties();

                slicesListProp.InsertArrayElementAtIndex(i);
                slicesListProp.GetArrayElementAtIndex(i).objectReferenceValue = sliceViewComp;
            }

            // Indicator at Top (Pinned at the top hinge pivot 0.5, 0.88)
            GameObject indicatorObj = CreateUIObject("ui_image_wheel_indicator", wheelCenter.transform);
            RectTransform indicatorRT = indicatorObj.GetComponent<RectTransform>();
            indicatorRT.pivot = new Vector2(0.5f, 0.88f);
            indicatorRT.sizeDelta = new Vector2(44, 60);
            indicatorRT.anchoredPosition = new Vector2(0, 260);
            Image indicatorImg = indicatorObj.AddComponent<Image>();
            indicatorImg.sprite = bronzeIndicator;
            indicatorImg.preserveAspect = true;
            indicatorImg.raycastTarget = false;
            wheelViewSO.FindProperty("ui_transform_indicator").objectReferenceValue = indicatorRT;
            wheelViewSO.FindProperty("ui_image_wheel_indicator").objectReferenceValue = indicatorImg;

            // Center Spin Button (Image already contains baked 'SPIN' text, no child label needed)
            GameObject spinBtnObj = CreateButton("ui_button_spin", wheelCenter.transform, spinBtnSprite, null, 0, isSliced: false);
            RectTransform spinBtnRT = spinBtnObj.GetComponent<RectTransform>();
            spinBtnRT.sizeDelta = new Vector2(130, 130);
            spinBtnRT.anchoredPosition = Vector2.zero;
            Button spinBtn = spinBtnObj.GetComponent<Button>();
            wheelViewSO.FindProperty("ui_button_spin").objectReferenceValue = spinBtn;

            wheelViewSO.ApplyModifiedProperties();
            wheelView.ApplySliceLayout();
            WheelLabelsSetupUtility.ApplyWheelLabelsToExistingScene();

            // =========================================================================
            // E. BOMB GAME OVER POPUP
            // =========================================================================
            GameObject bombPopup = CreateUIObject("ui_panel_bomb_popup", canvasObj.transform);
            RectTransform bombPopupRT = bombPopup.GetComponent<RectTransform>();
            SetAnchors(bombPopupRT, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            bombPopupRT.offsetMin = Vector2.zero;
            bombPopupRT.offsetMax = Vector2.zero;

            Image dimmer = bombPopup.AddComponent<Image>();
            dimmer.color = new Color(0, 0, 0, 0.88f);
            dimmer.raycastTarget = true;

            GameObject popupBox = CreateUIObject("ui_popup_box", bombPopup.transform);
            RectTransform boxRT = popupBox.GetComponent<RectTransform>();
            SetAnchors(boxRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            boxRT.sizeDelta = new Vector2(560, 560);
            Image boxImg = popupBox.AddComponent<Image>();
            boxImg.sprite = neutralFrame;
            boxImg.type = Image.Type.Sliced;
            boxImg.color = new Color(0.15f, 0.10f, 0.12f, 1f);

            TMP_Text bombTitle = CreateTMPText("ui_text_bomb_title", popupBox.transform, "BOMBA PATLADI!", 34, FontStyles.Bold, TextAlignmentOptions.Center);
            bombTitle.color = new Color(1f, 0.25f, 0.25f, 1f);
            bombTitle.rectTransform.anchoredPosition = new Vector2(0, 205);

            TMP_Text bombDesc = CreateTMPText("ui_text_bomb_desc", popupBox.transform, "Tüm topladığın ödüller kaybedildi!\nÖdüllerini kurtarmak için devam et veya pes et.", 18, FontStyles.Normal, TextAlignmentOptions.Center);
            bombDesc.rectTransform.anchoredPosition = new Vector2(0, 145);
            bombDesc.rectTransform.sizeDelta = new Vector2(480, 80);

            GameObject bombIconObj = CreateUIObject("ui_image_bomb_icon", popupBox.transform);
            RectTransform bombIconRT = bombIconObj.GetComponent<RectTransform>();
            bombIconRT.sizeDelta = new Vector2(170, 170);
            bombIconRT.anchoredPosition = new Vector2(0, 10);
            Image bombIconImg = bombIconObj.AddComponent<Image>();
            bombIconImg.sprite = bombIcon;
            bombIconImg.raycastTarget = false;

            GameObject giveUpBtnObj = CreateButton("ui_button_giveup", popupBox.transform, greyBtnSprite, "PES ET (SIFIRLA)", 20);
            RectTransform giveUpRT = giveUpBtnObj.GetComponent<RectTransform>();
            giveUpRT.sizeDelta = new Vector2(220, 62);
            giveUpRT.anchoredPosition = new Vector2(-125, -190);

            GameObject reviveBtnObj = CreateButton("ui_button_revive", popupBox.transform, orangeBtnSprite, "DEVAM ET (REVIVE)", 20);
            RectTransform reviveBtnRT = reviveBtnObj.GetComponent<RectTransform>();
            reviveBtnRT.sizeDelta = new Vector2(220, 62);
            reviveBtnRT.anchoredPosition = new Vector2(125, -190);

            BombPopupView bombPopupView = bombPopup.AddComponent<BombPopupView>();
            SerializedObject bpSO = new SerializedObject(bombPopupView);
            bpSO.FindProperty("ui_panel_popup_root").objectReferenceValue = bombPopup;
            bpSO.FindProperty("ui_transform_box").objectReferenceValue = boxRT;
            bpSO.FindProperty("ui_button_giveup").objectReferenceValue = giveUpBtnObj.GetComponent<Button>();
            bpSO.FindProperty("ui_button_revive").objectReferenceValue = reviveBtnObj.GetComponent<Button>();
            bpSO.ApplyModifiedProperties();

            bombPopup.SetActive(false);

            // =========================================================================
            // F. REWARD COLLECTION EFFECT CONTAINER
            // =========================================================================
            GameObject particleContainerObj = CreateUIObject("ui_particle_container", canvasObj.transform);
            RectTransform particleContainerRT = particleContainerObj.GetComponent<RectTransform>();
            SetAnchors(particleContainerRT, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            particleContainerRT.offsetMin = Vector2.zero;
            particleContainerRT.offsetMax = Vector2.zero;

            // Render below popups, above gameplay panels
            particleContainerObj.transform.SetSiblingIndex(bombPopup.transform.GetSiblingIndex());

            RewardCollectEffectView collectEffectView = particleContainerObj.AddComponent<RewardCollectEffectView>();
            SerializedObject ceSO = new SerializedObject(collectEffectView);
            ceSO.FindProperty("bronzeFrameSprite").objectReferenceValue = bronzeFrameSprite;
            ceSO.FindProperty("silverFrameSprite").objectReferenceValue = silverFrameSprite;
            ceSO.FindProperty("goldFrameSprite").objectReferenceValue = goldFrameSprite;
            ceSO.FindProperty("ui_particle_container").objectReferenceValue = particleContainerRT;
            ceSO.ApplyModifiedProperties();

            // Wire GameManager references
            gmSO.FindProperty("wheelView").objectReferenceValue = wheelView;
            gmSO.FindProperty("inventoryView").objectReferenceValue = inventoryView;
            gmSO.FindProperty("rewardCollectEffect").objectReferenceValue = collectEffectView;
            gmSO.ApplyModifiedProperties();

            RewardShowcaseSetupUtility.SetupRewardShowcase();
            BombPopupSetupUtility.SetupBombPopup();
            CashOutPopupSetupUtility.SetupCashOutPopup();

            // Save Scene
            EditorSceneManager.SaveScene(scene, ScenePath);

            // Add to Build Settings
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            EditorBuildSettings.scenes = scenes;

            Debug.Log($"[Vertigo] MainGame scene layout rebuilt matching screenshot with Canvas match=0 at {ScenePath}!");
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = pivot;
        }

        private static TMP_Text CreateTMPText(string name, Transform parent, string text, float fontSize, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject obj = CreateUIObject(name, parent);
            TMP_Text tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            tmp.maskable = false;
            return tmp;
        }

        private static GameObject CreateButton(string name, Transform parent, Sprite sprite, string label, float fontSize, bool isSliced = true)
        {
            GameObject btnObj = CreateUIObject(name, parent);
            Image img = btnObj.AddComponent<Image>();
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = isSliced ? Image.Type.Sliced : Image.Type.Simple;
                img.preserveAspect = !isSliced;
            }
            img.raycastTarget = true;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(1f, 1f, 1f, 1f);
            cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            cb.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.5f);
            btn.colors = cb;

            if (!string.IsNullOrEmpty(label))
            {
                TMP_Text txt = CreateTMPText("ui_text_label_value", btnObj.transform, label, fontSize, FontStyles.Bold, TextAlignmentOptions.Center);
                RectTransform txtRT = txt.rectTransform;
                SetAnchors(txtRT, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
                txtRT.offsetMin = Vector2.zero;
                txtRT.offsetMax = Vector2.zero;
            }

            return btnObj;
        }
    }
}
