using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;

namespace Vertigo.Editor
{
    public static class VertigoAssetSetupUtility
    {
        private const string DemoContentPath = "Assets/_Project/2D/demo_content";
        private const string CustomUIPath = "Assets/_Project/2D/Custom_UI";
        private const string AtlasPath = "Assets/_Project/Art/Atlases/VertigoDemoAtlas.spriteatlasv2";
        private const int AtlasPadding = 8;          // Extra padding prevents neighbour bleeding at lower mip levels
        private const int MaxAtlasSpriteSize = 1024; // Larger textures (full-screen VFX) stay out of the atlas

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

            string[] searchFolders = Directory.Exists(CustomUIPath)
                ? new[] { DemoContentPath, CustomUIPath }
                : new[] { DemoContentPath };
            string[] assetGuids = AssetDatabase.FindAssets("t:Texture", searchFolders);
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

                // Icons are authored at 512-720px but rendered at ~40-100px. Without mipmaps,
                // bilinear sampling skips texels when minifying heavily, which shows up as jagged/pixelated edges.
                if (!importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = true;
                    importer.mipmapFilter = TextureImporterMipFilter.KaiserFilter;
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
            if (name.Contains("ui_badge_frame"))
            {
                return new Vector4(48, 48, 48, 48);
            }
            if (name.Contains("ui_card_frame_12px"))
            {
                return new Vector4(28, 28, 28, 28);
            }
            if (name.Contains("ui_card_frame_4px"))
            {
                return new Vector4(4, 4, 4, 4);
            }
            if (name.Contains("ui_card_frame_gardient"))
            {
                return new Vector4(28, 28, 28, 28);
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

            // Sprite Atlas V2: the asset holds only the packable list, settings live on the importer.
            SpriteAtlasAsset atlasAsset = new SpriteAtlasAsset();
            atlasAsset.Add(CollectPackableSprites());
            SpriteAtlasAsset.Save(atlasAsset, AtlasPath);
            AssetDatabase.ImportAsset(AtlasPath, ImportAssetOptions.ForceUpdate);

            if (AssetImporter.GetAtPath(AtlasPath) is SpriteAtlasImporter atlasImporter)
            {
                // Rotation and tight packing are not supported by UGUI Image and break slicing/rendering.
                atlasImporter.packingSettings = new SpriteAtlasPackingSettings
                {
                    blockOffset = 1,
                    enableRotation = false,
                    enableTightPacking = false,
                    padding = AtlasPadding
                };

                atlasImporter.textureSettings = new SpriteAtlasTextureSettings
                {
                    readable = false,
                    generateMipMaps = true,
                    sRGB = true,
                    filterMode = FilterMode.Bilinear,
                    anisoLevel = 1
                };

                atlasImporter.includeInBuild = true;
                atlasImporter.SaveAndReimport();
            }

            Debug.Log($"[Vertigo] SpriteAtlas (V2) created/updated at {AtlasPath}");
        }

        /// <summary>
        /// Collects UI sprites for the atlas. Very large textures (e.g. full-screen VFX flashes)
        /// are excluded since they would waste most of an atlas page on their own.
        /// </summary>
        private static Object[] CollectPackableSprites()
        {
            var packables = new List<Object>();
            string[] searchFolders = Directory.Exists(CustomUIPath)
                ? new[] { DemoContentPath, CustomUIPath }
                : new[] { DemoContentPath };
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", searchFolders);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;

                importer.GetSourceTextureWidthAndHeight(out int width, out int height);
                if (Mathf.Max(width, height) > MaxAtlasSpriteSize) continue;

                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null) packables.Add(texture);
            }

            return packables.ToArray();
        }
    }
}
