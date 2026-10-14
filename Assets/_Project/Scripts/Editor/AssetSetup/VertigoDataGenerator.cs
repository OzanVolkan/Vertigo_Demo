using System.IO;
using UnityEditor;
using UnityEngine;
using Vertigo.Core.Enums;
using Vertigo.Rewards.Data;
using Vertigo.Wheel.Data;
using Vertigo.Zone.Data;

namespace Vertigo.Editor
{
    public static class VertigoDataGenerator
    {
        private const string RewardsPath = "Assets/_Project/Data/Rewards";
        private const string WheelsPath = "Assets/_Project/Data/Wheels";
        private const string ZonesPath = "Assets/_Project/Data/Zones";

        [MenuItem("Tools/Vertigo/Generate Initial Data Assets")]
        public static void GenerateAllData()
        {
            EnsureDirectories();

            // 1. Generate Rewards
            RewardDataSO bombReward = CreateReward("Reward_Bomb", "Bomb", "Assets/2D/demo_content/ui_card_icon_death.png", RewardType.Bomb);
            RewardDataSO cashReward = CreateReward("Reward_Cash", "Cash", "Assets/2D/demo_content/UI_icon_cash.png", RewardType.Cash, "$", false);
            RewardDataSO goldReward = CreateReward("Reward_Gold", "Gold", "Assets/2D/demo_content/UI_icon_gold.png", RewardType.Gold, "x", false);
            RewardDataSO bronzeChest = CreateReward("Reward_Chest_Bronze", "Bronze Chest", "Assets/2D/demo_content/UI_icon_chest_Bronze_nolight.png", RewardType.Chest);
            RewardDataSO silverChest = CreateReward("Reward_Chest_Silver", "Silver Chest", "Assets/2D/demo_content/UI_icon_chest_silver_nolight.png", RewardType.Chest);
            RewardDataSO goldChest = CreateReward("Reward_Chest_Gold", "Gold Chest", "Assets/2D/demo_content/UI_icon_chest_gold_nolight.png", RewardType.Chest);
            RewardDataSO superChest = CreateReward("Reward_Chest_Super", "Super Chest", "Assets/2D/demo_content/UI_icon_chest_super_nolight.png", RewardType.Chest);
            RewardDataSO shotgun = CreateReward("Reward_Shotgun", "Tier 1 Shotgun", "Assets/2D/demo_content/UI_Icon_Renders_tier1_shotgun.png", RewardType.Weapon);
            RewardDataSO rifle = CreateReward("Reward_Rifle", "Tier 2 Rifle", "Assets/2D/demo_content/UI_Icon_Renders_tier2_rifle.png", RewardType.Weapon);
            RewardDataSO bayonet = CreateReward("Reward_Bayonet", "MLE Bayonet", "Assets/2D/demo_content/ui_icon_mle_bayonet_easter_time.png", RewardType.Weapon);
            RewardDataSO grenade = CreateReward("Reward_Grenade", "M26 Grenade", "Assets/2D/demo_content/ui_icon_render_cons_grenade_m26.png", RewardType.Consumable);
            RewardDataSO healthshot = CreateReward("Reward_Healthshot", "Neurostim Healthshot", "Assets/2D/demo_content/ui_icon_render_cons_healthshot_2_neurostim.png", RewardType.Consumable);
            RewardDataSO pistolPoints = CreateReward("Reward_Points_Pistol", "Pistol Points", "Assets/2D/demo_content/UI_Icons_Pistol_Points.png", RewardType.Points);
            RewardDataSO shotgunPoints = CreateReward("Reward_Points_Shotgun", "Shotgun Points", "Assets/2D/demo_content/UI_Icons_Shotgun_Points.png", RewardType.Points);

            // 2. Generate Wheels
            Sprite bronzeBase = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_spin_bronze_base.png");
            Sprite bronzeIndicator = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_spin_bronze_indicator.png");
            Sprite silverBase = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_spin_silver_base.png");
            Sprite silverIndicator = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_spin_silver_indicator.png");
            Sprite goldBase = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_spin_golden_base.png");
            Sprite goldIndicator = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D/demo_content/ui_spin_golden_indicator.png");

            // Bronze Wheel (Normal Zone: 1 Bomb + 7 Rewards)
            WheelConfigSO bronzeWheel = CreateWheel("WheelConfig_Bronze_Normal", WheelType.BronzeNormal, bronzeBase, bronzeIndicator, new[]
            {
                new WheelSliceData(0, bombReward, 1, 1f),
                new WheelSliceData(1, cashReward, 150, 1.5f),
                new WheelSliceData(2, goldReward, 2, 1.2f),
                new WheelSliceData(3, pistolPoints, 5, 1.5f),
                new WheelSliceData(4, grenade, 1, 1f),
                new WheelSliceData(5, shotgunPoints, 10, 1.5f),
                new WheelSliceData(6, bronzeChest, 1, 0.8f),
                new WheelSliceData(7, shotgun, 1, 0.5f)
            });

            // Silver Wheel (Safe Zone: Risk-free Silver Spin without bomb)
            WheelConfigSO silverWheel = CreateWheel("WheelConfig_Silver_Safe", WheelType.SilverSafe, silverBase, silverIndicator, new[]
            {
                new WheelSliceData(0, silverChest, 1, 1f),
                new WheelSliceData(1, cashReward, 500, 1.5f),
                new WheelSliceData(2, goldReward, 10, 1.2f),
                new WheelSliceData(3, rifle, 1, 0.8f),
                new WheelSliceData(4, healthshot, 2, 1.5f),
                new WheelSliceData(5, bayonet, 1, 0.6f),
                new WheelSliceData(6, goldReward, 25, 0.9f),
                new WheelSliceData(7, cashReward, 1200, 1.1f)
            });

            // Golden Wheel (Super Zone: Risk-free Golden Spin with special rewards without bomb)
            WheelConfigSO goldWheel = CreateWheel("WheelConfig_Golden_Super", WheelType.GoldenSuper, goldBase, goldIndicator, new[]
            {
                new WheelSliceData(0, superChest, 1, 1f),
                new WheelSliceData(1, goldChest, 2, 1.2f),
                new WheelSliceData(2, goldReward, 100, 1f),
                new WheelSliceData(3, cashReward, 5000, 1.2f),
                new WheelSliceData(4, bayonet, 2, 0.8f),
                new WheelSliceData(5, rifle, 2, 0.8f),
                new WheelSliceData(6, goldReward, 250, 0.7f),
                new WheelSliceData(7, cashReward, 10000, 0.9f)
            });

            // 3. Generate Zone Manager Config
            CreateZoneManagerConfig("ZoneConfig_Default", bronzeWheel, silverWheel, goldWheel);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Vertigo] All initial Data assets generated successfully!");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(RewardsPath)) Directory.CreateDirectory(RewardsPath);
            if (!Directory.Exists(WheelsPath)) Directory.CreateDirectory(WheelsPath);
            if (!Directory.Exists(ZonesPath)) Directory.CreateDirectory(ZonesPath);
        }

