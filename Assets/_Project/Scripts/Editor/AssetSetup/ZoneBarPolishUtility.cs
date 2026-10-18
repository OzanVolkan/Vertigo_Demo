using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Zone.Views;

namespace Vertigo.Editor
{
    public static class ZoneBarPolishUtility
    {
        private const string PrefabDir = "Assets/_Project/Prefabs/UI/Components";
        private const string BadgePrefabPath = "Assets/_Project/Prefabs/UI/Components/ZoneCurrentBadge.prefab";

        [MenuItem("Vertigo/UI/Polish Zone Bar")]
        public static void PolishZoneBar()
        {
            PolishZoneBarInternal();
        }

        [InitializeOnLoadMethod]
        private static void AutoPolishOnLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorPrefs.GetBool("ZoneBarPolished_V6", false)) return;
            EditorPrefs.SetBool("ZoneBarPolished_V6", true);

            EditorApplication.delayCall += () =>
            {
                PolishZoneBarInternal();
            };
        }

        private static void PolishZoneBarInternal()
        {
            GameObject topBarObj = GameObject.Find("ui_panel_top_zonebar");
            if (topBarObj == null)
            {
                Debug.LogError("[ZoneBarPolish] Could not find 'ui_panel_top_zonebar' in scene!");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(topBarObj, "Polish Zone Bar (Sliding Badge)");

            // 1. Load Sprites
            Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/2D/Custom_UI/ui_zonebar_frame.png");
            Sprite badgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/2D/Custom_UI/ui_badge_current_zone.png");
            Sprite safeBadgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/2D/Custom_UI/ui_badge_current_zone_safe.png");
            Sprite superBadgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/2D/Custom_UI/ui_badge_current_zone_super.png");

            // 2. Ensure Badge Prefab exists
            GameObject badgePrefab = EnsureBadgePrefab(badgeSprite);

            // 3. Setup Bar Frame & Dimensions (15% scaled: 650x48 -> 748x55)
            RectTransform topBarRT = topBarObj.GetComponent<RectTransform>();
            topBarRT.sizeDelta = new Vector2(748, 55);
            topBarRT.anchoredPosition = new Vector2(0, -25);

            Image topBarBg = topBarObj.GetComponent<Image>();
            if (topBarBg != null)
            {
                topBarBg.sprite = frameSprite;
                topBarBg.type = Image.Type.Sliced;
                topBarBg.color = Color.white;
            }

            // 4. Setup or Find the Single Sliding Badge Object
            Transform badgeT = topBarObj.transform.Find("ui_badge_current_zone");
            GameObject badgeObj;
            if (badgeT == null)
            {
                if (badgePrefab != null)
                {
                    badgeObj = (GameObject)PrefabUtility.InstantiatePrefab(badgePrefab, topBarObj.transform);
                    badgeObj.name = "ui_badge_current_zone";
                }
                else
                {
                    badgeObj = new GameObject("ui_badge_current_zone", typeof(RectTransform));
                    badgeObj.transform.SetParent(topBarObj.transform, false);
                    Image bImg = badgeObj.AddComponent<Image>();
                    bImg.sprite = badgeSprite;
                    bImg.type = Image.Type.Simple;
                    bImg.preserveAspect = false;
                    bImg.color = Color.white;
                    bImg.raycastTarget = false;
                    bImg.maskable = false;
                }
            }
            else
            {
                badgeObj = badgeT.gameObject;
            }

            RectTransform badgeRT = badgeObj.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0.5f, 0.5f);
            badgeRT.anchorMax = new Vector2(0.5f, 0.5f);
            badgeRT.pivot = new Vector2(0.5f, 0.5f);
            badgeRT.sizeDelta = new Vector2(53, 62);

            // Ensure badge is sibling 0 so it renders behind the slot texts
            badgeObj.transform.SetSiblingIndex(0);

            // 5. Setup Container & Layout (15% scaled)
            Transform containerT = topBarObj.transform.Find("ui_container_zone_slots");
            if (containerT == null)
            {
                Debug.LogError("[ZoneBarPolish] Could not find 'ui_container_zone_slots'!");
                return;
            }

            // Container must be after badge in hierarchy so texts render on top
            containerT.SetSiblingIndex(1);

            RectTransform containerRT = containerT.GetComponent<RectTransform>();
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.offsetMin = new Vector2(11.5f, 0);
            containerRT.offsetMax = new Vector2(-11.5f, 0);

            HorizontalLayoutGroup hlg = containerT.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = containerT.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4.5f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // 6. Setup 13 Clean Text Slots (no individual badges)
            const int slotCount = 13;
            List<TMP_Text> slotTexts = new List<TMP_Text>();

            while (containerT.childCount > slotCount)
            {
                Object.DestroyImmediate(containerT.GetChild(containerT.childCount - 1).gameObject);
            }

            for (int i = 0; i < slotCount; i++)
            {
                Transform slotT = (i < containerT.childCount) ? containerT.GetChild(i) : null;
                GameObject slotObj;
                if (slotT == null)
                {
                    slotObj = new GameObject($"ui_slot_{i}", typeof(RectTransform));
                    slotObj.transform.SetParent(containerT, false);
                }
                else
                {
                    slotObj = slotT.gameObject;
                    slotObj.name = $"ui_slot_{i}";
                }

                // Remove legacy badge inside slot if present
                Transform legacyBadge = slotObj.transform.Find("ui_image_badge");
                if (legacyBadge != null)
                {
                    Object.DestroyImmediate(legacyBadge.gameObject);
                }

                Image directImg = slotObj.GetComponent<Image>();
                if (directImg != null)
                {
                    Object.DestroyImmediate(directImg);
                }

                // Slot Text
                Transform textT = slotObj.transform.Find("ui_text_slot_value");
                GameObject textObj;
                if (textT == null)
                {
                    textObj = new GameObject("ui_text_slot_value", typeof(RectTransform));
                    textObj.transform.SetParent(slotObj.transform, false);
                }
                else
                {
                    textObj = textT.gameObject;
                }

                RectTransform textRT = textObj.GetComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.pivot = new Vector2(0.5f, 0.5f);
                textRT.offsetMin = Vector2.zero;
                textRT.offsetMax = Vector2.zero;

                TMP_Text tmp = textObj.GetComponent<TextMeshProUGUI>();
                if (tmp == null) tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = (i + 1).ToString();
                tmp.fontSize = 28;
                tmp.fontStyle = (i == 0) ? FontStyles.Bold : FontStyles.Normal;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = (i == 0) ? new Color(0.08f, 0.08f, 0.10f, 1f) : Color.white;
                tmp.raycastTarget = false;
                tmp.maskable = false;

                slotTexts.Add(tmp);
            }

            // Force layout update so slot world positions are accurate immediately
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRT);

            // 7. Wire ZoneBarView
            ZoneBarView zoneBarView = topBarObj.GetComponent<ZoneBarView>();
            if (zoneBarView != null)
            {
                SerializedObject so = new SerializedObject(zoneBarView);

                so.FindProperty("ui_container_zone_slots").objectReferenceValue = containerRT;
                so.FindProperty("ui_transform_current_badge").objectReferenceValue = badgeRT;
                so.FindProperty("ui_image_current_badge").objectReferenceValue = badgeObj.GetComponent<Image>();
                so.FindProperty("badgeNormalSprite").objectReferenceValue = badgeSprite;
                so.FindProperty("badgeSafeSprite").objectReferenceValue = safeBadgeSprite;
                so.FindProperty("badgeSuperSprite").objectReferenceValue = superBadgeSprite;

                SerializedProperty textsProp = so.FindProperty("ui_slot_texts");
                textsProp.ClearArray();
                for (int i = 0; i < slotTexts.Count; i++)
                {
                    textsProp.InsertArrayElementAtIndex(i);
                    textsProp.GetArrayElementAtIndex(i).objectReferenceValue = slotTexts[i];
                }

                SerializedProperty bgsProp = so.FindProperty("ui_slot_backgrounds");
                bgsProp.ClearArray();

                so.ApplyModifiedProperties();

                // Snap badge and colors to Zone 1
                zoneBarView.SnapToZone(1);
            }

            EditorSceneManager.MarkSceneDirty(topBarObj.scene);
            EditorSceneManager.SaveScene(topBarObj.scene);
            Debug.Log("[ZoneBarPolish] Single Sliding Badge & Zone Bar configured successfully!");
        }

        private static GameObject EnsureBadgePrefab(Sprite badgeSprite)
        {
            if (!Directory.Exists(PrefabDir))
            {
                Directory.CreateDirectory(PrefabDir);
            }

            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(BadgePrefabPath);
            if (existing != null)
            {
                RectTransform exRT = existing.GetComponent<RectTransform>();
                if (exRT != null && exRT.sizeDelta != new Vector2(53, 62))
                {
                    exRT.sizeDelta = new Vector2(53, 62);
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }
                return existing;
            }

            GameObject temp = new GameObject("ZoneCurrentBadge", typeof(RectTransform));
            RectTransform rt = temp.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(53, 62);
            rt.pivot = new Vector2(0.5f, 0.5f);

            Image img = temp.AddComponent<Image>();
            img.sprite = badgeSprite;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
            img.color = Color.white;
            img.raycastTarget = false;
            img.maskable = false;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, BadgePrefabPath);
            Object.DestroyImmediate(temp);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ZoneBarPolish] Created sliding badge prefab at {BadgePrefabPath}");
            return prefab;
        }
    }
}
