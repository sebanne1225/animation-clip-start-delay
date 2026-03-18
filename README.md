# Sebanne Animation Clip Start Delay

## 概要

`Sebanne Animation Clip Start Delay` は、重い MMD モーションなどの `AnimationClip` を手でずらす代わりに、非破壊で遅延版 clip を生成する Unity Editor ツールです。

元 clip は変更せず、指定した秒数またはフレーム数ぶんだけ開始を後ろへずらした新規 `.anim` を作れます。Animation Window で大量キーを手動編集したくないケースを想定しています。

## 何ができるか

- 単一 `AnimationClip` を対象に、開始を遅らせた複製 clip を生成できます
- Delay を `Seconds` / `Frames` のどちらでも指定できます
- float curve、object reference curve、`AnimationEvent` をまとめて同じ秒数ぶん後ろへずらします
- `Dry Run` で、対象件数、結果長、出力先を先に確認できます
- `Generated` / `Same As Source` / `Custom` の 3 種類の出力先を選べます

## 現在対応していること

- MVP 範囲として単一 clip のみ対応しています
- 遅延方式は「先頭に空白時間を足して、全キーを後ろへずらす」A 方式のみです
- 元 clip は常に非破壊で、新規 asset のみ生成します
- 同名ファイルが存在する場合は上書きせず、安全に停止します
- Runtime 実装は最小限で、Editor 主体の package 構成です

## 使い方

1. Unity 上部メニューの `Tools/Sebanne/Animation Clip Start Delay` を開きます。
2. `Source Clip` に対象 `AnimationClip` を指定します。
3. `Delay Mode` で `Seconds` または `Frames` を選びます。
4. 遅延値を入力します。
5. `Output Location Mode` で出力先を選びます。
6. まず `Dry Run` を押して、ログで処理対象数と最終出力パスを確認します。
7. 問題がなければ `Generate` を押して新規 clip を生成します。

## Output Location の説明

- `Generated`
  - 既定の出力先です
  - `Assets/Sebanne/Generated/AnimationClipStartDelay/` に出力します
- `Same As Source`
  - source clip と同じフォルダに出力します
- `Custom`
  - 任意フォルダを指定できます
  - `Assets/` 配下のみ許可します
  - 最後に選んだフォルダは EditorPrefs で保持します

必要なフォルダは Generate 時に自動作成します。

## Dry Run / 診断

`Dry Run` では実ファイルを作らず、次の情報を Console ログに出します。

- Source Clip 名
- Delay Mode と Delay 値
- frameRate
- float curve 数
- object reference curve 数
- `AnimationEvent` 数
- source length
- result length
- Output Path

ログ prefix は `[ClipStartDelay]` で統一しているため、Unity Console 上で追跡しやすい構成です。

## 制限事項

- 複数 clip 一括処理には未対応です
- 全長維持で末尾を切る B 方式には未対応です
- 先頭で初期値を保持する C 方式には未対応です
- 再生プレビュー、AnimatorController 側の自動調整、Timeline 用特別対応は未実装です
- FBX 内蔵 clip の直接編集は対象外で、常に新規 clip を生成します

## ライセンス

MIT License です。詳細は `LICENSE` を参照してください。