        private static RewardDataSO CreateReward(string fileName, string displayName, string iconPath, RewardType type, string prefix = "x", bool abbrev = true)
        {
            string path = $"{RewardsPath}/{fileName}.asset";
            RewardDataSO reward = AssetDatabase.LoadAssetAtPath<RewardDataSO>(path);
            if (reward == null)
            {
                reward = ScriptableObject.CreateInstance<RewardDataSO>();
                AssetDatabase.CreateAsset(reward, path);
            }

            SerializedObject so = new SerializedObject(reward);
            so.FindProperty("id").stringValue = fileName;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("type").enumValueIndex = (int)type;
            so.FindProperty("amountPrefix").stringValue = prefix;
            so.FindProperty("useAbbreviatedAmount").boolValue = abbrev;

            Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (icon != null)
            {
                so.FindProperty("icon").objectReferenceValue = icon;
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(reward);
            return reward;
        }

        private static WheelConfigSO CreateWheel(string fileName, WheelType type, Sprite baseSprite, Sprite indicator, WheelSliceData[] slices)
        {
            string path = $"{WheelsPath}/{fileName}.asset";
            WheelConfigSO wheel = AssetDatabase.LoadAssetAtPath<WheelConfigSO>(path);
            if (wheel == null)
            {
                wheel = ScriptableObject.CreateInstance<WheelConfigSO>();
                AssetDatabase.CreateAsset(wheel, path);
            }

            SerializedObject so = new SerializedObject(wheel);
            so.FindProperty("wheelType").enumValueIndex = (int)type;
            if (baseSprite != null) so.FindProperty("wheelBaseSprite").objectReferenceValue = baseSprite;
            if (indicator != null) so.FindProperty("indicatorSprite").objectReferenceValue = indicator;

            SerializedProperty slicesProp = so.FindProperty("slices");
            slicesProp.ClearArray();
            for (int i = 0; i < slices.Length; i++)
            {
                slicesProp.InsertArrayElementAtIndex(i);
                SerializedProperty elem = slicesProp.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("sliceIndex").intValue = i;
                elem.FindPropertyRelative("reward").objectReferenceValue = slices[i].RewardSO;
                elem.FindPropertyRelative("amount").intValue = slices[i].Amount;
                elem.FindPropertyRelative("weight").floatValue = slices[i].Weight;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(wheel);
            return wheel;
        }

        private static ZoneManagerConfigSO CreateZoneManagerConfig(string fileName, WheelConfigSO normal, WheelConfigSO silver, WheelConfigSO gold)
        {
            string path = $"{ZonesPath}/{fileName}.asset";
            ZoneManagerConfigSO zoneConfig = AssetDatabase.LoadAssetAtPath<ZoneManagerConfigSO>(path);
            if (zoneConfig == null)
            {
                zoneConfig = ScriptableObject.CreateInstance<ZoneManagerConfigSO>();
                AssetDatabase.CreateAsset(zoneConfig, path);
            }

            SerializedObject so = new SerializedObject(zoneConfig);
            so.FindProperty("safeZoneInterval").intValue = 5;
            so.FindProperty("superZoneInterval").intValue = 30;
            so.FindProperty("defaultNormalWheel").objectReferenceValue = normal;
            so.FindProperty("defaultSilverWheel").objectReferenceValue = silver;
            so.FindProperty("defaultGoldenWheel").objectReferenceValue = gold;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(zoneConfig);
            return zoneConfig;
        }
    }
}
