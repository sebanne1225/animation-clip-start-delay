using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Sebanne.AnimationClipStartDelay.Editor
{
    internal static class AnimationClipStartDelayPathResolver
    {
        internal const string GeneratedOutputFolder = "Assets/Sebanne/Generated/AnimationClipStartDelay";

        public static string BuildAutomaticOutputFileNameBase(AnimationClip sourceClip, DelayMode delayMode, float delaySeconds, int delayFrames)
        {
            return Path.GetFileNameWithoutExtension(BuildOutputFileName(sourceClip, delayMode, delaySeconds, delayFrames));
        }

        public static string BuildOutputFileName(AnimationClip sourceClip, DelayMode delayMode, float delaySeconds, int delayFrames)
        {
            var sourceName = sourceClip != null ? sourceClip.name : "animation_clip";
            return BuildOutputFileName(sourceName, delayMode, delaySeconds, delayFrames);
        }

        public static string NormalizeOutputFileNameBase(string outputFileNameBase)
        {
            if (string.IsNullOrWhiteSpace(outputFileNameBase))
            {
                return string.Empty;
            }

            var normalized = outputFileNameBase.Trim();
            if (normalized.EndsWith(".anim", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(0, normalized.Length - ".anim".Length);
            }

            return normalized.Trim();
        }

        public static bool TryResolvePreviewOutputPath(
            AnimationClipStartDelayRequest request,
            out string outputPath,
            out string errorMessage)
        {
            outputPath = string.Empty;
            errorMessage = string.Empty;

            if (request == null || request.SourceClip == null)
            {
                errorMessage = "Source Clip を指定すると出力先プレビューを表示できます。";
                return false;
            }

            if (!TryResolveOutputDirectory(request, out var directoryPath, out errorMessage))
            {
                return false;
            }

            if (!TryBuildOutputFileName(request.OutputFileNameBase, out var fileName, out errorMessage))
            {
                return false;
            }

            outputPath = CombineAssetPath(directoryPath, fileName);
            return true;
        }

        public static bool TryResolveOutputPath(
            AnimationClipStartDelayRequest request,
            AnimationClipStartDelayValidationResult validationResult,
            out AnimationClipStartDelayResolvedOutput resolvedOutput,
            out string errorMessage)
        {
            resolvedOutput = null;
            errorMessage = string.Empty;

            if (request == null || validationResult == null)
            {
                errorMessage = "出力先の解決に必要な情報が不足しています。";
                return false;
            }

            if (!TryResolveOutputDirectory(request, out var directoryPath, out errorMessage))
            {
                return false;
            }

            if (!TryBuildOutputFileName(request.OutputFileNameBase, out var fileName, out errorMessage))
            {
                return false;
            }

            var assetPath = CombineAssetPath(directoryPath, fileName);

            if (AssetDatabase.LoadMainAssetAtPath(assetPath) != null)
            {
                errorMessage = "同名ファイルが既に存在するため生成を中断しました。既存ファイルを移動 / 削除 / リネームするか、Output File Name を変えるか、Delay 値を変えるか、Output Location Mode を変えてください: " + assetPath;
                return false;
            }

            resolvedOutput = new AnimationClipStartDelayResolvedOutput
            {
                DirectoryPath = directoryPath,
                AssetPath = assetPath,
                FileName = fileName,
            };

            return true;
        }

        public static void EnsureFolderExists(string assetFolderPath)
        {
            var normalizedPath = NormalizeAssetPath(assetFolderPath);
            if (string.IsNullOrWhiteSpace(normalizedPath) || AssetDatabase.IsValidFolder(normalizedPath))
            {
                return;
            }

            var parts = normalizedPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || !string.Equals(parts[0], "Assets", StringComparison.Ordinal))
            {
                return;
            }

            var currentPath = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var nextPath = currentPath + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                }

                currentPath = nextPath;
            }
        }

        private static string BuildOutputFileName(string sourceName, DelayMode delayMode, float delaySeconds, int delayFrames)
        {
            string suffix;
            switch (delayMode)
            {
                case DelayMode.Seconds:
                    suffix = SanitizeDelayToken(delaySeconds.ToString("0.00###", CultureInfo.InvariantCulture)) + "s";
                    break;

                case DelayMode.Frames:
                    suffix = delayFrames.ToString(CultureInfo.InvariantCulture) + "f";
                    break;

                default:
                    suffix = "delay";
                    break;
            }

            return sourceName + "__delay_" + suffix + ".anim";
        }

        private static bool TryBuildOutputFileName(string outputFileNameBase, out string fileName, out string errorMessage)
        {
            fileName = string.Empty;
            errorMessage = string.Empty;

            var normalizedBaseName = NormalizeOutputFileNameBase(outputFileNameBase);
            if (string.IsNullOrWhiteSpace(normalizedBaseName))
            {
                errorMessage = "Output File Name を入力してください。";
                return false;
            }

            if (normalizedBaseName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                errorMessage = "Output File Name に使えない文字が含まれています。";
                return false;
            }

            fileName = normalizedBaseName + ".anim";
            return true;
        }

        private static bool TryResolveOutputDirectory(
            AnimationClipStartDelayRequest request,
            out string directoryPath,
            out string errorMessage)
        {
            directoryPath = string.Empty;
            errorMessage = string.Empty;

            switch (request.OutputLocationMode)
            {
                case OutputLocationMode.Generated:
                    directoryPath = GeneratedOutputFolder;
                    return true;

                case OutputLocationMode.SameAsSource:
                    var sourceAssetPath = AssetDatabase.GetAssetPath(request.SourceClip);
                    if (string.IsNullOrWhiteSpace(sourceAssetPath))
                    {
                        errorMessage = "Same As Source を使うには source asset path が必要です。";
                        return false;
                    }

                    directoryPath = NormalizeAssetPath(Path.GetDirectoryName(sourceAssetPath));
                    if (string.IsNullOrWhiteSpace(directoryPath))
                    {
                        directoryPath = "Assets";
                    }

                    return true;

                case OutputLocationMode.Custom:
                    var customFolder = NormalizeAssetPath(request.CustomOutputFolder);
                    if (string.IsNullOrWhiteSpace(customFolder))
                    {
                        errorMessage = "Custom Output Folder を指定してください。";
                        return false;
                    }

                    if (!IsAssetsPath(customFolder))
                    {
                        errorMessage = "Custom 出力先は Assets/ 配下のみ指定できます。";
                        return false;
                    }

                    directoryPath = customFolder;
                    return true;

                default:
                    errorMessage = "Output Location Mode が不正です。";
                    return false;
            }
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return string.Empty;
            }

            var normalized = assetPath.Replace('\\', '/').Trim();
            while (normalized.EndsWith("/", StringComparison.Ordinal) && normalized.Length > "Assets".Length)
            {
                normalized = normalized.Substring(0, normalized.Length - 1);
            }

            return normalized;
        }

        private static bool IsAssetsPath(string assetPath)
        {
            return string.Equals(assetPath, "Assets", StringComparison.OrdinalIgnoreCase) ||
                   assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase);
        }

        private static string CombineAssetPath(string directoryPath, string fileName)
        {
            return NormalizeAssetPath(directoryPath) + "/" + fileName;
        }

        private static string SanitizeDelayToken(string value)
        {
            return value.Replace('-', '_').Replace('.', 'p');
        }
    }
}
