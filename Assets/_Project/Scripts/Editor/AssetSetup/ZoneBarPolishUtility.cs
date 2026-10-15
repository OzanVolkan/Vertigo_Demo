using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Enums;
using Vertigo.Zone.Data;
using Vertigo.Zone.Views;

namespace Vertigo.Editor
{
    public static class ZoneBarPolishUtility
    {
        [MenuItem("Vertigo/UI/Polish Zone Bar")]
        public static void PolishZoneBar()
        {
            PolishZoneBarInternal();
        }

        [InitializeOnLoadMethod]
        private static void AutoPolishOnLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorPrefs.GetBool("ZoneBarPolished_V2", false)) return;
            EditorPrefs.SetBool("ZoneBarPolished_V2", true);

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

            Undo.RegisterFullObjectHierarchyUndo(topBarObj, "Polish Zone Bar");

            // 1. Sprites
            Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_zonebar_frame.png");
            Sprite badgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_badge_current_zone.png");

            // 2. Bar Dimensions & Frame
            RectTransform topBarRT = topBarObj.GetComponent<RectTransform>();
            topBarRT.sizeDelta = new Vector2(650, 48);
            topBarRT.anchoredPosition = new Vector2(0, -25);

            Image topBarBg = topBarObj.GetComponent<Image>();
            if (topBarBg != null)
            {
                topBarBg.sprite = frameSprite;
                topBarBg.type = Image.Type.Sliced;
                topBarBg.color = Color.white;
            }

            // 3. Container & Layout
            Transform containerT = topBarObj.transform.Find("ui_container_zone_slots");
            if (containerT == null)
            {
                Debug.LogError("[ZoneBarPolish] Could not find 'ui_container_zone_slots'!");
                return;
            }

            RectTransform containerRT = containerT.GetComponent<RectTransform>();
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.offsetMin = new Vector2(10, 0);
            containerRT.offsetMax = new Vector2(-10, 0);

            HorizontalLayoutGroup hlg = containerT.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = containerT.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // 4. Slots (13 slots matching Image 2 reference)
            const int slotCount = 13;
            List<TMP_Text> slotTexts = new List<TMP_Text>();
            List<Image> slotBgs = new List<Image>();

            // Destroy excess children if > 13
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

                // Remove Image directly on slot root if present
                Image directImg = slotObj.GetComponent<Image>();
                if (directImg != null)
                {
                    Object.DestroyImmediate(directImg);
                }

                // Child Badge Object (ui_image_badge)
                Transform badgeT = slotObj.transform.Find("ui_image_badge");
                GameObject badgeObj;
                if (badgeT == null)
                {
                    badgeObj = new GameObject("ui_image_badge", typeof(RectTransform));
                    badgeObj.transform.SetParent(slotObj.transform, false);
                }
                else
                {
                    badgeObj = badgeT.gameObject;
                }

                RectTransform badgeRT = badgeObj.GetComponent<RectTransform>();
                badgeRT.anchorMin = new Vector2(0.5f, 0.5f);
                badgeRT.anchorMax = new Vector2(0.5f, 0.5f);
                badgeRT.pivot = new Vector2(0.5f, 0.5f);
                badgeRT.anchoredPosition = Vector2.zero;
                badgeRT.sizeDelta = new Vector2(46, 54);

                Image badgeImg = badgeObj.GetComponent<Image>();
                if (badgeImg == null) badgeImg = badgeObj.AddComponent<Image>();
                badgeImg.sprite = badgeSprite;
                badgeImg.type = Image.Type.Simple;
                badgeImg.preserveAspect = false;
                badgeImg.color = Color.white;
                badgeImg.raycastTarget = false;
                badgeImg.maskable = false;
                badgeImg.enabled = (i == 0);

                slotBgs.Add(badgeImg);

                // Child Text Object (ui_text_slot_value)
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

                textObj.transform.SetAsLastSibling();

                RectTransform textRT = textObj.GetComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.pivot = new Vector2(0.5f, 0.5f);
                textRT.offsetMin = Vector2.zero;
                textRT.offsetMax = Vector2.zero;

                TMP_Text tmp = textObj.GetComponent<TextMeshProUGUI>();
                if (tmp == null) tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = (i + 1).ToString();
                tmp.fontSize = 24;
                tmp.fontStyle = (i == 0) ? FontStyles.Bold : FontStyles.Normal;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = (i == 0) ? new Color(0.08f, 0.08f, 0.10f, 1f) : Color.white;
                tmp.raycastTarget = false;
                tmp.maskable = false;

                slotTexts.Add(tmp);
            }

            // 5. Wire ZoneBarView
            ZoneBarView zoneBarView = topBarObj.GetComponent<ZoneBarView>();
            if (zoneBarView != null)
            {
                SerializedObject so = new SerializedObject(zoneBarView);
                
                SerializedProperty textsProp = so.FindProperty("ui_slot_texts");
                textsProp.ClearArray();
                for (int i = 0; i < slotTexts.Count; i++)
                {
                    textsProp.InsertArrayElementAtIndex(i);
                    textsProp.GetArrayElementAtIndex(i).objectReferenceValue = slotTexts[i];
                }

                SerializedProperty bgsProp = so.FindProperty("ui_slot_backgrounds");
                bgsProp.ClearArray();
                for (int i = 0; i < slotBgs.Count; i++)
                {
                    bgsProp.InsertArrayElementAtIndex(i);
                    bgsProp.GetArrayElementAtIndex(i).objectReferenceValue = slotBgs[i];
                }

                so.ApplyModifiedProperties();

                // Trigger immediate visual refresh for current zone (Zone 1)
                zoneBarView.UpdateZoneBar(new ZoneData(1, ZoneType.Normal, null, 1f));
            }

            EditorSceneManager.MarkSceneDirty(topBarObj.scene);
            EditorSceneManager.SaveScene(topBarObj.scene);
            Debug.Log("[ZoneBarPolish] Zone Bar polished successfully with 13 slots, metallic frame, and silver notch badge!");
        }
    }
}
