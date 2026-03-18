using System;
using System.IO;
using UnityEngine;

namespace Sebanne.AnimationClipStartDelay.Editor
{
    internal static class AnimationClipStartDelayFolderUtility
    {
        public static string GetAbsoluteFolderPath(string assetPath)
        {
            var normalizedAssetPath = string.IsNullOrWhiteSpace(assetPath) ? "Assets" : assetPath.Replace('\\', '/');
            if (string.Equals(normalizedAssetPath, "Assets", StringComparison.OrdinalIgnoreCase))
            {
                return Application.dataPath.Replace('\\', '/');
            }

            if (!normalizedAssetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                return Application.dataPath.Replace('\\', '/');
            }

            var relativePath = normalizedAssetPath.Substring("Assets".Length).TrimStart('/', '\\');
            return Path.GetFullPath(Path.Combine(Application.dataPath, relativePath)).Replace('\\', '/');
        }

        public static bool TryAbsolutePathToAssetPath(string absolutePath, out string assetPath)
        {
            assetPath = string.Empty;

            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return false;
            }

            var normalizedAbsolutePath = Path.GetFullPath(absolutePath).Replace('\\', '/');
            var normalizedAssetsPath = Path.GetFullPath(Application.dataPath).Replace('\\', '/');

            if (string.Equals(normalizedAbsolutePath, normalizedAssetsPath, StringComparison.OrdinalIgnoreCase))
            {
                assetPath = "Assets";
                return true;
            }

            if (!normalizedAbsolutePath.StartsWith(normalizedAssetsPath + "/", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            assetPath = "Assets/" + normalizedAbsolutePath.Substring(normalizedAssetsPath.Length + 1);
            return true;
        }
    }
}
