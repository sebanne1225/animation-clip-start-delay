using UnityEngine;

namespace Sebanne.AnimationClipStartDelay.Editor
{
    internal static class AnimationClipStartDelayLog
    {
        private const string Prefix = "[ClipStartDelay] ";

        public static void Info(string message)
        {
            Debug.Log(Prefix + message);
        }

        public static void Warning(string message)
        {
            Debug.LogWarning(Prefix + message);
        }

        public static void Error(string message)
        {
            Debug.LogError(Prefix + message);
        }
    }
}
