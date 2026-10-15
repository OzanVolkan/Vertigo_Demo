using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Vertigo.Core.State;
using Vertigo.Rewards.Views;
using Vertigo.Wheel.Views;

namespace Vertigo.Editor
{
    public static class RewardCollectEffectSetupUtility
    {
        [MenuItem("Vertigo/UI/Setup Reward Collect Effect")]
        public static void SetupRewardCollectEffect()
        {
            SetupRewardCollectEffectInternal();
        }

        [InitializeOnLoadMethod]
        private static void AutoSetupOnLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorPrefs.GetBool("RewardCollectEffectSetup_V2", false)) return;
            EditorPrefs.SetBool("RewardCollectEffectSetup_V2", true);

            EditorApplication.delayCall += () =>
            {
                SetupRewardCollectEffectInternal();
            };
        }

        private static void SetupRewardCollectEffectInternal()
        {
            var canvasObj = GameObject.Find("ui_canvas_main");
            if (canvasObj == null)
            {
                Debug.LogWarning("[RewardCollectEffectSetup] 'ui_canvas_main' not found in current scene!");
                return;
            }

            Transform containerTr = canvasObj.transform.Find("ui_particle_container");
            GameObject containerObj;
            if (containerTr == null)
            {
                containerObj = new GameObject("ui_particle_container", typeof(RectTransform));
                containerObj.transform.SetParent(canvasObj.transform, false);
            }
            else
            {
                containerObj = containerTr.gameObject;
            }

            RectTransform rt = containerObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Transform bombPopup = canvasObj.transform.Find("ui_panel_bomb_popup");
            if (bombPopup != null)
            {
                containerObj.transform.SetSiblingIndex(bombPopup.GetSiblingIndex());
            }

            RewardCollectEffectView effectView = containerObj.GetComponent<RewardCollectEffectView>();
            if (effectView == null)
            {
                effectView = containerObj.AddComponent<RewardCollectEffectView>();
            }

            Sprite bronzeFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_particle_frame_bronze.png");
            Sprite silverFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_particle_frame_silver.png");
            Sprite goldFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_particle_frame_gold.png");

            SerializedObject so = new SerializedObject(effectView);
            so.FindProperty("bronzeFrameSprite").objectReferenceValue = bronzeFrame;
            so.FindProperty("silverFrameSprite").objectReferenceValue = silverFrame;
            so.FindProperty("goldFrameSprite").objectReferenceValue = goldFrame;
            so.FindProperty("ui_particle_container").objectReferenceValue = rt;
            so.ApplyModifiedProperties();

            // Wire GameManager
            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                SerializedObject gmSO = new SerializedObject(gm);
                WheelView wv = Object.FindFirstObjectByType<WheelView>();
                InventoryView iv = Object.FindFirstObjectByType<InventoryView>();

                if (wv != null) gmSO.FindProperty("wheelView").objectReferenceValue = wv;
                if (iv != null) gmSO.FindProperty("inventoryView").objectReferenceValue = iv;
                gmSO.FindProperty("rewardCollectEffect").objectReferenceValue = effectView;
                gmSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(gm);
            }

            EditorUtility.SetDirty(containerObj);
            EditorSceneManager.MarkSceneDirty(canvasObj.scene);
            EditorSceneManager.SaveScene(canvasObj.scene);

            Debug.Log("[RewardCollectEffectSetup] Reward collect effect successfully setup and wired in scene!");
        }
    }
}
