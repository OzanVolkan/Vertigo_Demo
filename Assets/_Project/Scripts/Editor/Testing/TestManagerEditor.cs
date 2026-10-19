using UnityEditor;
using UnityEngine;
using Vertigo.Core.Enums;
using Vertigo.Core.Events;
using Vertigo.Core.Save;
using Vertigo.Rewards.Controllers;
using Vertigo.Testing;

namespace Vertigo.Editor
{
    [CustomEditor(typeof(TestManager))]
    public class TestManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TestManager testManager = (TestManager)target;
            serializedObject.Update();

            EditorGUILayout.Space(6);
            DrawStatusHeader(testManager);
            EditorGUILayout.Space(6);

            // 1. Wallet (Banked Gold & Cash) Controls
            DrawWalletSection(testManager);
            EditorGUILayout.Space(6);

            // 2. Target Zone Control
            DrawZoneNavigationSection(testManager);
            EditorGUILayout.Space(6);

            // 3. Quick Presets Section
            DrawQuickPresetsSection(testManager);
            EditorGUILayout.Space(6);

            // 4. Quick Cheats & Debug Actions (Play mode)
            if (Application.isPlaying)
            {
                DrawPlayModeActionsSection();
                EditorGUILayout.Space(6);
            }

            // 5. Test Flags & Cheats
            DrawTestFlagsSection();
            EditorGUILayout.Space(6);

            // 6. Inventory & Scroll Testing (Play Mode)
            if (Application.isPlaying)
            {
                DrawInventoryTestingSection(testManager);
                EditorGUILayout.Space(6);
            }

            // 7. Dependencies
            DrawReferencesSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawWalletSection(TestManager testManager)
        {
            Color prevBg = GUI.backgroundColor;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("WALLET (BANKED GOLD & CASH)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                $"Banked Gold: {SessionInventoryManager.GetBankedGold():N0}   |   Banked Cash: ${SessionInventoryManager.GetBankedCash():N0}",
                EditorStyles.miniLabel);

            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(1f, 0.82f, 0.2f, 1f);
            if (GUILayout.Button("+100 Banked Gold", GUILayout.Height(28)))
            {
                testManager.Add100BankedGold();
            }

            GUI.backgroundColor = new Color(1f, 0.45f, 0.45f, 1f);
            if (GUILayout.Button("Reset Wallet (100)", GUILayout.Height(28)))
            {
                testManager.ResetWallet();
            }

            GUI.backgroundColor = prevBg;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawZoneNavigationSection(TestManager testManager)
        {
            Color prevBg = GUI.backgroundColor;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("ZONE NAVIGATION", EditorStyles.boldLabel);

            SerializedProperty targetZoneProp = serializedObject.FindProperty("targetZone");
            EditorGUILayout.IntSlider(targetZoneProp, 1, 60, new GUIContent("Target Zone"));

            EditorGUILayout.Space(4);

            GUI.backgroundColor = new Color(0.35f, 0.75f, 1f, 1f);
            if (GUILayout.Button($"JUMP TO ZONE {targetZoneProp.intValue}", GUILayout.Height(36)))
            {
                testManager.JumpToZone(targetZoneProp.intValue);
            }
            GUI.backgroundColor = prevBg;

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("<< Previous Zone (-1)", GUILayout.Height(28))) testManager.PreviousZone();
            if (GUILayout.Button("Next Zone (+1) >>", GUILayout.Height(28))) testManager.NextZone();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawQuickPresetsSection(TestManager testManager)
        {
            Color prevBg = GUI.backgroundColor;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("QUICK PRESETS", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(0.4f, 0.95f, 0.4f, 1f);
            if (GUILayout.Button("Zone 1 (Safe Start - Silver Wheel)", GUILayout.Height(28)))
            {
                testManager.JumpToZone(1);
            }

            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Safe Zones (Silver Wheel - No Bomb):", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Zone 5", GUILayout.Height(26))) testManager.JumpToZone(5);
            if (GUILayout.Button("Zone 10", GUILayout.Height(26))) testManager.JumpToZone(10);
            if (GUILayout.Button("Zone 15", GUILayout.Height(26))) testManager.JumpToZone(15);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Zone 20", GUILayout.Height(26))) testManager.JumpToZone(20);
            if (GUILayout.Button("Zone 25", GUILayout.Height(26))) testManager.JumpToZone(25);
            if (GUILayout.Button("Zone 35", GUILayout.Height(26))) testManager.JumpToZone(35);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Zone 40", GUILayout.Height(26))) testManager.JumpToZone(40);
            if (GUILayout.Button("Zone 45", GUILayout.Height(26))) testManager.JumpToZone(45);
            if (GUILayout.Button("Zone 50", GUILayout.Height(26))) testManager.JumpToZone(50);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Zone 55 (Final Safe Zone)", GUILayout.Height(26))) testManager.JumpToZone(55);

            EditorGUILayout.Space(2);

            GUI.backgroundColor = new Color(1f, 0.82f, 0.2f, 1f);
            EditorGUILayout.LabelField("Super Zones & Finale:", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("★ ZONE 30 (SUPER) ★", GUILayout.Height(28))) testManager.JumpToZone(30);
            if (GUILayout.Button("★ ZONE 60 (FINAL) ★", GUILayout.Height(28))) testManager.JumpToZone(60);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);
            GUI.backgroundColor = new Color(0.85f, 0.85f, 1f, 1f);
            if (GUILayout.Button("Zone 56 (Post-Safe / Final Stretch)", GUILayout.Height(26)))
            {
                testManager.JumpToZone(56);
            }

