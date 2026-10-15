using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Rewards.Views;

namespace Vertigo.Editor
{
    public static class InventoryScrollViewSetupUtility
    {
        [MenuItem("Vertigo/UI/Setup Inventory Scroll View")]
        public static void SetupInventoryScrollView()
        {
            SetupInventoryScrollViewInternal();
        }

        [InitializeOnLoadMethod]
        private static void AutoSetupOnLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorPrefs.GetBool("InventoryScrollViewSetup_V3", false)) return;
            EditorPrefs.SetBool("InventoryScrollViewSetup_V3", true);

            EditorApplication.delayCall += () =>
            {
                SetupInventoryScrollViewInternal();
            };
        }

        private static void SetupInventoryScrollViewInternal()
        {
            GameObject leftPanelObj = GameObject.Find("ui_panel_left_inventory");
            if (leftPanelObj == null)
            {
                Debug.LogWarning("[InventoryScrollViewSetup] 'ui_panel_left_inventory' not found!");
                return;
            }

            InventoryView inventoryView = leftPanelObj.GetComponent<InventoryView>();
            Transform slotsTr = leftPanelObj.transform.Find("ui_container_slots");
            Transform existingScrollViewTr = leftPanelObj.transform.Find("ui_scrollview_inventory");

            GameObject scrollViewObj;
            if (existingScrollViewTr == null)
            {
                scrollViewObj = new GameObject("ui_scrollview_inventory", typeof(RectTransform));
                scrollViewObj.transform.SetParent(leftPanelObj.transform, false);
            }
            else
            {
                scrollViewObj = existingScrollViewTr.gameObject;
            }

            RectTransform svRT = scrollViewObj.GetComponent<RectTransform>();
            svRT.anchorMin = Vector2.zero;
            svRT.anchorMax = Vector2.one;
            svRT.pivot = new Vector2(0.5f, 0.5f);
            svRT.offsetMin = new Vector2(0, 16);
            svRT.offsetMax = new Vector2(0, -82);

            // Add or get RectMask2D
            RectMask2D mask = scrollViewObj.GetComponent<RectMask2D>();
            if (mask == null)
            {
                mask = scrollViewObj.AddComponent<RectMask2D>();
            }

            // Ensure ScrollView has a transparent Image so drag/scroll wheel events are received
            Image svImg = scrollViewObj.GetComponent<Image>();
            if (svImg == null)
            {
                svImg = scrollViewObj.AddComponent<Image>();
            }
            svImg.color = Color.clear;
            svImg.raycastTarget = true;
            svImg.maskable = true;

            // Add or get ScrollRect
            ScrollRect scrollRect = scrollViewObj.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                scrollRect = scrollViewObj.AddComponent<ScrollRect>();
            }
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.scrollSensitivity = 25f;
            scrollRect.viewport = svRT;

            // Reparent ui_container_slots into ui_scrollview_inventory if it's currently a direct child of leftPanel
            if (slotsTr != null && slotsTr.parent == leftPanelObj.transform)
            {
                slotsTr.SetParent(scrollViewObj.transform, false);
            }
            else if (slotsTr == null)
            {
                slotsTr = scrollViewObj.transform.Find("ui_container_slots");
            }

            if (slotsTr != null)
            {
                RectTransform scRT = slotsTr.GetComponent<RectTransform>();
                scRT.anchorMin = new Vector2(0f, 1f);
                scRT.anchorMax = new Vector2(1f, 1f);
                scRT.pivot = new Vector2(0.5f, 1f);
                scRT.anchoredPosition = Vector2.zero;
                scRT.sizeDelta = Vector2.zero;

                scrollRect.content = scRT;

                // Ensure VerticalLayoutGroup
                VerticalLayoutGroup vlg = slotsTr.GetComponent<VerticalLayoutGroup>();
                if (vlg != null)
                {
                    vlg.padding = new RectOffset(0, 0, 4, 12);
                    vlg.spacing = 14;
                    vlg.childControlWidth = true;
                    vlg.childControlHeight = true;
                    vlg.childForceExpandWidth = true;
                    vlg.childForceExpandHeight = false;
                    vlg.childAlignment = TextAnchor.UpperCenter;
                }

                // Ensure ContentSizeFitter
                ContentSizeFitter csf = slotsTr.GetComponent<ContentSizeFitter>();
                if (csf == null)
                {
                    csf = slotsTr.gameObject.AddComponent<ContentSizeFitter>();
                }
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            // Wire InventoryView
            if (inventoryView != null)
            {
                SerializedObject invSO = new SerializedObject(inventoryView);
                invSO.FindProperty("ui_scroll_rect").objectReferenceValue = scrollRect;
                if (slotsTr != null)
                {
                    invSO.FindProperty("ui_container_slots").objectReferenceValue = slotsTr;
                }
                invSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(inventoryView);
            }

            // Enforce maskable on all MaskableGraphic (Image, TMP) components inside scroll view so nothing bleeds past RectMask2D
            MaskableGraphic[] graphics = scrollViewObj.GetComponentsInChildren<MaskableGraphic>(true);
            foreach (var g in graphics)
            {
                if (g != null && !g.maskable)
                {
                    g.maskable = true;
                    EditorUtility.SetDirty(g);
                }
            }

            EditorUtility.SetDirty(scrollViewObj);
            if (slotsTr != null) EditorUtility.SetDirty(slotsTr.gameObject);
            EditorSceneManager.MarkSceneDirty(leftPanelObj.scene);
            EditorSceneManager.SaveScene(leftPanelObj.scene);

            Debug.Log("[InventoryScrollViewSetup] Successfully upgraded inventory panel with ScrollRect and RectMask2D!");
        }
    }
}
