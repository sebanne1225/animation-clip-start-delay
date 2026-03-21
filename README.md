# Animation Clip Start Delay

## 概要

`Animation Clip Start Delay` は、重い MMD モーションなどの `AnimationClip` を手でずらす代わりに、非破壊で遅延版 Clip を生成する Unity Editor ツールです。

元Clipは変更せず、指定した秒数またはフレーム数ぶんだけ開始を後ろへずらした新規 `.anim` を作れます。Animation Window で大量キーを手動編集したくないケースを想定しています。

## 何ができるか

- 単一 `AnimationClip` を対象に、開始を遅らせた複製 Clip を生成できます
- Delay を `Seconds` / `Frames` のどちらでも指定できます
- float curve、object reference curve、`AnimationEvent` をまとめて同じ秒数ぶん後ろへずらします
- `Dry Run` で、対象件数、結果長、出力先を先に確認できます
- `Generated` / `Same As Source` / `Custom` の 3 種類の出力先を選べます

## 現在対応していること

- 単一 Clip のみ対応しています
- 先頭に待機時間を追加してから元の動きを始める方式に対応しています
- 元Clipは常に非破壊で、新規 asset のみ生成します
- 同名ファイルが存在する場合は上書きせず、安全に停止します
- Runtime 実装は最小限で、Editor 主体の package 構成です

## VCC / VPM での導入

1. VPM source として `https://sebanne1225.github.io/sebanne-listing/source.json` を追加します。
2. package 一覧から `Animation Clip Start Delay` (`com.sebanne.animation-clip-start-delay`) を追加します。
3. Unity を開き、package が導入されていることを確認します。

listing repo: `https://github.com/sebanne1225/sebanne-listing`

## 使い方

1. Unity 上部メニューの `Tools/Sebanne/Animation Clip Start Delay` を開きます。
2. `Source Clip` に対象 `AnimationClip` を指定します。
3. `Delay Mode` で `Seconds` または `Frames` を選びます。
4. 遅延値を入力します。
5. 必要なら `Output File Name` を調整します。
6. `Output Location` で出力先を選びます。
7. まず `Dry Run` を押して、ログで処理対象数と最終出力パスを確認します。
8. 問題がなければ `Generate` を押して新規 Clip を生成します。

## Output Location の説明

- `Generated`
  - 既定の出力先です
  - `Assets/Sebanne/AnimationClipStartDelay/Generated/` に出力します
- `Same As Source`
  - 元Clipと同じフォルダに出力します
- `Custom`
  - 任意フォルダを指定できます
  - `Assets/` 配下のみ許可します
  - 最後に選んだフォルダは Unity Editor 内で保持します

必要なフォルダは Generate 時に自動作成します。

## Dry Run / 診断

`Dry Run` では実ファイルを作らず、次の情報を Console ログに出します。

- Source Clip 名
- Delay Mode と Delay 値
- フレームレート
- float curve 数
- object reference curve 数
- `AnimationEvent` 数
- 元Clipの長さ
- 生成後の長さ
- 出力パス

ログ prefix は `[ClipStartDelay]` で統一しているため、Unity Console 上で追跡しやすい構成です。

## Release Asset

GitHub Release には、VPM 配布確認や手動保管に使える package zip を添付します。

- 例: `com.sebanne.animation-clip-start-delay-0.1.3.zip`

## 制限事項

- 複数 Clip の一括処理には未対応です
- Clip 全体の長さを維持したまま末尾を切る方式には未対応です
- 先頭で初期値を保持してから再生を始める方式には未対応です
- 再生プレビュー、AnimatorController 側の自動調整、Timeline 用特別対応は未実装です
- FBX 内蔵 Clip の直接編集は対象外で、常に新規 Clip を生成します

## ライセンス

MIT License です。詳細は `LICENSE` を参照してください。
