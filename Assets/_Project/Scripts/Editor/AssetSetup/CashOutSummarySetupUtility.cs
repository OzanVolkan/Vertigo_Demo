using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.State;
using Vertigo.UI.Panels;

namespace Vertigo.Editor
{
    public static class CashOutSummarySetupUtility
    {
        private const string PrefKey = "Vertigo_CashOutSummarySetup_V8";

        private const string ChestSpritePath = "Assets/2D/demo_content/UI_icon_chest_gold_nolight.png";
        private const string BadgeCardStackSpritePath = "Assets/2D/demo_content/ui_badge_card_stack.png";
        private const string SilverCardSpritePath = "Assets/2D/demo_content/ui_card_frame_silver.png";
        private const string GoldCardSpritePath = "Assets/2D/demo_content/ui_card_frame_gold.png";
        private const string OrangeBtnSpritePath = "Assets/2D/demo_content/UI_button_orange_standard.png";
        private const string GreyBtnSpritePath = "Assets/2D/demo_content/UI_button_grey_standard.png";
        private const string CardFrameSpritePath = "Assets/2D/demo_content/ui_card_frame_12px_neutral.png";
        private const string GlowCircleSpritePath = "Assets/2D/demo_content/ui_glow_circle.png";
        private const string StarGlowSpritePath = "Assets/2D/demo_content/star_glow_alpha.png";

        private const string SilverCardPrefabPath = "Assets/_Project/Prefabs/UI/Cards/Card_Reward_Silver.prefab";
        private const string GoldCardPrefabPath = "Assets/_Project/Prefabs/UI/Cards/Card_Reward_Gold.prefab";

        [MenuItem("Vertigo/UI/Setup Cash Out Reward Summary Screen")]
        public static void SetupCashOutSummaryScreen()
        {
            SetupSummaryScreenInternal();
        }

        [MenuItem("Vertigo/UI/Force Recreate Card Prefabs (Silver & Gold)")]
        public static void ForceRecreateCardPrefabs()
        {
            Sprite silverCardSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SilverCardSpritePath);
            Sprite goldCardSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GoldCardSpritePath);
            EnsureCardPrefab(SilverCardPrefabPath, silverCardSprite, false, forceRecreate: true);
            EnsureCardPrefab(GoldCardPrefabPath, goldCardSprite, true, forceRecreate: true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            SetupSummaryScreenInternal();
        }

