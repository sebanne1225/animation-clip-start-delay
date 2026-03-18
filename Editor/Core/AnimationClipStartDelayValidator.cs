using System.Globalization;
using UnityEditor;

namespace Sebanne.AnimationClipStartDelay.Editor
{
    internal static class AnimationClipStartDelayValidator
    {
        private const float MinimumFrameRate = 0.0001f;

        public static bool TryValidate(
            AnimationClipStartDelayRequest request,
            out AnimationClipStartDelayValidationResult validationResult,
            out string errorMessage)
        {
            validationResult = null;
            errorMessage = string.Empty;

            if (request == null)
            {
                errorMessage = "リクエストが不正です。";
                return false;
            }

            if (request.SourceClip == null)
            {
                errorMessage = "Source Clip が未指定です。";
                return false;
            }

            var sourceAssetPath = AssetDatabase.GetAssetPath(request.SourceClip);
            if (string.IsNullOrWhiteSpace(sourceAssetPath))
            {
                errorMessage = "AnimationClip として処理できる source asset が見つかりません。";
                return false;
            }

            var frameRate = request.SourceClip.frameRate;
            float delaySeconds;
            string delayValueText;

            switch (request.DelayMode)
            {
                case DelayMode.Seconds:
                    if (request.DelaySeconds <= 0f || float.IsNaN(request.DelaySeconds) || float.IsInfinity(request.DelaySeconds))
                    {
                        errorMessage = "Delay Seconds は 0 より大きい値を指定してください。";
                        return false;
                    }

                    delaySeconds = request.DelaySeconds;
                    delayValueText = FormatSeconds(delaySeconds);
                    break;

                case DelayMode.Frames:
                    if (request.DelayFrames <= 0)
                    {
                        errorMessage = "Delay Frames は 1 以上を指定してください。";
                        return false;
                    }

                    if (frameRate <= MinimumFrameRate || float.IsNaN(frameRate) || float.IsInfinity(frameRate))
                    {
                        errorMessage = "Frames モードの換算に必要な frameRate を解決できません。";
                        return false;
                    }

                    delaySeconds = request.DelayFrames / frameRate;
                    delayValueText = request.DelayFrames.ToString(CultureInfo.InvariantCulture) + " frames (" + FormatSeconds(delaySeconds) + ")";
                    break;

                default:
                    errorMessage = "Delay Mode が不正です。";
                    return false;
            }

            validationResult = new AnimationClipStartDelayValidationResult
            {
                SourceClip = request.SourceClip,
                SourceAssetPath = sourceAssetPath,
                DelaySeconds = delaySeconds,
                FrameRate = frameRate,
                DelayValueText = delayValueText,
            };

            return true;
        }

        private static string FormatSeconds(float seconds)
        {
            return seconds.ToString("0.000", CultureInfo.InvariantCulture) + " sec";
        }
    }
}
