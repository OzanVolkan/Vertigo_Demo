using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.State;
using Vertigo.UI.Panels;

namespace Vertigo.Editor
{
    public static class BombPopupSetupUtility
    {
        private const string TrashIconPrefKey = "Vertigo_BombPopup_GiveUpTrashIcon_V1";
        private const string CircularBombPrefKey = "Vertigo_BombPopup_CircularCenterpiece_V1";
        private const string RedVignettePrefKey = "Vertigo_BombPopup_FourCornerVignette_V1";
        private const string ThreeButtonsPrefKey = "Vertigo_BombPopup_ThreeButtons_V1";
        private const string TrashIconPath = "Assets/2D/demo_content/ui_icon_trash.png";
        private const string BombCircleBgPath = "Assets/2D/demo_content/ui_bomb_circle_bg.png";
        private const string RedVignettePath = "Assets/2D/demo_content/ui_red_vignette.png";
        private const string VideoAdIconPath = "Assets/2D/demo_content/ui_icon_video_ad.png";

        [MenuItem("Vertigo/UI/Apply Bomb Popup 3 Buttons (Give Up + Coin Revive + Ad Revive)")]
        public static void ApplyThreeBombButtons()
        {
            ApplyThreeBombButtonsToExistingScene();
        }

        [MenuItem("Vertigo/UI/Apply 4-Corner Red Vignette")]
        public static void ApplyFourCornerRedVignette()
        {
            ApplyFourCornerRedVignetteToExistingScene();
        }

        [MenuItem("Vertigo/UI/Apply Circular Bomb Centerpiece")]
        public static void ApplyCircularBombCenterpiece()
        {
            ApplyCircularBombCenterpieceToExistingScene();
        }

        [MenuItem("Vertigo/UI/Apply Give Up Button Trash Icon")]
        public static void ApplyGiveUpButtonTrashIcon()
        {
            ApplyGiveUpTrashIconToExistingScene();
        }

        [MenuItem("Vertigo/UI/Setup Bomb Popup View (Full Rebuild)")]
        public static void SetupBombPopup()
        {
            SetupBombPopupInternal();
        }

        [InitializeOnLoadMethod]
        private static void AutoSetupOnLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            bool needsTrash = !EditorPrefs.GetBool(TrashIconPrefKey, false);
            bool needsCircular = !EditorPrefs.GetBool(CircularBombPrefKey, false);
            bool needsVignette = !EditorPrefs.GetBool(RedVignettePrefKey, false);
            bool needsThreeButtons = !EditorPrefs.GetBool(ThreeButtonsPrefKey, false);

            if (needsTrash || needsCircular || needsVignette || needsThreeButtons)
            {
                EditorPrefs.SetBool(TrashIconPrefKey, true);
                EditorPrefs.SetBool(CircularBombPrefKey, true);
                EditorPrefs.SetBool(RedVignettePrefKey, true);
                EditorPrefs.SetBool(ThreeButtonsPrefKey, true);
                EditorApplication.delayCall += () =>
                {
                    if (needsTrash) ApplyGiveUpTrashIconToExistingScene();
                    if (needsCircular) ApplyCircularBombCenterpieceToExistingScene();
                    if (needsVignette) ApplyFourCornerRedVignetteToExistingScene();
                    if (needsThreeButtons) ApplyThreeBombButtonsToExistingScene();
                };
            }
        }

