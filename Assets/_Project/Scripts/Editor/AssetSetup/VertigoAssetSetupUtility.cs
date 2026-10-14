using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Vertigo.Editor
{
    public static class VertigoAssetSetupUtility
    {
        private const string DemoContentPath = "Assets/2D/demo_content";
        private const string AtlasPath = "Assets/_Project/Art/Atlases/VertigoDemoAtlas.spriteatlasv2";

        [InitializeOnLoadMethod]
        private static void OnProjectLoaded()
        {
            EditorApplication.delayCall += () =>
            {
                string defaultZoneConfig = "Assets/_Project/Data/Zones/ZoneConfig_Default.asset";
                string scenePath = "Assets/_Project/Scenes/MainGame.unity";
                if (!File.Exists(defaultZoneConfig) || !File.Exists(scenePath))
                {
                    Debug.Log("[Vertigo] Running automatic first-time setup...");
                    FullProjectSetup();
                }
            };
        }

        [MenuItem("Tools/Vertigo/Full Project Setup (Assets + Data + Scene)", priority = 0)]
        public static void FullProjectSetup()
        {
            SetupAllAssets();
            VertigoDataGenerator.GenerateAllData();
            VertigoSceneSetup.SetupMainGameScene();
            Debug.Log("[Vertigo] Full Project Setup successfully completed!");
        }

        [MenuItem("Tools/Vertigo/Setup Textures and Sprite Atlas", priority = 10)]
        public static void SetupAllAssets()
        {
            SetupTextures();
            CreateOrUpdateSpriteAtlas();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Vertigo] Textures and Sprite Atlas setup completed successfully!");
        }

        public static void SetupTextures()
        {
            if (!Directory.Exists(DemoContentPath))
            {
                Debug.LogWarning($"[Vertigo] Path not found: {DemoContentPath}");
                return;
            }

            string[] assetGuids = AssetDatabase.FindAssets("t:Texture", new[] { DemoContentPath });
            int modifiedCount = 0;

            foreach (string guid in assetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                if (importer == null) continue;

                bool modified = false;

                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    modified = true;
                }

                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    modified = true;
                }

                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    modified = true;
                }

                // Setup 9-slice borders for buttons and frames
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                Vector4 targetBorder = GetBorderForAsset(fileName);
                if (importer.spriteBorder != targetBorder)
                {
                    importer.spriteBorder = targetBorder;
                    modified = true;
                }

                if (modified)
                {
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    modifiedCount++;
                }
            }

            Debug.Log($"[Vertigo] Configured {modifiedCount} textures as Sprites with proper borders.");
        }

        private static Vector4 GetBorderForAsset(string name)
        {
            // Vector4 is: X = Left, Y = Bottom, Z = Right, W = Top
            if (name.Contains("button_orange") || name.Contains("button_grey"))
            {
                return new Vector4(20, 20, 20, 20);
            }
            if (name.Contains("ui_card_frame_12px"))
            {
                return new Vector4(12, 12, 12, 12);
            }
            if (name.Contains("ui_card_frame_4px"))
            {
                return new Vector4(4, 4, 4, 4);
            }
            if (name.Contains("ui_card_frame_gardient"))
            {
                return new Vector4(12, 12, 12, 12);
            }
            if (name.Contains("ui_card_panel_zone") || name.Contains("ui_card_zone_map"))
            {
                return new Vector4(8, 8, 8, 8);
            }

            return Vector4.zero;
        }

        public static void CreateOrUpdateSpriteAtlas()
        {
            string dir = Path.GetDirectoryName(AtlasPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AtlasPath);
            if (atlas == null)
            {
                atlas = new SpriteAtlas();
                AssetDatabase.CreateAsset(atlas, AtlasPath);
            }

            // Atlas settings for mobile performance
            SpriteAtlasPackingSettings packingSettings = new SpriteAtlasPackingSettings
            {
                blockOffset = 1,
                enableRotation = false,
                enableTightPacking = false,
                padding = 4
            };
            atlas.SetPackingSettings(packingSettings);

            SpriteAtlasTextureSettings textureSettings = new SpriteAtlasTextureSettings
            {
                readable = false,
                generateMipMaps = false,
                sRGB = true,
                filterMode = FilterMode.Bilinear
            };
            atlas.SetTextureSettings(textureSettings);

            // Add demo_content folder as packable
            Object demoFolderObj = AssetDatabase.LoadAssetAtPath<Object>(DemoContentPath);
            if (demoFolderObj != null)
            {
                atlas.Remove(atlas.GetPackables());
                atlas.Add(new[] { demoFolderObj });
                EditorUtility.SetDirty(atlas);
            }

            Debug.Log($"[Vertigo] SpriteAtlas created/updated at {AtlasPath}");
        }
    }
}
