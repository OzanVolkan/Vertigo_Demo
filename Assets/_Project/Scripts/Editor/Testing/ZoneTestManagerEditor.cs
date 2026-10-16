using UnityEditor;
using UnityEngine;
using Vertigo.Core.Enums;
using Vertigo.Core.Events;
using Vertigo.Testing;

namespace Vertigo.Editor
{
    [CustomEditor(typeof(ZoneTestManager))]
    public class ZoneTestManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            ZoneTestManager testManager = (ZoneTestManager)target;

            serializedObject.Update();

            // 1. Header Banner & Status
            EditorGUILayout.Space(6);
            DrawStatusHeader(testManager);
            EditorGUILayout.Space(6);

            // 2. Target Zone Control
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("ZONE NAVIGATION", EditorStyles.boldLabel);

            SerializedProperty targetZoneProp = serializedObject.FindProperty("targetZone");
            EditorGUILayout.IntSlider(targetZoneProp, 1, 60, new GUIContent("Target Zone"));

            EditorGUILayout.Space(4);

            // Big Jump Button
            Color prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.35f, 0.75f, 1f, 1f); // Crisp cyan/blue
            if (GUILayout.Button($"JUMP TO ZONE {targetZoneProp.intValue}", GUILayout.Height(36)))
            {
                testManager.JumpToZone(targetZoneProp.intValue);
            }
            GUI.backgroundColor = prevBg;

            EditorGUILayout.Space(4);

            // Previous / Next Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("<< Previous Zone (-1)", GUILayout.Height(28)))
            {
                testManager.PreviousZone();
            }
            if (GUILayout.Button("Next Zone (+1) >>", GUILayout.Height(28)))
            {
                testManager.NextZone();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(6);

            // 3. Quick Presets Section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("QUICK PRESETS", EditorStyles.boldLabel);

            // Normal Start
            if (GUILayout.Button("Zone 1 (Normal Start - Bronze Wheel)", GUILayout.Height(26)))
            {
                testManager.JumpToZone(1);
            }

            EditorGUILayout.Space(2);

            // Safe Zones (Silver Wheel)
            GUI.backgroundColor = new Color(0.4f, 0.95f, 0.4f, 1f); // Neon green
            EditorGUILayout.LabelField("Safe Zones (Silver Wheel - No Bomb):", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Zone 5", GUILayout.Height(26))) testManager.JumpToZone(5);
            if (GUILayout.Button("Zone 10", GUILayout.Height(26))) testManager.JumpToZone(10);
            if (GUILayout.Button("Zone 15", GUILayout.Height(26))) testManager.JumpToZone(15);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Zone 20", GUILayout.Height(26))) testManager.JumpToZone(20);
            if (GUILayout.Button("Zone 25", GUILayout.Height(26))) testManager.JumpToZone(25);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            // Super Zone (Golden Wheel)
            GUI.backgroundColor = new Color(1f, 0.82f, 0.2f, 1f); // Rich gold
            EditorGUILayout.LabelField("Super Zone (Golden Wheel - Special Rewards):", EditorStyles.miniLabel);
            if (GUILayout.Button("★ ZONE 30 (SUPER ZONE - GOLDEN WHEEL) ★", GUILayout.Height(32)))
            {
                testManager.JumpToZone(30);
            }

            GUI.backgroundColor = prevBg;
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(6);

            // 4. Quick Cheats & Debug Actions (Play mode)
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("GAMEPLAY ACTIONS (PLAY MODE)", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(1f, 0.4f, 0.4f, 1f); // Red
                if (GUILayout.Button("Trigger Bomb (Death)", GUILayout.Height(26)))
                {
                    GameEvents.FireBombTriggered();
                }

                GUI.backgroundColor = new Color(0.4f, 0.85f, 1f, 1f); // Blue
                if (GUILayout.Button("Cash Out (Leave)", GUILayout.Height(26)))
                {
                    GameEvents.FireCashOutPromptRequested();
                }
                GUI.backgroundColor = prevBg;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(6);
            }

            // 5. Test Flags & Cheats
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("TEST FLAGS & CHEATS", EditorStyles.boldLabel);

            SerializedProperty disableBombProp = serializedObject.FindProperty("disableBomb");
            SerializedProperty forceBombProp = serializedObject.FindProperty("forceBomb");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(disableBombProp, new GUIContent("Disable Bomb (0% Bomb Chance)", "When checked, wheel spins will NEVER land on a bomb slice."));
            if (EditorGUI.EndChangeCheck() && disableBombProp.boolValue)
            {
                forceBombProp.boolValue = false;
            }

            if (disableBombProp.boolValue)
            {
                EditorGUILayout.HelpBox("BOMBA DEVRE DISI (%0 SANS): Cark donuslerinde bomba dilimi elenir ve asla bomba gelmez.", MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(forceBombProp, new GUIContent("Force Bomb (100% Bomb Chance)", "When checked, normal wheel spins will ALWAYS land on the bomb slice."));
            if (EditorGUI.EndChangeCheck() && forceBombProp.boolValue)
            {
                disableBombProp.boolValue = false;
            }

            if (forceBombProp.boolValue)
            {
                EditorGUILayout.HelpBox("GARANTI BOMBA (%100 SANS): Bomba iceren carklarda donus kesinlikle bomba uzerinde durur.", MessageType.Error);
            }

            SerializedProperty clearInvProp = serializedObject.FindProperty("clearInventoryOnJump");
            EditorGUILayout.PropertyField(clearInvProp, new GUIContent("Clear Inventory On Jump"));

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(6);

            // 6. Inventory & Scroll Testing (Play Mode)
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("INVENTORY & SCROLL TESTING", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Sol panel envanterinin tasma ve kaydirma (scroll) davranisini test edin:", EditorStyles.miniLabel);

                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.4f, 0.9f, 0.5f, 1f); // Mint Green
                if (GUILayout.Button("+1 Random Reward", GUILayout.Height(30)))
                {
                    testManager.AddRandomTestReward();
                }

                GUI.backgroundColor = new Color(1f, 0.75f, 0.2f, 1f); // Warm Gold
                if (GUILayout.Button("Fill 12 Items (Test Scroll Overflow)", GUILayout.Height(30)))
                {
                    testManager.FillInventoryForScrollTest(12);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.6f, 0.8f, 1f, 1f); // Cyan
                if (GUILayout.Button("Scroll To Top", GUILayout.Height(26)))
                {
                    testManager.ScrollToTop();
                }
                if (GUILayout.Button("Scroll To Bottom", GUILayout.Height(26)))
                {
                    testManager.ScrollToBottom();
                }

                GUI.backgroundColor = new Color(1f, 0.45f, 0.45f, 1f); // Red
                if (GUILayout.Button("Clear Inventory", GUILayout.Height(26)))
                {
                    testManager.ClearInventory();
                }
                GUI.backgroundColor = prevBg;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(6);
            }

            // 7. Dependencies
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("REFERENCES", EditorStyles.boldLabel);

            SerializedProperty zmProp = serializedObject.FindProperty("zoneManager");
            SerializedProperty gmProp = serializedObject.FindProperty("gameManager");
            SerializedProperty wcProp = serializedObject.FindProperty("wheelController");
            SerializedProperty invMgrProp = serializedObject.FindProperty("inventoryManager");
            SerializedProperty invViewProp = serializedObject.FindProperty("inventoryView");
            SerializedProperty rewardsProp = serializedObject.FindProperty("availableRewards");

            EditorGUILayout.PropertyField(zmProp);
            EditorGUILayout.PropertyField(gmProp);
            EditorGUILayout.PropertyField(wcProp);
            EditorGUILayout.PropertyField(invMgrProp);
            EditorGUILayout.PropertyField(invViewProp);
            EditorGUILayout.PropertyField(rewardsProp);

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawStatusHeader(ZoneTestManager testManager)
        {
            int currentZone = testManager.GetCurrentZone();
            ZoneType zoneType = testManager.GetCurrentZoneType();
            float multiplier = testManager.GetCurrentMultiplier();

            Color statusColor;
            string typeName;

            switch (zoneType)
            {
                case ZoneType.Safe:
                    statusColor = new Color(0.25f, 0.85f, 0.25f, 1f);
                    typeName = "GÜVENLİ BÖLGE (Silver Wheel - Safe)";
                    break;
                case ZoneType.Super:
                    statusColor = new Color(1f, 0.82f, 0.15f, 1f);
                    typeName = "SÜPER BÖLGE (Golden Wheel - Super)";
                    break;
                default:
                    statusColor = new Color(0.85f, 0.88f, 0.95f, 1f);
                    typeName = "NORMAL BÖLGE (Bronze Wheel - Normal)";
                    break;
            }

            GUIStyle boxStyle = new GUIStyle(EditorStyles.helpBox);
            boxStyle.padding = new RectOffset(12, 12, 10, 10);

            EditorGUILayout.BeginVertical(boxStyle);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 15;
            titleStyle.normal.textColor = statusColor;

            EditorGUILayout.LabelField($"CURRENT: ZONE {currentZone}", titleStyle);

            GUIStyle subStyle = new GUIStyle(EditorStyles.label);
            subStyle.fontSize = 12;
            subStyle.normal.textColor = Color.white;

            EditorGUILayout.LabelField($"Tip: {typeName}", subStyle);
            EditorGUILayout.LabelField($"Ödül Çarpanı: {multiplier:F2}x", subStyle);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Play Mode aktif değilken de zone seçebilirsiniz, tam çark akışını test etmek için Play'e basabilirsiniz.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        [MenuItem("Vertigo/Testing/Add Zone Test Manager To Scene")]
        public static void AddZoneTestManagerToScene()
        {
            EnsureInScene();
        }

        [InitializeOnLoadMethod]
        private static void AutoEnsureOnLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorPrefs.GetBool("ZoneTestManagerSetup_V1", false)) return;
            EditorPrefs.SetBool("ZoneTestManagerSetup_V1", true);

            EditorApplication.delayCall += () =>
            {
                EnsureInScene();
            };
        }

        private static void EnsureInScene()
        {
            ZoneTestManager existing = Object.FindFirstObjectByType<ZoneTestManager>();
            if (existing != null) return;

            GameObject targetObj = GameObject.Find("[GameCore]");
            if (targetObj == null) targetObj = GameObject.Find("GameCore");
            if (targetObj == null) targetObj = GameObject.Find("GameSystems");

            if (targetObj == null)
            {
                targetObj = new GameObject("[ZoneTestManager]");
            }

            ZoneTestManager ztm = targetObj.AddComponent<ZoneTestManager>();
            ztm.EnsureDependencies();
            EditorUtility.SetDirty(targetObj);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(targetObj.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(targetObj.scene);
            Debug.Log($"[ZoneTestManager] Added ZoneTestManager to '{targetObj.name}' in scene!");
        }
    }
}
