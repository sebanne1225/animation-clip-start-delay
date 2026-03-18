using UnityEngine;

namespace Sebanne.AnimationClipStartDelay.Editor
{
    internal enum DelayMode
    {
        Seconds,
        Frames
    }

    internal enum OutputLocationMode
    {
        Generated,
        SameAsSource,
        Custom
    }

    internal sealed class AnimationClipStartDelayRequest
    {
        public AnimationClip SourceClip { get; set; }

        public DelayMode DelayMode { get; set; }

        public float DelaySeconds { get; set; }

        public int DelayFrames { get; set; }

        public OutputLocationMode OutputLocationMode { get; set; }

        public string CustomOutputFolder { get; set; }

        public string OutputFileNameBase { get; set; }
    }

    internal sealed class AnimationClipStartDelayValidationResult
    {
        public AnimationClip SourceClip { get; set; }

        public string SourceAssetPath { get; set; }

        public float DelaySeconds { get; set; }

        public float FrameRate { get; set; }

        public string DelayValueText { get; set; }
    }

    internal sealed class AnimationClipStartDelayResolvedOutput
    {
        public string DirectoryPath { get; set; }

        public string AssetPath { get; set; }

        public string FileName { get; set; }
    }

    internal sealed class AnimationClipStartDelayAnalysis
    {
        public AnimationClip SourceClip { get; set; }

        public DelayMode DelayMode { get; set; }

        public float DelaySeconds { get; set; }

        public int DelayFrames { get; set; }

        public float FrameRate { get; set; }

        public int FloatCurveCount { get; set; }

        public int ObjectReferenceCurveCount { get; set; }

        public int AnimationEventCount { get; set; }

        public float SourceLengthSeconds { get; set; }

        public float ResultLengthSeconds { get; set; }

        public string DelayValueText { get; set; }

        public string OutputPath { get; set; }
    }
}
