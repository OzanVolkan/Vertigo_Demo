using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Vertigo.Editor
{
    /// <summary>
    /// Prepares the Game view for crisp UI previews on the three target aspect ratios.
    /// <para>
    /// Pixelated UI in the editor usually comes from the Game view itself, not from the game:
    /// "Low Resolution Aspect Ratios" renders at a reduced resolution and the result is then
    /// upscaled with point filtering. Fixed-resolution presets avoid that entirely.
    /// </para>
    /// Unity exposes no public API for Game view sizes, so this uses reflection and fails gracefully.
    /// </summary>
    public static class VertigoGameViewSetup
    {
        private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const int FixedResolutionSizeType = 1; // UnityEditor.GameViewSizeType.FixedResolution

        private static readonly (int Width, int Height, string Label)[] TestResolutions =
        {
            (2400, 1080, "Vertigo 20:9"),
            (1920, 1080, "Vertigo 16:9"),
            (2048, 1536, "Vertigo 4:3"),
        };

        [MenuItem("Tools/Vertigo/Configure Game View (20:9, 16:9, 4:3)", priority = 20)]
        public static void ConfigureGameView()
        {
            try
            {
                int defaultSizeIndex = RegisterTestResolutions();
                ApplyToGameView(defaultSizeIndex);
                Debug.Log("[Vertigo] Game view configured: added 20:9 / 16:9 / 4:3 fixed resolutions and disabled 'Low Resolution Aspect Ratios'.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Vertigo] Could not configure the Game view automatically ({e.Message}). " +
                                 "Set it manually: Game view resolution dropdown -> uncheck 'Low Resolution Aspect Ratios', " +
                                 "add 2400x1080 / 1920x1080 / 2048x1536 fixed resolutions and keep Scale at or below 1x.");
            }
        }

        /// <summary>Adds the test resolutions to the active Game view size group and returns the index of the 16:9 preset.</summary>
        private static int RegisterTestResolutions()
        {
            Assembly editorAssembly = typeof(UnityEditor.Editor).Assembly;
            Type sizesType = editorAssembly.GetType("UnityEditor.GameViewSizes", throwOnError: true);
            Type sizeType = editorAssembly.GetType("UnityEditor.GameViewSize", throwOnError: true);
            Type sizeTypeEnum = editorAssembly.GetType("UnityEditor.GameViewSizeType", throwOnError: true);

            object sizes = typeof(ScriptableSingleton<>).MakeGenericType(sizesType)
                .GetProperty("instance", BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);

            object group = sizesType.GetProperty("currentGroup", AnyInstance)?.GetValue(sizes)
                           ?? sizesType.GetMethod("GetGroup", AnyInstance).Invoke(sizes, new object[] { GameViewSizeGroupType.Standalone });

            Type groupType = group.GetType();
            MethodInfo getTotalCount = groupType.GetMethod("GetTotalCount", AnyInstance);
            MethodInfo getSize = groupType.GetMethod("GetGameViewSize", AnyInstance);
            MethodInfo addCustomSize = groupType.GetMethod("AddCustomSize", AnyInstance);

            int defaultIndex = -1;
            foreach (var resolution in TestResolutions)
            {
                int index = FindSizeIndex(group, getTotalCount, getSize, resolution.Width, resolution.Height);
                if (index < 0)
                {
                    object enumValue = Enum.ToObject(sizeTypeEnum, FixedResolutionSizeType);
                    object newSize = Activator.CreateInstance(sizeType, enumValue, resolution.Width, resolution.Height, resolution.Label);
                    addCustomSize.Invoke(group, new[] { newSize });
                    index = (int)getTotalCount.Invoke(group, null) - 1;
                }

                if (resolution.Width == 1920 && resolution.Height == 1080) defaultIndex = index;
            }

            sizesType.GetMethod("SaveToHDD", AnyInstance)?.Invoke(sizes, null);
            return defaultIndex;
        }

        private static int FindSizeIndex(object group, MethodInfo getTotalCount, MethodInfo getSize, int width, int height)
        {
            int count = (int)getTotalCount.Invoke(group, null);
            for (int i = 0; i < count; i++)
            {
                object size = getSize.Invoke(group, new object[] { i });
                Type t = size.GetType();
                int w = (int)t.GetProperty("width", AnyInstance).GetValue(size);
                int h = (int)t.GetProperty("height", AnyInstance).GetValue(size);
                int sizeKind = Convert.ToInt32(t.GetProperty("sizeType", AnyInstance).GetValue(size));

                if (w == width && h == height && sizeKind == FixedResolutionSizeType) return i;
            }
            return -1;
        }

        private static void ApplyToGameView(int sizeIndex)
        {
            Type gameViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView", throwOnError: true);
            EditorWindow gameView = EditorWindow.GetWindow(gameViewType, false, null, false);

            // Render aspect-ratio presets at full resolution.
            gameViewType.GetProperty("lowResolutionForAspectRatios", AnyInstance)?.SetValue(gameView, false);

            // Select the 16:9 preset; SizeSelectionCallback also resets the zoom to fit the window.
            if (sizeIndex >= 0)
            {
                MethodInfo selectCallback = gameViewType.GetMethod("SizeSelectionCallback", AnyInstance);
                if (selectCallback != null)
                    selectCallback.Invoke(gameView, new object[] { sizeIndex, null });
                else
                    gameViewType.GetProperty("selectedSizeIndex", AnyInstance)?.SetValue(gameView, sizeIndex);
            }

            gameView.Repaint();
        }
    }
}
