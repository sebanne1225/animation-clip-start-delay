using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Sebanne.AnimationClipStartDelay.Editor
{
    internal static class AnimationClipStartDelayProcessor
    {
        public static bool TryDryRun(
            AnimationClipStartDelayRequest request,
            out AnimationClipStartDelayAnalysis analysis,
            out string errorMessage)
        {
            analysis = null;
            errorMessage = string.Empty;

            if (!TryAnalyze(request, out analysis, out errorMessage))
            {
                AnimationClipStartDelayLog.Error(errorMessage);
                return false;
            }

            AnimationClipStartDelayLog.Info("Dry Run Start");
            LogAnalysis(analysis);
            AnimationClipStartDelayLog.Info("Dry Run Complete");
            return true;
        }

        public static bool TryGenerate(
            AnimationClipStartDelayRequest request,
            out AnimationClipStartDelayAnalysis analysis,
            out string errorMessage)
        {
            analysis = null;
            errorMessage = string.Empty;

            if (!TryAnalyze(request, out analysis, out errorMessage))
            {
                AnimationClipStartDelayLog.Error(errorMessage);
                return false;
            }

            AnimationClipStartDelayLog.Info("Generate Start");

            AnimationClip generatedClip = null;

            try
            {
                AnimationClipStartDelayPathResolver.EnsureFolderExists(Path.GetDirectoryName(analysis.OutputPath));

                generatedClip = UnityEngine.Object.Instantiate(request.SourceClip);
                generatedClip.name = Path.GetFileNameWithoutExtension(analysis.OutputPath);

                ApplyDelay(generatedClip, request.SourceClip, analysis.DelaySeconds);

                AssetDatabase.CreateAsset(generatedClip, analysis.OutputPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(analysis.OutputPath);

                AnimationClipStartDelayLog.Info("Created Clip: " + analysis.OutputPath);
                AnimationClipStartDelayLog.Info("Applied Float Curves: " + analysis.FloatCurveCount.ToString(CultureInfo.InvariantCulture));
                AnimationClipStartDelayLog.Info("Applied Object Reference Curves: " + analysis.ObjectReferenceCurveCount.ToString(CultureInfo.InvariantCulture));
                AnimationClipStartDelayLog.Info("Applied Animation Events: " + analysis.AnimationEventCount.ToString(CultureInfo.InvariantCulture));
                AnimationClipStartDelayLog.Info("Source Length: " + FormatSeconds(analysis.SourceLengthSeconds));
                AnimationClipStartDelayLog.Info("Result Length: " + FormatSeconds(analysis.ResultLengthSeconds));
                AnimationClipStartDelayLog.Info("Generate Complete");
                return true;
            }
            catch (Exception exception)
            {
                if (analysis != null && !string.IsNullOrWhiteSpace(analysis.OutputPath))
                {
                    var existingAsset = AssetDatabase.LoadMainAssetAtPath(analysis.OutputPath);
                    if (existingAsset != null)
                    {
                        AssetDatabase.DeleteAsset(analysis.OutputPath);
                    }
                }

                if (generatedClip != null)
                {
                    UnityEngine.Object.DestroyImmediate(generatedClip);
                }

                errorMessage = "AnimationClip の生成中に例外が発生しました: " + exception.Message;
                AnimationClipStartDelayLog.Error(errorMessage);
                return false;
            }
        }

        private static bool TryAnalyze(
            AnimationClipStartDelayRequest request,
            out AnimationClipStartDelayAnalysis analysis,
            out string errorMessage)
        {
            analysis = null;
            errorMessage = string.Empty;

            if (!AnimationClipStartDelayValidator.TryValidate(request, out var validationResult, out errorMessage))
            {
                return false;
            }

            if (!AnimationClipStartDelayPathResolver.TryResolveOutputPath(request, validationResult, out var resolvedOutput, out errorMessage))
            {
                return false;
            }

            var floatCurveCount = AnimationUtility.GetCurveBindings(request.SourceClip).Length;
            var objectReferenceCurveCount = AnimationUtility.GetObjectReferenceCurveBindings(request.SourceClip).Length;
            var animationEventCount = AnimationUtility.GetAnimationEvents(request.SourceClip).Length;
            var sourceLength = request.SourceClip.length;
            var resultLength = sourceLength + validationResult.DelaySeconds;

            analysis = new AnimationClipStartDelayAnalysis
            {
                SourceClip = request.SourceClip,
                DelayMode = request.DelayMode,
                DelaySeconds = validationResult.DelaySeconds,
                DelayFrames = request.DelayFrames,
                FrameRate = validationResult.FrameRate,
                FloatCurveCount = floatCurveCount,
                ObjectReferenceCurveCount = objectReferenceCurveCount,
                AnimationEventCount = animationEventCount,
                SourceLengthSeconds = sourceLength,
                ResultLengthSeconds = resultLength,
                DelayValueText = validationResult.DelayValueText,
                OutputPath = resolvedOutput.AssetPath,
            };

            return true;
        }

        private static void ApplyDelay(AnimationClip targetClip, AnimationClip sourceClip, float delaySeconds)
        {
            var floatBindings = AnimationUtility.GetCurveBindings(sourceClip);
            for (var i = 0; i < floatBindings.Length; i++)
            {
                var binding = floatBindings[i];
                var sourceCurve = AnimationUtility.GetEditorCurve(sourceClip, binding);
                if (sourceCurve == null)
                {
                    continue;
                }

                AnimationUtility.SetEditorCurve(targetClip, binding, ShiftCurve(sourceCurve, delaySeconds));
            }

            var objectReferenceBindings = AnimationUtility.GetObjectReferenceCurveBindings(sourceClip);
            for (var i = 0; i < objectReferenceBindings.Length; i++)
            {
                var binding = objectReferenceBindings[i];
                var sourceKeyframes = AnimationUtility.GetObjectReferenceCurve(sourceClip, binding);
                AnimationUtility.SetObjectReferenceCurve(targetClip, binding, ShiftObjectReferenceKeyframes(sourceKeyframes, delaySeconds));
            }

            var sourceEvents = AnimationUtility.GetAnimationEvents(sourceClip);
            AnimationUtility.SetAnimationEvents(targetClip, ShiftAnimationEvents(sourceEvents, delaySeconds));
            targetClip.EnsureQuaternionContinuity();
        }

        private static AnimationCurve ShiftCurve(AnimationCurve sourceCurve, float delaySeconds)
        {
            var sourceKeys = sourceCurve.keys;
            var shiftedKeys = new Keyframe[sourceKeys.Length];

            for (var i = 0; i < sourceKeys.Length; i++)
            {
                var key = sourceKeys[i];
                key.time += delaySeconds;
                shiftedKeys[i] = key;
            }

            var shiftedCurve = new AnimationCurve(shiftedKeys)
            {
                preWrapMode = sourceCurve.preWrapMode,
                postWrapMode = sourceCurve.postWrapMode,
            };

            return shiftedCurve;
        }

        private static ObjectReferenceKeyframe[] ShiftObjectReferenceKeyframes(ObjectReferenceKeyframe[] sourceKeyframes, float delaySeconds)
        {
            var shiftedKeyframes = new ObjectReferenceKeyframe[sourceKeyframes.Length];

            for (var i = 0; i < sourceKeyframes.Length; i++)
            {
                shiftedKeyframes[i] = new ObjectReferenceKeyframe
                {
                    time = sourceKeyframes[i].time + delaySeconds,
                    value = sourceKeyframes[i].value,
                };
            }

            return shiftedKeyframes;
        }

        private static AnimationEvent[] ShiftAnimationEvents(AnimationEvent[] sourceEvents, float delaySeconds)
        {
            var shiftedEvents = new AnimationEvent[sourceEvents.Length];

            for (var i = 0; i < sourceEvents.Length; i++)
            {
                var sourceEvent = sourceEvents[i];
                shiftedEvents[i] = new AnimationEvent
                {
                    functionName = sourceEvent.functionName,
                    stringParameter = sourceEvent.stringParameter,
                    floatParameter = sourceEvent.floatParameter,
                    intParameter = sourceEvent.intParameter,
                    objectReferenceParameter = sourceEvent.objectReferenceParameter,
                    messageOptions = sourceEvent.messageOptions,
                    time = sourceEvent.time + delaySeconds,
                };
            }

            return shiftedEvents;
        }

        private static void LogAnalysis(AnimationClipStartDelayAnalysis analysis)
        {
            AnimationClipStartDelayLog.Info("Source Clip: " + analysis.SourceClip.name);
            AnimationClipStartDelayLog.Info("Delay Mode: " + analysis.DelayMode);
            AnimationClipStartDelayLog.Info("Delay Value: " + analysis.DelayValueText);
            AnimationClipStartDelayLog.Info("Frame Rate: " + analysis.FrameRate.ToString("0.###", CultureInfo.InvariantCulture));
            AnimationClipStartDelayLog.Info("Float Curves: " + analysis.FloatCurveCount.ToString(CultureInfo.InvariantCulture));
            AnimationClipStartDelayLog.Info("Object Reference Curves: " + analysis.ObjectReferenceCurveCount.ToString(CultureInfo.InvariantCulture));
            AnimationClipStartDelayLog.Info("Animation Events: " + analysis.AnimationEventCount.ToString(CultureInfo.InvariantCulture));
            AnimationClipStartDelayLog.Info("Source Length: " + FormatSeconds(analysis.SourceLengthSeconds));
            AnimationClipStartDelayLog.Info("Result Length: " + FormatSeconds(analysis.ResultLengthSeconds));
            AnimationClipStartDelayLog.Info("Output Path: " + analysis.OutputPath);
        }

        private static string FormatSeconds(float seconds)
        {
            return seconds.ToString("0.000", CultureInfo.InvariantCulture) + " sec";
        }
    }
}