        [InitializeOnLoadMethod]
        private static void AutoSetupOnLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorPrefs.GetBool(PrefKey, false)) return;
            EditorPrefs.SetBool(PrefKey, true);

            EditorApplication.delayCall += () =>
            {
                SetupSummaryScreenInternal();
            };
        }

        private static void SetupSummaryScreenInternal()
        {
            GameObject canvasObj = GameObject.Find("ui_canvas_main");
            if (canvasObj == null)
            {
                var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var c in canvases)
                {
                    if (c.name == "ui_canvas_main")
                    {
                        canvasObj = c.gameObject;
                        break;
                    }
                }
            }

            if (canvasObj == null)
            {
                Debug.LogWarning("[CashOutSummarySetup] 'ui_canvas_main' not found in current scene!");
                return;
            }

            Sprite chestSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ChestSpritePath);
            Sprite badgeStackSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BadgeCardStackSpritePath);
            Sprite silverCardSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SilverCardSpritePath);
            Sprite goldCardSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GoldCardSpritePath);
            Sprite orangeBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(OrangeBtnSpritePath);
            Sprite greyBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GreyBtnSpritePath);
            Sprite cardFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CardFrameSpritePath);
            Sprite glowCircleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GlowCircleSpritePath);
            Sprite starGlowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(StarGlowSpritePath);

            // Ensure or load Card Prefabs (preserves any user edits in prefab mode!)
            CashOutSummaryCardView silverCardPrefab = EnsureCardPrefab(SilverCardPrefabPath, silverCardSprite, false);
            CashOutSummaryCardView goldCardPrefab = EnsureCardPrefab(GoldCardPrefabPath, goldCardSprite, true);

            // Recreate or update panel cleanly
            Transform existingTr = canvasObj.transform.Find("ui_panel_cashout_summary");
            if (existingTr != null)
            {
                Object.DestroyImmediate(existingTr.gameObject);
            }

            // 1. Root Panel
            GameObject rootObj = CreateUIObject("ui_panel_cashout_summary", canvasObj.transform);
            RectTransform rootRT = rootObj.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.pivot = new Vector2(0.5f, 0.5f);
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;
            rootObj.transform.SetAsLastSibling();

            CanvasGroup canvasGroup = rootObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            // Dark vignette overlay
            Image bgDimmer = rootObj.AddComponent<Image>();
            bgDimmer.color = new Color(0.04f, 0.07f, 0.10f, 0.94f); // Rich dark slate
            bgDimmer.raycastTarget = true;

            CashOutRewardSummaryView summaryView = rootObj.AddComponent<CashOutRewardSummaryView>();

            // 2. Skip Button (Top-Left)
            GameObject skipBtnObj = CreateUIObject("ui_button_skip", rootObj.transform);
            RectTransform skipRT = skipBtnObj.GetComponent<RectTransform>();
            skipRT.anchorMin = new Vector2(0f, 1f);
            skipRT.anchorMax = new Vector2(0f, 1f);
            skipRT.pivot = new Vector2(0f, 1f);
            skipRT.anchoredPosition = new Vector2(40f, -40f);
            skipRT.sizeDelta = new Vector2(150f, 52f);

            Image skipImg = skipBtnObj.AddComponent<Image>();
            if (greyBtnSprite != null)
            {
                skipImg.sprite = greyBtnSprite;
                skipImg.type = Image.Type.Sliced;
            }
            skipImg.color = new Color(0.85f, 0.90f, 0.95f, 0.9f);
            Button skipButton = skipBtnObj.AddComponent<Button>();

            TMP_Text skipText = CreateTMPText("ui_text_skip", skipBtnObj.transform, "SKIP", 24f, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform skipTextRT = skipText.GetComponent<RectTransform>();
            skipTextRT.anchorMin = Vector2.zero;
            skipTextRT.anchorMax = Vector2.one;
            skipTextRT.offsetMin = Vector2.zero;
            skipTextRT.offsetMax = Vector2.zero;
            skipText.color = Color.white;

            // 3. Phase 1: Reveal Stage
            GameObject revealStageObj = CreateUIObject("ui_transform_reveal_stage", rootObj.transform);
            RectTransform revealStageRT = revealStageObj.GetComponent<RectTransform>();
            revealStageRT.anchorMin = Vector2.zero;
            revealStageRT.anchorMax = Vector2.one;
            revealStageRT.offsetMin = Vector2.zero;
            revealStageRT.offsetMax = Vector2.zero;

            // Chest Container
            GameObject chestObj = CreateUIObject("ui_transform_chest", revealStageObj.transform);
            RectTransform chestRT = chestObj.GetComponent<RectTransform>();
            chestRT.anchorMin = new Vector2(0.5f, 0.5f);
            chestRT.anchorMax = new Vector2(0.5f, 0.5f);
            chestRT.pivot = new Vector2(0.5f, 0.5f);
            chestRT.anchoredPosition = new Vector2(0f, -200f);
            chestRT.sizeDelta = new Vector2(260f, 220f);

            // Behind chest glow
            GameObject chestGlowObj = CreateUIObject("ui_image_chest_glow", chestObj.transform);
            RectTransform chestGlowRT = chestGlowObj.GetComponent<RectTransform>();
            chestGlowRT.anchorMin = new Vector2(0.5f, 0.5f);
            chestGlowRT.anchorMax = new Vector2(0.5f, 0.5f);
            chestGlowRT.anchoredPosition = Vector2.zero;
            chestGlowRT.sizeDelta = new Vector2(360f, 320f);
            Image chestGlowImg = chestGlowObj.AddComponent<Image>();
            if (glowCircleSprite != null) chestGlowImg.sprite = glowCircleSprite;
            chestGlowImg.color = new Color(1f, 0.85f, 0.45f, 0.32f);
            chestGlowImg.raycastTarget = false;

            // Chest Image
            GameObject chestImgObj = CreateUIObject("ui_image_chest", chestObj.transform);
            RectTransform chestImgRT = chestImgObj.GetComponent<RectTransform>();
            chestImgRT.anchorMin = Vector2.zero;
            chestImgRT.anchorMax = Vector2.one;
            chestImgRT.offsetMin = Vector2.zero;
            chestImgRT.offsetMax = Vector2.zero;
            Image chestImg = chestImgObj.AddComponent<Image>();
            if (chestSprite != null) chestImg.sprite = chestSprite;
            chestImg.preserveAspect = true;
            chestImg.raycastTarget = false;

            // Remaining Items Badge (beside chest - fanned cards stack matching reference)
            GameObject badgeObj = CreateUIObject("ui_badge_remaining", chestObj.transform);
            RectTransform badgeRT = badgeObj.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0.5f, 0.5f);
            badgeRT.anchorMax = new Vector2(0.5f, 0.5f);
            badgeRT.pivot = new Vector2(0.5f, 0.5f);
            badgeRT.anchoredPosition = new Vector2(108f, -28f);
            badgeRT.sizeDelta = new Vector2(80f, 88f);

            Image badgeImg = badgeObj.AddComponent<Image>();
            if (badgeStackSprite != null)
            {
                badgeImg.sprite = badgeStackSprite;
                badgeImg.preserveAspect = true;
            }
            else if (cardFrameSprite != null)
            {
                badgeImg.sprite = cardFrameSprite;
                badgeImg.type = Image.Type.Sliced;
            }
            badgeImg.color = Color.white;
            badgeImg.raycastTarget = false;

            TMP_Text remainingCountText = CreateTMPText("ui_text_remaining_count", badgeObj.transform, "0", 34f, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform remTextRT = remainingCountText.GetComponent<RectTransform>();
            // Centered on the front card of the fanned stack (cards fan out to the left, so front card center is slightly right)
            remTextRT.anchorMin = new Vector2(0.5f, 0.5f);
            remTextRT.anchorMax = new Vector2(0.5f, 0.5f);
            remTextRT.pivot = new Vector2(0.5f, 0.5f);
            remTextRT.anchoredPosition = new Vector2(8f, -2f);
            remTextRT.sizeDelta = new Vector2(48f, 52f);
            remainingCountText.color = new Color(0.96f, 0.12f, 0.12f, 1f);
            remainingCountText.outlineWidth = 0.2f;
            remainingCountText.outlineColor = new Color32(40, 0, 0, 220);

            // Active Floating Card (Phase 1)
            GameObject activeCardObj = CreateUIObject("ui_transform_active_card", revealStageObj.transform);
            RectTransform activeCardRT = activeCardObj.GetComponent<RectTransform>();
            activeCardRT.anchorMin = new Vector2(0.5f, 0.5f);
            activeCardRT.anchorMax = new Vector2(0.5f, 0.5f);
            activeCardRT.pivot = new Vector2(0.5f, 0.5f);
            activeCardRT.anchoredPosition = new Vector2(0f, 70f);
            activeCardRT.sizeDelta = new Vector2(210f, 280f);

            CanvasGroup activeCardCG = activeCardObj.AddComponent<CanvasGroup>();

            // Glow behind active card
            GameObject cardGlowObj = CreateUIObject("ui_image_card_glow", activeCardObj.transform);
            RectTransform cardGlowRT = cardGlowObj.GetComponent<RectTransform>();
            cardGlowRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardGlowRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardGlowRT.anchoredPosition = Vector2.zero;
            cardGlowRT.sizeDelta = new Vector2(380f, 480f);
            Image cardGlowImg = cardGlowObj.AddComponent<Image>();
            if (starGlowSprite != null) cardGlowImg.sprite = starGlowSprite;
            else if (glowCircleSprite != null) cardGlowImg.sprite = glowCircleSprite;
            cardGlowImg.color = new Color(0.25f, 0.55f, 0.95f, 0.35f);
            cardGlowImg.raycastTarget = false;

            // Instantiate Silver & Gold Prefab Instances for Active Reveal
            CashOutSummaryCardView activeSilverCard = null;
            if (silverCardPrefab != null)
            {
                GameObject activeSilverObj = (GameObject)PrefabUtility.InstantiatePrefab(silverCardPrefab.gameObject, activeCardObj.transform);
                activeSilverObj.name = "ActiveCard_Silver";
                RectTransform silverRT = activeSilverObj.GetComponent<RectTransform>();
                silverRT.anchorMin = new Vector2(0.5f, 0.5f);
                silverRT.anchorMax = new Vector2(0.5f, 0.5f);
                silverRT.pivot = new Vector2(0.5f, 0.5f);
                silverRT.anchoredPosition = Vector2.zero;
                activeSilverObj.SetActive(false);
                activeSilverCard = activeSilverObj.GetComponent<CashOutSummaryCardView>();
            }

            CashOutSummaryCardView activeGoldCard = null;
            if (goldCardPrefab != null)
            {
                GameObject activeGoldObj = (GameObject)PrefabUtility.InstantiatePrefab(goldCardPrefab.gameObject, activeCardObj.transform);
                activeGoldObj.name = "ActiveCard_Gold";
                RectTransform goldRT = activeGoldObj.GetComponent<RectTransform>();
                goldRT.anchorMin = new Vector2(0.5f, 0.5f);
                goldRT.anchorMax = new Vector2(0.5f, 0.5f);
                goldRT.pivot = new Vector2(0.5f, 0.5f);
                goldRT.anchoredPosition = Vector2.zero;
                activeGoldObj.SetActive(false);
                activeGoldCard = activeGoldObj.GetComponent<CashOutSummaryCardView>();
            }

            activeCardObj.SetActive(false);

            // 4. Phase 2: Final Summary Stage
            GameObject summaryStageObj = CreateUIObject("ui_transform_summary_stage", rootObj.transform);
            RectTransform summaryStageRT = summaryStageObj.GetComponent<RectTransform>();
            summaryStageRT.anchorMin = Vector2.zero;
            summaryStageRT.anchorMax = Vector2.one;
            summaryStageRT.offsetMin = Vector2.zero;
            summaryStageRT.offsetMax = Vector2.zero;

            // Header Title: "YOU GOT:"
            TMP_Text headerText = CreateTMPText("ui_text_header", summaryStageObj.transform, "YOU GOT:", 46f, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform headerRT = headerText.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0.5f, 0.5f);
            headerRT.anchorMax = new Vector2(0.5f, 0.5f);
            headerRT.pivot = new Vector2(0.5f, 0.5f);
            headerRT.anchoredPosition = new Vector2(0f, 360f);
            headerRT.sizeDelta = new Vector2(700f, 70f);
            headerText.color = Color.white;

            // Scroll View for Summary Cards
            GameObject scrollObj = CreateUIObject("ui_scroll_summary_cards", summaryStageObj.transform);
            RectTransform scrollRT = scrollObj.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0.5f, 0.5f);
            scrollRT.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRT.pivot = new Vector2(0.5f, 0.5f);
            scrollRT.anchoredPosition = new Vector2(0f, 20f);
            scrollRT.sizeDelta = new Vector2(1720f, 380f);

            ScrollRect scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            // Viewport
            GameObject viewportObj = CreateUIObject("ui_viewport", scrollObj.transform);
            RectTransform viewportRT = viewportObj.GetComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = Vector2.zero;
            viewportRT.offsetMax = Vector2.zero;

            Image viewportImg = viewportObj.AddComponent<Image>();
            viewportImg.color = Color.clear;
            viewportImg.raycastTarget = true;

            viewportObj.AddComponent<RectMask2D>();
            scrollRect.viewport = viewportRT;

            // Content Container
            GameObject contentObj = CreateUIObject("ui_container_cards", viewportObj.transform);
            RectTransform contentRT = contentObj.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 0.5f);
            contentRT.anchorMax = new Vector2(0f, 0.5f);
            contentRT.pivot = new Vector2(0f, 0.5f);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(0f, 360f);

            HorizontalLayoutGroup hlg = contentObj.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.padding = new RectOffset(40, 40, 0, 0);
            hlg.spacing = 24f;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            scrollRect.content = contentRT;
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 25f;

            // Continue Button (Bottom Center)
            GameObject contBtnObj = CreateUIObject("ui_button_continue", summaryStageObj.transform);
            RectTransform contRT = contBtnObj.GetComponent<RectTransform>();
            contRT.anchorMin = new Vector2(0.5f, 0.5f);
            contRT.anchorMax = new Vector2(0.5f, 0.5f);
            contRT.pivot = new Vector2(0.5f, 0.5f);
            contRT.anchoredPosition = new Vector2(0f, -380f);
            contRT.sizeDelta = new Vector2(320f, 76f);

            Image contImg = contBtnObj.AddComponent<Image>();
            if (orangeBtnSprite != null)
            {
                contImg.sprite = orangeBtnSprite;
                contImg.type = Image.Type.Sliced;
            }
            Button contButton = contBtnObj.AddComponent<Button>();

            TMP_Text contText = CreateTMPText("ui_text_continue", contBtnObj.transform, "CONTINUE", 30f, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform contTextRT = contText.GetComponent<RectTransform>();
            contTextRT.anchorMin = Vector2.zero;
            contTextRT.anchorMax = Vector2.one;
            contTextRT.offsetMin = Vector2.zero;
            contTextRT.offsetMax = Vector2.zero;
            contText.color = Color.white;

            summaryStageObj.SetActive(false);

            // 5. Wire all serialized fields into CashOutRewardSummaryView via SerializedObject
            SerializedObject summarySO = new SerializedObject(summaryView);
            summarySO.FindProperty("ui_panel_summary_root").objectReferenceValue = rootObj;
            summarySO.FindProperty("ui_canvas_group").objectReferenceValue = canvasGroup;
            summarySO.FindProperty("ui_text_header").objectReferenceValue = headerText;
            summarySO.FindProperty("ui_button_skip").objectReferenceValue = skipButton;
            summarySO.FindProperty("ui_button_continue").objectReferenceValue = contButton;

            summarySO.FindProperty("ui_transform_reveal_stage").objectReferenceValue = revealStageRT;
            summarySO.FindProperty("ui_transform_chest").objectReferenceValue = chestRT;
            summarySO.FindProperty("ui_badge_remaining").objectReferenceValue = badgeRT;
            summarySO.FindProperty("ui_text_remaining_count").objectReferenceValue = remainingCountText;
            summarySO.FindProperty("ui_transform_active_card").objectReferenceValue = activeCardRT;
            summarySO.FindProperty("activeSilverCard").objectReferenceValue = activeSilverCard;
            summarySO.FindProperty("activeGoldCard").objectReferenceValue = activeGoldCard;
            summarySO.FindProperty("ui_image_active_card_glow").objectReferenceValue = cardGlowImg;
            summarySO.FindProperty("ui_active_card_canvas_group").objectReferenceValue = activeCardCG;

            summarySO.FindProperty("silverCardSprite").objectReferenceValue = silverCardSprite;
            summarySO.FindProperty("goldCardSprite").objectReferenceValue = goldCardSprite;

            summarySO.FindProperty("ui_transform_summary_stage").objectReferenceValue = summaryStageRT;
            summarySO.FindProperty("ui_container_cards").objectReferenceValue = contentObj.transform;
            summarySO.FindProperty("silverCardPrefab").objectReferenceValue = silverCardPrefab;
            summarySO.FindProperty("goldCardPrefab").objectReferenceValue = goldCardPrefab;
            summarySO.FindProperty("cardTemplate").objectReferenceValue = null;
            summarySO.FindProperty("ui_scroll_rect").objectReferenceValue = scrollRect;
            summarySO.ApplyModifiedProperties();

            // Wire into GameManager
            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                SerializedObject gmSO = new SerializedObject(gm);
                var prop = gmSO.FindProperty("cashOutSummaryView");
                if (prop != null)
                {
                    prop.objectReferenceValue = summaryView;
                    gmSO.ApplyModifiedProperties();
                }
                EditorUtility.SetDirty(gm);
            }

            // Keep rootObj active in hierarchy so Awake/OnEnable hook events, hidden via alpha = 0
            rootObj.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            EditorUtility.SetDirty(rootObj);
            EditorSceneManager.MarkSceneDirty(canvasObj.scene);
            EditorSceneManager.SaveScene(canvasObj.scene);
            Debug.Log("[CashOutSummarySetup] Cash Out Reward Summary Screen successfully built, wired into GameManager, and saved to scene!");
        }

        public static CashOutSummaryCardView EnsureCardPrefab(string prefabPath, Sprite cardSprite, bool isGold, bool forceRecreate = false)
        {
            if (!forceRecreate)
            {
                CashOutSummaryCardView existing = AssetDatabase.LoadAssetAtPath<CashOutSummaryCardView>(prefabPath);
                if (existing != null)
                {
                    return existing;
                }
            }

            string dir = System.IO.Path.GetDirectoryName(prefabPath);
            if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            string name = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
            GameObject cardObj = new GameObject(name, typeof(RectTransform));
            RectTransform cardRT = cardObj.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(210f, 280f);

            Image bgImg = cardObj.AddComponent<Image>();
            bgImg.sprite = cardSprite;
            bgImg.color = Color.white;
            bgImg.preserveAspect = true;
            bgImg.raycastTarget = true;

            // Card Header Text - precisely centered in top compartment
            Color textColor = isGold ? new Color(1f, 0.85f, 0.35f, 1f) : Color.white;
            TMP_Text nameText = CreateTMPText("ui_text_name", cardObj.transform, isGold ? "GOLD REWARD" : "REWARD", 16f, FontStyles.Bold, TextAlignmentOptions.Center, autoSize: true, minSize: 11f, maxSize: 16f);
            RectTransform nameRT = nameText.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0f, 1f);
            nameRT.anchorMax = new Vector2(1f, 1f);
            nameRT.pivot = new Vector2(0.5f, 0.5f);
            nameRT.anchoredPosition = new Vector2(0f, -40f);
            nameRT.sizeDelta = new Vector2(-28f, 26f);
            nameText.color = textColor;

            // Card Center Icon - prominent in spotlight/sunburst
            GameObject iconObj = CreateUIObject("ui_image_icon", cardObj.transform);
            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = new Vector2(0f, -2f);
            iconRT.sizeDelta = new Vector2(145f, 125f);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Card Bottom Amount Text - precisely centered in bottom compartment
            TMP_Text amountText = CreateTMPText("ui_text_amount", cardObj.transform, "x10", 22f, FontStyles.Bold, TextAlignmentOptions.Center, autoSize: true, minSize: 15f, maxSize: 22f);
            RectTransform amountRT = amountText.GetComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0f, 0f);
            amountRT.anchorMax = new Vector2(1f, 0f);
            amountRT.pivot = new Vector2(0.5f, 0.5f);
            amountRT.anchoredPosition = new Vector2(0f, 35f);
            amountRT.sizeDelta = new Vector2(-28f, 28f);
            amountText.color = textColor;

            CashOutSummaryCardView cardView = cardObj.AddComponent<CashOutSummaryCardView>();
            SerializedObject so = new SerializedObject(cardView);
            so.FindProperty("ui_text_name").objectReferenceValue = nameText;
            so.FindProperty("ui_image_icon").objectReferenceValue = iconImg;
            so.FindProperty("ui_text_amount").objectReferenceValue = amountText;
            so.FindProperty("ui_image_bg").objectReferenceValue = bgImg;
            so.FindProperty("overrideTextColorFromReward").boolValue = false;
            so.FindProperty("silverCardSprite").objectReferenceValue = cardSprite;
            so.FindProperty("goldCardSprite").objectReferenceValue = cardSprite;
            so.ApplyModifiedProperties();

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(cardObj, prefabPath);
            Object.DestroyImmediate(cardObj);

            Debug.Log($"[CashOutSummarySetup] Created reward card prefab at: {prefabPath}");
            return savedPrefab.GetComponent<CashOutSummaryCardView>();
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
            TextAlignmentOptions alignment,
            bool autoSize = false,
            float minSize = 12f,
            float maxSize = 18f)
        {
            GameObject obj = CreateUIObject(name, parent);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            tmp.maskable = true;
            if (autoSize)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = minSize;
                tmp.fontSizeMax = maxSize;
            }
            return tmp;
        }
    }
}