        private static Sprite EnsureSpriteImported(string assetPath)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null && (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        /// <summary>
        /// Surgically updates ONLY 'ui_image_top_red_vignette' inside 'ui_panel_bomb_popup'
        /// to use the 4-corner full-screen red vignette sprite, preserving all user scene layout changes.
        /// </summary>
        public static void ApplyFourCornerRedVignetteToExistingScene()
        {
            GameObject canvasObj = GameObject.Find("ui_canvas_main");
            if (canvasObj == null) return;

            Transform bombPopupTr = canvasObj.transform.Find("ui_panel_bomb_popup");
            if (bombPopupTr == null) return;

            Transform vignetteTr = bombPopupTr.Find("ui_image_top_red_vignette");
            if (vignetteTr == null)
            {
                Debug.LogWarning("[BombPopupSetup] 'ui_image_top_red_vignette' not found inside 'ui_panel_bomb_popup'.");
                return;
            }

            Sprite redVignetteSprite = EnsureSpriteImported(RedVignettePath);
            ConfigureFourCornerVignette(vignetteTr.gameObject, redVignetteSprite);

            EditorUtility.SetDirty(vignetteTr.gameObject);
            EditorSceneManager.MarkSceneDirty(canvasObj.scene);
            EditorSceneManager.SaveScene(canvasObj.scene);

            Debug.Log("[BombPopupSetup] Applied 4-corner red vignette to 'ui_image_top_red_vignette'!");
        }

        private static void ConfigureFourCornerVignette(GameObject vignetteObj, Sprite redVignetteSprite)
        {
            RectTransform rt = vignetteObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = vignetteObj.GetComponent<Image>();
            if (img == null) img = vignetteObj.AddComponent<Image>();
            img.sprite = redVignetteSprite;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
            img.color = new Color(1f, 1f, 1f, 0.78f);
            img.raycastTarget = false;
            img.maskable = false;
        }

        /// <summary>
        /// Surgically replaces ONLY the rectangular card visuals inside ui_popup_box with the
        /// circular penalty medallion + red glow + rotating danger rays, preserving all user scene layout changes.
        /// </summary>
        public static void ApplyCircularBombCenterpieceToExistingScene()
        {
            GameObject canvasObj = GameObject.Find("ui_canvas_main");
            if (canvasObj == null) return;

            Transform bombPopupTr = canvasObj.transform.Find("ui_panel_bomb_popup");
            if (bombPopupTr == null) return;

            Transform boxTr = bombPopupTr.Find("ui_popup_box");
            if (boxTr == null)
            {
                Debug.LogWarning("[BombPopupSetup] 'ui_popup_box' not found inside 'ui_panel_bomb_popup'.");
                return;
            }

            Sprite bombIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_card_icon_death.png");
            Sprite glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_glow_circle.png");
            Sprite raysSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_sunburst_rays.png");
            Sprite circleBgSprite = EnsureSpriteImported(BombCircleBgPath);

            ConfigureCircularBombCenterpiece(
                boxTr.gameObject,
                bombIconSprite,
                glowSprite,
                raysSprite,
                circleBgSprite,
                out Image redGlowImg,
                out RectTransform dangerRaysRT,
                out Image circleBgImg,
                out RectTransform bombIconRT);

            BombPopupView popupView = bombPopupTr.GetComponent<BombPopupView>();
            if (popupView != null)
            {
                SerializedObject bpSO = new SerializedObject(popupView);
                bpSO.FindProperty("ui_image_red_glow").objectReferenceValue = redGlowImg;
                bpSO.FindProperty("ui_transform_danger_rays").objectReferenceValue = dangerRaysRT;
                bpSO.FindProperty("ui_image_circle_bg").objectReferenceValue = circleBgImg;
                bpSO.FindProperty("ui_transform_bomb_icon").objectReferenceValue = bombIconRT;
                bpSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(popupView);
            }

            EditorUtility.SetDirty(boxTr.gameObject);
            EditorSceneManager.MarkSceneDirty(canvasObj.scene);
            EditorSceneManager.SaveScene(canvasObj.scene);

            Debug.Log("[BombPopupSetup] Replaced rectangular bomb card with circular penalty medallion & crimson glow!");
        }

        private static void ConfigureCircularBombCenterpiece(
            GameObject cardContainer,
            Sprite bombIconSprite,
            Sprite glowSprite,
            Sprite raysSprite,
            Sprite circleBgSprite,
            out Image redGlowImg,
            out RectTransform dangerRaysRT,
            out Image circleBgImg,
            out RectTransform bombIconRT)
        {
            // Remove rectangular card children if present
            string[] obsoleteCardChildren =
            {
                "ui_image_card_outer_glow",
                "ui_image_card_bg",
                "ui_image_card_border"
            };
            foreach (string childName in obsoleteCardChildren)
            {
                Transform oldChild = cardContainer.transform.Find(childName);
                if (oldChild != null)
                {
                    Object.DestroyImmediate(oldChild.gameObject);
                }
            }

            // If cardContainer itself had a rectangular frame Image, disable/remove it
            if (cardContainer.TryGetComponent(out Image rootBoxImg))
            {
                Object.DestroyImmediate(rootBoxImg);
            }

            RectTransform cardRT = cardContainer.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(320f, 320f);

            // 1. Rotating Blood-Red Danger Rays (Behind everything)
            Transform raysTr = cardContainer.transform.Find("ui_transform_danger_rays");
            GameObject raysObj = raysTr != null ? raysTr.gameObject : CreateUIObject("ui_transform_danger_rays", cardContainer.transform);
            dangerRaysRT = raysObj.GetComponent<RectTransform>();
            dangerRaysRT.anchoredPosition = Vector2.zero;
            dangerRaysRT.sizeDelta = new Vector2(580f, 580f);
            Image raysImg = raysObj.GetComponent<Image>();
            if (raysImg == null) raysImg = raysObj.AddComponent<Image>();
            raysImg.sprite = raysSprite;
            raysImg.color = new Color(0.95f, 0.10f, 0.10f, 0.34f);
            raysImg.preserveAspect = true;
            raysImg.raycastTarget = false;
            raysImg.maskable = false;

            // 2. Large Pulsing Crimson Aura Glow
            Transform glowTr = cardContainer.transform.Find("ui_image_red_glow");
            GameObject redGlowObj = glowTr != null ? glowTr.gameObject : CreateUIObject("ui_image_red_glow", cardContainer.transform);
            RectTransform redGlowRT = redGlowObj.GetComponent<RectTransform>();
            redGlowRT.anchoredPosition = Vector2.zero;
            redGlowRT.sizeDelta = new Vector2(500f, 500f);
            redGlowImg = redGlowObj.GetComponent<Image>();
            if (redGlowImg == null) redGlowImg = redGlowObj.AddComponent<Image>();
            redGlowImg.sprite = glowSprite;
            redGlowImg.color = new Color(1f, 0.08f, 0.08f, 0.82f);
            redGlowImg.preserveAspect = true;
            redGlowImg.raycastTarget = false;
            redGlowImg.maskable = false;

            // 3. Circular Dark Penalty Medallion with Glowing Crimson Rim
            Transform circleTr = cardContainer.transform.Find("ui_image_circle_bg");
            GameObject circleObj = circleTr != null ? circleTr.gameObject : CreateUIObject("ui_image_circle_bg", cardContainer.transform);
            RectTransform circleRT = circleObj.GetComponent<RectTransform>();
            circleRT.anchoredPosition = Vector2.zero;
            circleRT.sizeDelta = new Vector2(310f, 310f);
            circleBgImg = circleObj.GetComponent<Image>();
            if (circleBgImg == null) circleBgImg = circleObj.AddComponent<Image>();
            circleBgImg.sprite = circleBgSprite;
            circleBgImg.color = Color.white;
            circleBgImg.preserveAspect = true;
            circleBgImg.raycastTarget = false;
            circleBgImg.maskable = false;

            // 4. Bomb Icon Centered Inside the Circular Medallion
            Transform iconTr = cardContainer.transform.Find("ui_image_bomb_icon");
            GameObject bombIconObj = iconTr != null ? iconTr.gameObject : CreateUIObject("ui_image_bomb_icon", cardContainer.transform);
            bombIconRT = bombIconObj.GetComponent<RectTransform>();
            bombIconRT.anchoredPosition = new Vector2(0f, 4f);
            bombIconRT.sizeDelta = new Vector2(210f, 210f);
            Image bombIconImg = bombIconObj.GetComponent<Image>();
            if (bombIconImg == null) bombIconImg = bombIconObj.AddComponent<Image>();
            if (bombIconImg.sprite == null) bombIconImg.sprite = bombIconSprite;
            bombIconImg.preserveAspect = true;
            bombIconImg.raycastTarget = false;
            bombIconImg.maskable = false;

            // Enforce correct layering inside ui_popup_box
            raysObj.transform.SetSiblingIndex(0);
            redGlowObj.transform.SetSiblingIndex(1);
            circleObj.transform.SetSiblingIndex(2);
            bombIconObj.transform.SetSiblingIndex(3);
        }

        /// <summary>
        /// Surgically updates ONLY 'ui_transform_buttons' inside 'ui_panel_bomb_popup' to the 3-button layout:
        /// 1. Left: Grey 'GIVE UP' button with trash icon
        /// 2. Middle: Green 'REVIVE' button with gold coin + cost ('25')
        /// 3. Right: Blue 'REVIVE' button with video film-strip play icon
        /// Preserves 'ui_transform_buttons.anchoredPosition' and all other scene objects!
        /// </summary>
        public static void ApplyThreeBombButtonsToExistingScene()
        {
            GameObject canvasObj = GameObject.Find("ui_canvas_main");
            if (canvasObj == null) return;

            Transform bombPopupTr = canvasObj.transform.Find("ui_panel_bomb_popup");
            if (bombPopupTr == null) return;

            Transform buttonsTr = bombPopupTr.Find("ui_transform_buttons");
            if (buttonsTr == null)
            {
                Debug.LogWarning("[BombPopupSetup] 'ui_transform_buttons' not found inside 'ui_panel_bomb_popup'.");
                return;
            }

            Sprite buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/UI_button_grey_standard.png");
            Sprite goldCoinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/UI_icon_gold.png");
            Sprite trashSprite = EnsureSpriteImported(TrashIconPath);
            Sprite videoAdSprite = EnsureSpriteImported(VideoAdIconPath);

            ConfigureThreeBombButtons(
                buttonsTr.gameObject,
                buttonSprite,
                trashSprite,
                goldCoinSprite,
                videoAdSprite,
                out Button giveUpBtn,
                out Button reviveBtn,
                out Button reviveAdBtn,
                out TMP_Text reviveCostText);

            BombPopupView popupView = bombPopupTr.GetComponent<BombPopupView>();
            if (popupView != null)
            {
                SerializedObject bpSO = new SerializedObject(popupView);
                bpSO.FindProperty("ui_button_giveup").objectReferenceValue = giveUpBtn;
                bpSO.FindProperty("ui_button_revive").objectReferenceValue = reviveBtn;
                bpSO.FindProperty("ui_button_revive_ad").objectReferenceValue = reviveAdBtn;
                bpSO.FindProperty("ui_text_revive_cost_value").objectReferenceValue = reviveCostText;
                bpSO.FindProperty("reviveCost").intValue = 25;
                bpSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(popupView);
            }

            EditorUtility.SetDirty(buttonsTr.gameObject);
            EditorSceneManager.MarkSceneDirty(canvasObj.scene);
            EditorSceneManager.SaveScene(canvasObj.scene);

            Debug.Log("[BombPopupSetup] Updated 'ui_transform_buttons' with 3 buttons (Give Up, Coin Revive, Video Ad Revive) while preserving scene layout!");
        }

        private static void ConfigureThreeBombButtons(
            GameObject buttonsContainer,
            Sprite buttonSprite,
            Sprite trashSprite,
            Sprite goldCoinSprite,
            Sprite videoAdSprite,
            out Button giveUpBtn,
            out Button reviveBtn,
            out Button reviveAdBtn,
            out TMP_Text reviveCostText)
        {
            RectTransform buttonsRT = buttonsContainer.GetComponent<RectTransform>();
            buttonsRT.sizeDelta = new Vector2(700f, 84f);

            // Clear existing children inside ui_transform_buttons so we build the clean 3-button row
            for (int i = buttonsContainer.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(buttonsContainer.transform.GetChild(i).gameObject);
            }

            Vector2 btnSize = new Vector2(218f, 68f);

            // =========================================================================
            // 1. LEFT: GIVE UP Button (Silver/Grey + Trash Icon + "GIVE UP")
            // =========================================================================
            GameObject giveUpBtnObj = CreateUIObject("ui_button_giveup", buttonsContainer.transform);
            RectTransform giveUpRT = giveUpBtnObj.GetComponent<RectTransform>();
            giveUpRT.anchoredPosition = new Vector2(-238f, 0f);
            giveUpRT.sizeDelta = btnSize;

            Image giveUpImg = giveUpBtnObj.AddComponent<Image>();
            giveUpImg.sprite = buttonSprite;
            giveUpImg.type = Image.Type.Sliced;
            giveUpImg.color = new Color(0.70f, 0.74f, 0.82f, 1f);

            giveUpBtn = giveUpBtnObj.AddComponent<Button>();
            giveUpBtn.targetGraphic = giveUpImg;

            TMP_Text giveUpText = CreateTMPText(
                "ui_text_label",
                giveUpBtnObj.transform,
                "GIVE UP",
                21f,
                FontStyles.Bold,
                TextAlignmentOptions.Center);
            giveUpText.color = Color.white;
            ConfigureGiveUpButtonContent(giveUpBtnObj, trashSprite);

            // =========================================================================
            // 2. CENTER: COIN REVIVE Button (Green + Gold Coin "25" + "REVIVE")
            // =========================================================================
            GameObject reviveBtnObj = CreateUIObject("ui_button_revive", buttonsContainer.transform);
            RectTransform reviveRT = reviveBtnObj.GetComponent<RectTransform>();
            reviveRT.anchoredPosition = new Vector2(0f, 0f);
            reviveRT.sizeDelta = btnSize;

            Image reviveImg = reviveBtnObj.AddComponent<Image>();
            reviveImg.sprite = buttonSprite;
            reviveImg.type = Image.Type.Sliced;
            reviveImg.color = new Color(0.32f, 0.78f, 0.08f, 1f);

            reviveBtn = reviveBtnObj.AddComponent<Button>();
            reviveBtn.targetGraphic = reviveImg;

            // Gold Coin Icon inside Revive Button (top row)
            GameObject coinIconObj = CreateUIObject("ui_image_revive_coin", reviveBtnObj.transform);
            RectTransform coinRT = coinIconObj.GetComponent<RectTransform>();
            coinRT.anchoredPosition = new Vector2(-20f, 13f);
            coinRT.sizeDelta = new Vector2(25f, 25f);
            Image coinImg = coinIconObj.AddComponent<Image>();
            coinImg.sprite = goldCoinSprite;
            coinImg.preserveAspect = true;
            coinImg.raycastTarget = false;
            coinImg.maskable = false;

            // Cost Value Text ("25")
            reviveCostText = CreateTMPText(
                "ui_text_revive_cost_value",
                reviveBtnObj.transform,
                "25",
                21f,
                FontStyles.Bold,
                TextAlignmentOptions.Left);
            reviveCostText.color = Color.white;
            reviveCostText.rectTransform.anchoredPosition = new Vector2(22f, 13f);
            reviveCostText.rectTransform.sizeDelta = new Vector2(64f, 26f);

            // "REVIVE" Label (bottom row)
            TMP_Text reviveLabelText = CreateTMPText(
                "ui_text_label",
                reviveBtnObj.transform,
                "REVIVE",
                20f,
                FontStyles.Bold,
                TextAlignmentOptions.Center);
            reviveLabelText.color = Color.white;
            reviveLabelText.rectTransform.anchoredPosition = new Vector2(0f, -13f);
            reviveLabelText.rectTransform.sizeDelta = new Vector2(190f, 26f);

            // =========================================================================
            // 3. RIGHT: VIDEO AD REVIVE Button (Blue + "REVIVE" + Film-Strip Play Icon)
            // =========================================================================
            GameObject reviveAdBtnObj = CreateUIObject("ui_button_revive_ad", buttonsContainer.transform);
            RectTransform reviveAdRT = reviveAdBtnObj.GetComponent<RectTransform>();
            reviveAdRT.anchoredPosition = new Vector2(238f, 0f);
            reviveAdRT.sizeDelta = btnSize;

            Image reviveAdImg = reviveAdBtnObj.AddComponent<Image>();
            reviveAdImg.sprite = buttonSprite;
            reviveAdImg.type = Image.Type.Sliced;
            reviveAdImg.color = new Color(0.24f, 0.55f, 0.98f, 1f);

            reviveAdBtn = reviveAdBtnObj.AddComponent<Button>();
            reviveAdBtn.targetGraphic = reviveAdImg;

            // "REVIVE" Label on the left side of the blue button
            TMP_Text reviveAdLabelText = CreateTMPText(
                "ui_text_label",
                reviveAdBtnObj.transform,
                "REVIVE",
                21f,
                FontStyles.Bold,
                TextAlignmentOptions.Center);
            reviveAdLabelText.color = Color.white;
            reviveAdLabelText.rectTransform.anchoredPosition = new Vector2(-28f, 0f);
            reviveAdLabelText.rectTransform.sizeDelta = new Vector2(124f, 44f);

            // Video Ad Film-Strip Play Icon on the right side of the blue button
            GameObject videoIconObj = CreateUIObject("ui_image_video_icon", reviveAdBtnObj.transform);
            RectTransform videoIconRT = videoIconObj.GetComponent<RectTransform>();
            videoIconRT.anchoredPosition = new Vector2(54f, 0f);
            videoIconRT.sizeDelta = new Vector2(48f, 38f);
            Image videoIconImg = videoIconObj.AddComponent<Image>();
            videoIconImg.sprite = videoAdSprite;
            videoIconImg.color = Color.white;
            videoIconImg.preserveAspect = true;
            videoIconImg.raycastTarget = false;
            videoIconImg.maskable = false;
        }

        /// <summary>
        /// Surgically adds the trash can icon to the existing ui_button_giveup in the scene
        /// WITHOUT touching or resetting any other user UI adjustments.
        /// </summary>
        public static void ApplyGiveUpTrashIconToExistingScene()
        {
            GameObject canvasObj = GameObject.Find("ui_canvas_main");
            if (canvasObj == null) return;

            Transform bombPopupTr = canvasObj.transform.Find("ui_panel_bomb_popup");
            if (bombPopupTr == null) return;

            Button giveUpBtn = FindButtonRecursive(bombPopupTr, "ui_button_giveup");
            if (giveUpBtn == null)
            {
                Debug.LogWarning("[BombPopupSetup] 'ui_button_giveup' not found inside 'ui_panel_bomb_popup'.");
                return;
            }

            Sprite trashSprite = EnsureSpriteImported(TrashIconPath);
            ConfigureGiveUpButtonContent(giveUpBtn.gameObject, trashSprite);

            EditorUtility.SetDirty(giveUpBtn.gameObject);
            EditorSceneManager.MarkSceneDirty(canvasObj.scene);
            EditorSceneManager.SaveScene(canvasObj.scene);

            Debug.Log("[BombPopupSetup] Added trash icon to 'ui_button_giveup' while preserving all scene UI adjustments!");
        }

        private static void ConfigureGiveUpButtonContent(GameObject giveUpBtnObj, Sprite trashSprite)
        {
            // 1. Trash Can Icon on the left
            Transform iconTr = giveUpBtnObj.transform.Find("ui_image_giveup_icon");
            GameObject iconObj = iconTr != null ? iconTr.gameObject : CreateUIObject("ui_image_giveup_icon", giveUpBtnObj.transform);

            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = new Vector2(-58f, 1f);
            iconRT.sizeDelta = new Vector2(36f, 36f);

            Image iconImg = iconObj.GetComponent<Image>();
            if (iconImg == null) iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = trashSprite;
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            iconImg.maskable = false;

            // 2. Shift "GIVE UP" label slightly right so icon and text are balanced like the reference
            Transform labelTr = giveUpBtnObj.transform.Find("ui_text_label");
            if (labelTr == null)
            {
                TMP_Text anyTmp = giveUpBtnObj.GetComponentInChildren<TMP_Text>(true);
                if (anyTmp != null) labelTr = anyTmp.transform;
            }

            if (labelTr != null && labelTr.TryGetComponent(out RectTransform labelRT))
            {
                labelRT.anchorMin = Vector2.zero;
                labelRT.anchorMax = Vector2.one;
                labelRT.offsetMin = new Vector2(32f, 0f);
                labelRT.offsetMax = Vector2.zero;
            }
        }

        private static Button FindButtonRecursive(Transform parent, string targetName)
        {
            if (parent.name == targetName && parent.TryGetComponent(out Button b))
                return b;

            for (int i = 0; i < parent.childCount; i++)
            {
                Button found = FindButtonRecursive(parent.GetChild(i), targetName);
                if (found != null) return found;
            }
            return null;
        }

        private static void SetupBombPopupInternal()
        {
            GameObject canvasObj = GameObject.Find("ui_canvas_main");
            if (canvasObj == null)
            {
                Debug.LogWarning("[BombPopupSetup] 'ui_canvas_main' not found!");
                return;
            }

            // Load Sprites
            Sprite bombIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_card_icon_death.png");
            Sprite bannerFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_zonebar_frame.png");
            Sprite glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_glow_circle.png");
            Sprite raysSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_sunburst_rays.png");
            Sprite buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/UI_button_grey_standard.png");
            Sprite goldCoinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/UI_icon_gold.png");
            Sprite trashSprite = EnsureSpriteImported(TrashIconPath);
            Sprite circleBgSprite = EnsureSpriteImported(BombCircleBgPath);
            Sprite videoAdSprite = EnsureSpriteImported(VideoAdIconPath);

            // Destroy old bomb popup if present so we build a clean hierarchy
            Transform existingTr = canvasObj.transform.Find("ui_panel_bomb_popup");
            if (existingTr != null)
            {
                Object.DestroyImmediate(existingTr.gameObject);
            }

            GameObject rootObj = CreateUIObject("ui_panel_bomb_popup", canvasObj.transform);
            RectTransform rootRT = rootObj.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.pivot = new Vector2(0.5f, 0.5f);
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // Ensure it renders at the top of the canvas
            rootObj.transform.SetAsLastSibling();

            CanvasGroup canvasGroup = rootObj.AddComponent<CanvasGroup>();

            // 1. Dark Dimmer Overlay
            Image dimmerImg = rootObj.AddComponent<Image>();
            dimmerImg.color = new Color(0.03f, 0.02f, 0.03f, 0.85f);
            dimmerImg.raycastTarget = true;

            // 2. 4-Corner Red Danger Atmosphere Vignette
            GameObject topVignetteObj = CreateUIObject("ui_image_top_red_vignette", rootObj.transform);
            ConfigureFourCornerVignette(topVignetteObj, EnsureSpriteImported(RedVignettePath));

            float centerX = 75f; // Aligned with wheel & zone bar horizontal center

            // =========================================================================
            // 3. TOP HEADER BANNER ("OH NO, A BOMB EXPLODED RIGHT IN YOUR HANDS!")
            // =========================================================================
            GameObject headerContainer = CreateUIObject("ui_transform_header", rootObj.transform);
            RectTransform headerRT = headerContainer.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0.5f, 0.5f);
            headerRT.anchorMax = new Vector2(0.5f, 0.5f);
            headerRT.pivot = new Vector2(0.5f, 0.5f);
            headerRT.anchoredPosition = new Vector2(centerX, 330f);
            headerRT.sizeDelta = new Vector2(880f, 110f);

            GameObject bannerObj = CreateUIObject("ui_image_header_banner", headerContainer.transform);
            RectTransform bannerRT = bannerObj.GetComponent<RectTransform>();
            bannerRT.anchoredPosition = new Vector2(0f, 18f);
            bannerRT.sizeDelta = new Vector2(860f, 56f);
            Image bannerImg = bannerObj.AddComponent<Image>();
            bannerImg.sprite = bannerFrameSprite;
            bannerImg.type = bannerFrameSprite != null && bannerFrameSprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            bannerImg.color = new Color(0.11f, 0.11f, 0.14f, 0.95f);
            bannerImg.raycastTarget = false;
            bannerImg.maskable = false;

            TMP_Text titleText = CreateTMPText(
                "ui_text_bomb_title",
                bannerObj.transform,
                "OH NO, A BOMB EXPLODED RIGHT IN YOUR HANDS!",
                25f,
                FontStyles.Bold,
                TextAlignmentOptions.Center);
            titleText.color = Color.white;
            titleText.rectTransform.anchorMin = Vector2.zero;
            titleText.rectTransform.anchorMax = Vector2.one;
            titleText.rectTransform.offsetMin = new Vector2(16f, 2f);
            titleText.rectTransform.offsetMax = new Vector2(-16f, -2f);

            TMP_Text subtitleText = CreateTMPText(
                "ui_text_bomb_desc",
                headerContainer.transform,
                "Revive yourself to keep your rewards.",
                19f,
                FontStyles.Normal,
                TextAlignmentOptions.Center);
            subtitleText.color = new Color(0.92f, 0.94f, 0.96f, 1f);
            subtitleText.rectTransform.anchoredPosition = new Vector2(0f, -30f);
            subtitleText.rectTransform.sizeDelta = new Vector2(680f, 34f);

            // =========================================================================
            // 4. CENTER CIRCULAR PENALTY MEDALLION & RED GLOW
            // =========================================================================
            GameObject cardContainer = CreateUIObject("ui_popup_box", rootObj.transform);
            RectTransform cardRT = cardContainer.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.anchoredPosition = new Vector2(centerX, 15f);

            ConfigureCircularBombCenterpiece(
                cardContainer,
                bombIconSprite,
                glowSprite,
                raysSprite,
                circleBgSprite,
                out Image redGlowImg,
                out RectTransform dangerRaysRT,
                out Image circleBgImg,
                out RectTransform bombIconRT);

            // =========================================================================
            // 5. BOTTOM BUTTONS (GIVE UP, COIN REVIVE & VIDEO AD REVIVE)
            // =========================================================================
            GameObject buttonsContainer = CreateUIObject("ui_transform_buttons", rootObj.transform);
            RectTransform buttonsRT = buttonsContainer.GetComponent<RectTransform>();
            buttonsRT.anchorMin = new Vector2(0.5f, 0.5f);
            buttonsRT.anchorMax = new Vector2(0.5f, 0.5f);
            buttonsRT.pivot = new Vector2(0.5f, 0.5f);
            buttonsRT.anchoredPosition = new Vector2(centerX, -260f);

            ConfigureThreeBombButtons(
                buttonsContainer,
                buttonSprite,
                trashSprite,
                goldCoinSprite,
                videoAdSprite,
                out Button giveUpBtn,
                out Button reviveBtn,
                out Button reviveAdBtn,
                out TMP_Text reviveCostText);

            // =========================================================================
            // 6. WIRE BOMBPOPUPVIEW & GAMEMANAGER
            // =========================================================================
            BombPopupView popupView = rootObj.AddComponent<BombPopupView>();
            SerializedObject bpSO = new SerializedObject(popupView);
            bpSO.FindProperty("ui_panel_popup_root").objectReferenceValue = rootObj;
            bpSO.FindProperty("ui_canvas_group").objectReferenceValue = canvasGroup;
            bpSO.FindProperty("ui_transform_header").objectReferenceValue = headerRT;
            bpSO.FindProperty("ui_transform_box").objectReferenceValue = cardRT;
            bpSO.FindProperty("ui_transform_buttons").objectReferenceValue = buttonsRT;
            bpSO.FindProperty("ui_image_red_glow").objectReferenceValue = redGlowImg;
            bpSO.FindProperty("ui_transform_danger_rays").objectReferenceValue = dangerRaysRT;
            bpSO.FindProperty("ui_image_circle_bg").objectReferenceValue = circleBgImg;
            bpSO.FindProperty("ui_transform_bomb_icon").objectReferenceValue = bombIconRT;
            bpSO.FindProperty("ui_text_revive_cost_value").objectReferenceValue = reviveCostText;
            bpSO.FindProperty("ui_button_giveup").objectReferenceValue = giveUpBtn;
            bpSO.FindProperty("ui_button_revive").objectReferenceValue = reviveBtn;
            bpSO.FindProperty("ui_button_revive_ad").objectReferenceValue = reviveAdBtn;
            bpSO.FindProperty("reviveCost").intValue = 25;
            bpSO.ApplyModifiedProperties();

            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                SerializedObject gmSO = new SerializedObject(gm);
                gmSO.FindProperty("bombPopupView").objectReferenceValue = popupView;
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

            Debug.Log("[BombPopupSetup] Successfully built circular-style BombPopupView in MainGame scene!");
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