            GUI.backgroundColor = prevBg;
            EditorGUILayout.EndVertical();
        }

        private void DrawPlayModeActionsSection()
        {
            Color prevBg = GUI.backgroundColor;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("GAMEPLAY ACTIONS (PLAY MODE)", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f, 1f);
            if (GUILayout.Button("Trigger Bomb (Death)", GUILayout.Height(26)))
            {
                GameEvents.FireBombTriggered();
            }

            GUI.backgroundColor = new Color(0.4f, 0.85f, 1f, 1f);
            if (GUILayout.Button("Cash Out (Leave)", GUILayout.Height(26)))
            {
                GameEvents.FireCashOutPromptRequested();
            }
            GUI.backgroundColor = prevBg;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawTestFlagsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("TEST FLAGS & CHEATS", EditorStyles.boldLabel);

            SerializedProperty disableBombProp = serializedObject.FindProperty("disableBomb");
            SerializedProperty forceBombProp = serializedObject.FindProperty("forceBomb");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(disableBombProp, new GUIContent("Disable Bomb (0% Bomb Chance)"));
            if (EditorGUI.EndChangeCheck() && disableBombProp.boolValue)
            {
                forceBombProp.boolValue = false;
            }

            if (disableBombProp.boolValue)
            {
                EditorGUILayout.HelpBox("BOMBA DEVRE DISI (%0 SANS): Cark donuslerinde bomba dilimi elenir ve asla bomba gelmez.", MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(forceBombProp, new GUIContent("Force Bomb (100% Bomb Chance)"));
            if (EditorGUI.EndChangeCheck() && forceBombProp.boolValue)
            {
                disableBombProp.boolValue = false;
            }

            if (forceBombProp.boolValue)
            {
                EditorGUILayout.HelpBox("GARANTI BOMBA (%100 SANS): Bomba iceren carklarda donus kesinlikle bomba uzerinde durur.", MessageType.Error);
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("clearInventoryOnJump"), new GUIContent("Clear Inventory On Jump"));
            EditorGUILayout.EndVertical();
        }

        private void DrawInventoryTestingSection(TestManager testManager)
        {
            Color prevBg = GUI.backgroundColor;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("INVENTORY & SCROLL TESTING", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.4f, 0.9f, 0.5f, 1f);
            if (GUILayout.Button("+1 Random Reward", GUILayout.Height(30)))
            {
                testManager.AddRandomTestReward();
            }

            GUI.backgroundColor = new Color(1f, 0.75f, 0.2f, 1f);
            if (GUILayout.Button("Fill 12 Items (Test Scroll Overflow)", GUILayout.Height(30)))
            {
                testManager.FillInventoryForScrollTest(12);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.6f, 0.8f, 1f, 1f);
            if (GUILayout.Button("Scroll To Top", GUILayout.Height(26))) testManager.ScrollToTop();
            if (GUILayout.Button("Scroll To Bottom", GUILayout.Height(26))) testManager.ScrollToBottom();

            GUI.backgroundColor = new Color(1f, 0.45f, 0.45f, 1f);
            if (GUILayout.Button("Clear Inventory", GUILayout.Height(26))) testManager.ClearInventory();
            GUI.backgroundColor = prevBg;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawReferencesSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("REFERENCES", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("zoneManager"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gameManager"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wheelController"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("inventoryManager"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("inventoryView"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("availableRewards"));
            EditorGUILayout.EndVertical();
        }

        private void DrawStatusHeader(TestManager testManager)
        {
            int currentZone = testManager.GetCurrentZone();
            ZoneType zoneType = testManager.GetCurrentZoneType();
            float multiplier = testManager.GetCurrentMultiplier();

            (Color statusColor, string typeName) = zoneType switch
            {
                ZoneType.Safe => (new Color(0.25f, 0.85f, 0.25f, 1f), "SAFE ZONE (Silver Wheel - Safe)"),
                ZoneType.Super => (new Color(1f, 0.82f, 0.15f, 1f), "SUPER ZONE (Golden Wheel - Super)"),
                _ => (new Color(0.85f, 0.88f, 0.95f, 1f), "NORMAL ZONE (Bronze Wheel - Normal)")
            };

            GUIStyle boxStyle = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(12, 12, 10, 10) };
            EditorGUILayout.BeginVertical(boxStyle);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 15 };
            titleStyle.normal.textColor = statusColor;
            EditorGUILayout.LabelField($"CURRENT: ZONE {currentZone}", titleStyle);

            GUIStyle subStyle = new GUIStyle(EditorStyles.label) { fontSize = 12 };
            subStyle.normal.textColor = Color.white;
            EditorGUILayout.LabelField($"Type: {typeName}", subStyle);
            EditorGUILayout.LabelField($"Reward Multiplier: {multiplier:F2}x", subStyle);

            EditorGUILayout.EndVertical();
        }

        [MenuItem("Vertigo/Testing/Add Test Manager To Scene")]
        public static void AddTestManagerToScene() => EnsureInScene();

        [MenuItem("Vertigo/Save System/Reset Save Data (Default 100 Gold)")]
        public static void MenuResetSaveData()
        {
            SessionInventoryManager.ResetBankedWallet();
            Debug.Log($"[SaveService] Save data reset. Gold: {SessionInventoryManager.GetBankedGold():N0}");
        }

        [MenuItem("Vertigo/Save System/Open Save Folder")]
        public static void MenuOpenSaveFolder()
        {
            string path = SaveService.FilePath;
            if (System.IO.File.Exists(path))
            {
                EditorUtility.RevealInFinder(path);
            }
            else
            {
                EditorUtility.RevealInFinder(Application.persistentDataPath);
            }
        }

        private static void EnsureInScene()
        {
            TestManager existing = Object.FindFirstObjectByType<TestManager>();
            if (existing != null) return;

            GameObject targetObj = GameObject.Find("[GameCore]")
                ?? GameObject.Find("GameCore")
                ?? GameObject.Find("GameSystems")
                ?? new GameObject("[TestManager]");

            TestManager tm = targetObj.AddComponent<TestManager>();
            tm.EnsureDependencies();
            EditorUtility.SetDirty(targetObj);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(targetObj.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(targetObj.scene);
        }
    }
}
