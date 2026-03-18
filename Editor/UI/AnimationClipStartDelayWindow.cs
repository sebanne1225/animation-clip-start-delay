using UnityEditor;
using UnityEngine;

namespace Sebanne.AnimationClipStartDelay.Editor
{
    public sealed class AnimationClipStartDelayWindow : EditorWindow
    {
        private const string WindowTitle = "Animation Clip Start Delay";
        private const string MenuPath = "Tools/Sebanne/Animation Clip Start Delay";
        private const string DefaultStatusMessage = "まず Dry Run で処理内容を確認する運用を推奨します。";

        private AnimationClip _sourceClip;
        private DelayMode _delayMode = DelayMode.Seconds;
        private float _delaySeconds = 0.5f;
        private int _delayFrames = 15;
        private OutputLocationMode _outputLocationMode = OutputLocationMode.Generated;
        private string _customOutputFolder;
        private string _outputFileNameBase;
        private string _lastAutomaticOutputFileNameBase;
        private bool _hasManualOutputFileNameOverride;
        private Vector2 _scrollPosition;
        private string _statusMessage = DefaultStatusMessage;
        private MessageType _statusMessageType = MessageType.Info;
        private bool _hasCustomOutputError;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            var window = GetWindow<AnimationClipStartDelayWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(520f, 520f);
            window.Show();
        }

