using UnityEditor;

namespace Sebanne.AnimationClipStartDelay.Editor
{
    internal static class AnimationClipStartDelayEditorPrefs
    {
        private const string CustomOutputFolderKey = "Sebanne.AnimationClipStartDelay.CustomOutputFolder";

        public static string LoadCustomOutputFolder()
        {
            return EditorPrefs.GetString(CustomOutputFolderKey, "Assets");
        }

        public static void SaveCustomOutputFolder(string assetPath)
        {
            if (!string.IsNullOrWhiteSpace(assetPath))
            {
                EditorPrefs.SetString(CustomOutputFolderKey, assetPath);
            }
        }
    }
}