        private void OnEnable()
        {
            _customOutputFolder = AnimationClipStartDelayEditorPrefs.LoadCustomOutputFolder();
            SyncOutputFileNameWithAutomaticName();
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation Clip Start Delay", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("元Clipを変更せず、遅延版の新規Clipを生成します。", MessageType.Info);

            EditorGUILayout.Space();
            _sourceClip = (AnimationClip)EditorGUILayout.ObjectField("Source Clip（元Clip）", _sourceClip, typeof(AnimationClip), false);

            EditorGUILayout.Space(10f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("入力", EditorStyles.boldLabel);
            _delayMode = (DelayMode)EditorGUILayout.EnumPopup("Delay Mode", _delayMode);
            EditorGUILayout.LabelField("遅らせる単位を選びます。Seconds は秒、Frames はフレームです。", EditorStyles.wordWrappedMiniLabel);
            DrawDelayFieldCard(
                "Delay Seconds",
                _delayMode == DelayMode.Seconds,
                () => { _delaySeconds = EditorGUILayout.FloatField("秒数", _delaySeconds); });
            DrawDelayFieldCard(
                "Delay Frames",
                _delayMode == DelayMode.Frames,
                () => { _delayFrames = EditorGUILayout.IntField("フレーム数", _delayFrames); });
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("出力", EditorStyles.boldLabel);
            var previousOutputLocationMode = _outputLocationMode;
            _outputLocationMode = (OutputLocationMode)EditorGUILayout.EnumPopup("Output Location（保存先）", _outputLocationMode);
            EditorGUILayout.LabelField("保存先を選びます。Generated / 元Clipと同じ場所 / 任意フォルダ から選べます。", EditorStyles.wordWrappedMiniLabel);
            if (previousOutputLocationMode != _outputLocationMode &&
                _outputLocationMode != OutputLocationMode.Custom &&
                _hasCustomOutputError)
            {
                ResetStatusMessage();
            }

            if (_outputLocationMode == OutputLocationMode.Custom)
            {
                DrawCustomFolderField();
            }

            SyncOutputFileNameWithAutomaticName();

            EditorGUILayout.Space();
            DrawOutputFileNameField();

            EditorGUILayout.Space();
            DrawOutputPathPreview();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("実行", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Dry Run", GUILayout.Height(32f)))
            {
                RunDryRun();
            }

            if (GUILayout.Button("Generate", GUILayout.Height(32f)))
            {
                RunGenerate();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(_statusMessage, _statusMessageType);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void DrawDelayFieldCard(string label, bool isActive, System.Action drawField)
        {
            var previousColor = GUI.backgroundColor;
            var previousGuiColor = GUI.color;
            GUI.backgroundColor = isActive ? new Color(0.80f, 0.92f, 1.00f) : previousColor;
            GUI.color = isActive ? previousGuiColor : new Color(previousGuiColor.r, previousGuiColor.g, previousGuiColor.b, 0.65f);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = previousColor;
            EditorGUILayout.LabelField(label, isActive ? EditorStyles.miniBoldLabel : EditorStyles.miniLabel);
            using (new EditorGUI.DisabledScope(!isActive))
            {
                drawField.Invoke();
            }

            if (isActive)
            {
                EditorGUILayout.LabelField("現在この値を使用します", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
            GUI.color = previousGuiColor;
        }

        private void DrawCustomFolderField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            _customOutputFolder = EditorGUILayout.TextField("Custom Output Folder（任意フォルダ）", _customOutputFolder);
            if (EditorGUI.EndChangeCheck())
            {
                AnimationClipStartDelayEditorPrefs.SaveCustomOutputFolder(_customOutputFolder);
            }

            if (GUILayout.Button("選択...", GUILayout.Width(88f)))
            {
                SelectCustomFolder();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawOutputPathPreview()
        {
            var request = CreateRequest();

            if (_sourceClip == null)
            {
                EditorGUILayout.HelpBox("Source Clip を指定すると最終出力パスを確認できます。", MessageType.None);
                return;
            }

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("最終出力パス", EditorStyles.boldLabel);

            if (AnimationClipStartDelayPathResolver.TryResolvePreviewOutputPath(request, out var previewPath, out var previewError))
            {
                EditorGUILayout.SelectableLabel(previewPath, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f));
            }
            else
            {
                EditorGUILayout.HelpBox(previewError, MessageType.Warning);
            }
        }

        private void DrawOutputFileNameField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var editedValue = EditorGUILayout.TextField("Output File Name（保存名）", _outputFileNameBase);
            if (EditorGUI.EndChangeCheck())
            {
                _outputFileNameBase = AnimationClipStartDelayPathResolver.NormalizeOutputFileNameBase(editedValue);
                _hasManualOutputFileNameOverride = HasManualOutputFileNameOverride();
            }

            GUILayout.Label(".anim", GUILayout.Width(44f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("拡張子 .anim は自動で付与されます。", EditorStyles.miniLabel);
        }

        private void RunDryRun()
        {
            var request = CreateRequest();
            if (AnimationClipStartDelayProcessor.TryDryRun(request, out var analysis, out var errorMessage))
            {
                _statusMessage = "Dry Run が完了しました。Console で詳細ログを確認してください。\nOutput Path: " + analysis.OutputPath;
                _statusMessageType = MessageType.Info;
                _hasCustomOutputError = false;
                return;
            }

            _statusMessage = errorMessage;
            _statusMessageType = MessageType.Error;
            _hasCustomOutputError = IsCustomOutputError(errorMessage);
        }

        private void RunGenerate()
        {
            var request = CreateRequest();
            if (AnimationClipStartDelayProcessor.TryGenerate(request, out var analysis, out var errorMessage))
            {
                var createdAsset = AssetDatabase.LoadMainAssetAtPath(analysis.OutputPath);
                if (createdAsset != null)
                {
                    EditorGUIUtility.PingObject(createdAsset);
                }

                _statusMessage = "Generate が完了しました。Project ウィンドウで生成ファイルを確認してください。\nCreated Clip: " + analysis.OutputPath;
                _statusMessageType = MessageType.Info;
                _hasCustomOutputError = false;
                return;
            }

            _statusMessage = errorMessage;
            _statusMessageType = MessageType.Error;
            _hasCustomOutputError = IsCustomOutputError(errorMessage);
        }

        private AnimationClipStartDelayRequest CreateRequest()
        {
            return new AnimationClipStartDelayRequest
            {
                SourceClip = _sourceClip,
                DelayMode = _delayMode,
                DelaySeconds = _delaySeconds,
                DelayFrames = _delayFrames,
                OutputLocationMode = _outputLocationMode,
                CustomOutputFolder = _customOutputFolder,
                OutputFileNameBase = _outputFileNameBase,
            };
        }

        private void SelectCustomFolder()
        {
            var initialFolder = AnimationClipStartDelayFolderUtility.GetAbsoluteFolderPath(_customOutputFolder);
            var selectedFolder = EditorUtility.OpenFolderPanel("Custom Output Folder", initialFolder, string.Empty);
            if (string.IsNullOrWhiteSpace(selectedFolder))
            {
                return;
            }

            if (!AnimationClipStartDelayFolderUtility.TryAbsolutePathToAssetPath(selectedFolder, out var assetPath))
            {
                _statusMessage = "Custom 出力先は Assets/ 配下のみ選択できます。";
                _statusMessageType = MessageType.Error;
                _hasCustomOutputError = true;
                AnimationClipStartDelayLog.Error(_statusMessage);
                return;
            }

            _customOutputFolder = assetPath;
            AnimationClipStartDelayEditorPrefs.SaveCustomOutputFolder(assetPath);
            _statusMessage = "Custom 出力先を更新しました。";
            _statusMessageType = MessageType.Info;
            _hasCustomOutputError = false;
        }

        private void ResetStatusMessage()
        {
            _statusMessage = DefaultStatusMessage;
            _statusMessageType = MessageType.Info;
            _hasCustomOutputError = false;
        }

        private static bool IsCustomOutputError(string errorMessage)
        {
            return !string.IsNullOrWhiteSpace(errorMessage) &&
                   (errorMessage.Contains("Custom Output Folder") ||
                    errorMessage.Contains("Custom 出力先") ||
                    errorMessage.Contains("Assets/ 配下"));
        }

        private void SyncOutputFileNameWithAutomaticName()
        {
            if (_sourceClip == null)
            {
                return;
            }

            var automaticBaseName = AnimationClipStartDelayPathResolver.BuildAutomaticOutputFileNameBase(
                _sourceClip,
                _delayMode,
                _delaySeconds,
                _delayFrames);

            if (!_hasManualOutputFileNameOverride ||
                string.IsNullOrWhiteSpace(_outputFileNameBase) ||
                _outputFileNameBase == _lastAutomaticOutputFileNameBase)
            {
                _outputFileNameBase = automaticBaseName;
                _hasManualOutputFileNameOverride = false;
            }

            _lastAutomaticOutputFileNameBase = automaticBaseName;
        }

        private bool HasManualOutputFileNameOverride()
        {
            if (_sourceClip == null)
            {
                return !string.IsNullOrWhiteSpace(_outputFileNameBase);
            }

            var automaticBaseName = AnimationClipStartDelayPathResolver.BuildAutomaticOutputFileNameBase(
                _sourceClip,
                _delayMode,
                _delaySeconds,
                _delayFrames);

            _lastAutomaticOutputFileNameBase = automaticBaseName;
            return _outputFileNameBase != automaticBaseName;
        }
    }
}
