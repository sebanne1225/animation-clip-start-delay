# CODEX_HANDOFF

> この文書は開発用のハンドオフ / 設計メモです。エンドユーザー向けの使い方や導入説明ではありません。公開向けの説明は `README.md` を参照してください。

## Goal
AnimationClip の開始を、指定した秒数またはフレーム数だけ遅らせた新規 Clip を生成する Unity Editor ツールを作る。

目的は、MMD 由来などのキーが非常に多い重い AnimationClip を、
Animation Window 上で手動編集せず、安全に複製生成できるようにすること。

このツールは「元 Clip を編集しない」「新規生成のみ」「Dry Run で内容確認できる」を重視する。

---

## Current Scope
今回は MVP として、以下の範囲だけ対応する。

- 単一 AnimationClip 対応
- 秒指定 / フレーム指定 両対応
- 遅延方式は A のみ
  - A = 先頭に空白時間を足す
  - 全キーを後ろへずらし、そのぶん Clip 全体が長くなる
- 元 Clip は非破壊
- 新規 Clip 生成
- Dry Run 実装
- ログ重視
- Unity EditorWindow として実装

---

## Non-Goals
今回はまだ対応しないこと:

- 複数 Clip 一括処理
- B方式（全長は維持し、末尾を切る）
- C方式（先頭で初期値ホールド）
- 再生プレビュー
- AnimatorController 側の自動調整
- FBX 内蔵 Clip の直接編集
- Timeline 用の特別対応
- Undo/Redo の高度対応

---

## Expected User Flow
1. Source Clip を指定
2. Delay Mode を選ぶ
   - Seconds
   - Frames
3. Delay 値を入力
4. 必要なら Output File Name を調整
5. Output Location を選ぶ
   - Generated
   - Same As Source
   - Custom
6. Dry Run を実行して、処理対象数と出力先を確認
7. Generate を実行して、新規 Clip を生成

---

## Output Location Rules

### Output Location
- Generated
- Same As Source
- Custom

### Default
- Generated

### Output Path Rules
- Generated:
  - `Assets/Sebanne/Generated/AnimationClipStartDelay/`
- Same As Source:
  - source clip と同じフォルダ
- Custom:
  - ユーザー指定フォルダ
  - `Assets/` 配下のみ許可

### Notes
- 必要なフォルダは自動作成する
- Dry Run / Generate の両方で最終出力パスをログ表示する
- 初回表示時は Generated を選択状態にする
- Custom の最後の選択先は EditorPrefs で保持してよい

---

## Delay Rules

### Delay Mode: Seconds
- ユーザーが入力した秒数をそのまま offset として使う

### Delay Mode: Frames
- `AnimationClip.frameRate` を使って秒換算する
- 例:
  - 30fps の clip で 15 frames = 0.5 sec

### Validation
- Delay 値が 0 以下なら中断
- Clip の frameRate が不正または 0 に近い場合は理由をログ表示して中断してよい

---

## Processing Rules

### Base Rule
AnimationClip 内の全ての時間情報を、指定秒数ぶん後ろへずらす。

### Targeted Data
以下を全件対象にする:

1. float curve
2. object reference curve
3. AnimationEvent

### Required Behavior
- すべての keyframe.time に delay を加算する
- object reference keyframe の time にも delay を加算する
- AnimationEvent.time にも delay を加算する
- 結果として clip length は delay 分だけ伸びる前提

### Important
見た目の float curve だけでなく、
object reference curve と AnimationEvent も必ず対象にすること。
一部だけずらしてズレが起きる状態を避けたい。

---

## Source Safety Rules
- 元 Clip は絶対に変更しない
- 必ず新規 AnimationClip を生成する
- Generate 前に source asset を直接書き換えない
- 同名ファイルが既に存在する場合、初期状態では上書きしない
- 上書きは今回は未対応でもよい
- 既存ファイルがある場合はエラー表示して中断でよい

---

## Naming Rules

### Output Name Suffix
Delay 表現が分かる suffix を付ける。

例:
- `<SourceName>__delay_0p50s.anim`
- `<SourceName>__delay_15f.anim`

### Notes
- 小数点はファイル名で扱いやすい形へ置換する
  - 例: `0.50` -> `0p50`
- 実際の suffix 仕様は多少調整してよいが、
  見て意味が分かる命名を優先すること

---

## Editor Window Requirements

### Main Fields
- Source Clip
- Delay Mode
- Delay Seconds
- Delay Frames
- Output File Name
- Output Location
- Custom Output Folder
- Dry Run button
- Generate button

### UI Behavior
- Delay Mode が Seconds の時だけ Delay Seconds 入力を強調
- Delay Mode が Frames の時だけ Delay Frames 入力を強調
- Output Location が Custom の時だけ folder 指定 UI を表示
- 現在の最終出力パスが分かる表示を入れる
- 初見でも意味が分かりやすい日本語 UI を優先する

### Menu
- `Tools/Sebanne/Animation Clip Start Delay`

---

## Dry Run Requirements

### Purpose
Generate 前に、
- 何を対象にするか
- どこに出すか
- 結果長がどうなるか
を確認できること。

### Dry Run Log Example
[ClipStartDelay] Dry Run Start
[ClipStartDelay] Source Clip: walk_mmd
[ClipStartDelay] Delay Mode: Seconds
[ClipStartDelay] Delay Value: 0.500 sec
[ClipStartDelay] Frame Rate: 30
[ClipStartDelay] Float Curves: 412
[ClipStartDelay] Object Reference Curves: 0
[ClipStartDelay] Animation Events: 2
[ClipStartDelay] Source Length: 3.267 sec
[ClipStartDelay] Result Length: 3.767 sec
[ClipStartDelay] Output Path: Assets/Sebanne/Generated/AnimationClipStartDelay/walk_mmd__delay_0p50s.anim
[ClipStartDelay] Dry Run Complete

### Dry Run Must Show
- Source Clip
- Delay Mode
- Delay Value
- Frame Rate
- Float Curves count
- Object Reference Curves count
- Animation Events count
- Source Length
- Result Length
- Output Path

---

## Generate Requirements

### Generate Behavior
- source clip から新規 clip を生成
- required curves / events をすべて offset して保存
- AssetDatabase に保存
- 完了ログを出す

### Generate Log Example
[ClipStartDelay] Generate Start
[ClipStartDelay] Created Clip: Assets/Sebanne/Generated/AnimationClipStartDelay/walk_mmd__delay_0p50s.anim
[ClipStartDelay] Applied Float Curves: 412
[ClipStartDelay] Applied Object Reference Curves: 0
[ClipStartDelay] Applied Animation Events: 2
[ClipStartDelay] Source Length: 3.267 sec
[ClipStartDelay] Result Length: 3.767 sec
[ClipStartDelay] Generate Complete

---

## Guard Rules
以下の場合は Generate を中断し、理由が分かるログを出すこと。

- Source Clip 未指定
- Delay 値が 0 以下
- Output Path の解決に失敗
- Custom 出力先が `Assets/` 配下ではない
- 同名ファイルが既に存在する
- AnimationClip として処理できない
- frameRate 解決不可で Frames モードの換算ができない

---

## Implementation Notes
- Editor 専用実装でよい
- Runtime 側は不要なら最小限または無しでよい
- namespace は package 名や repo 命名に合わせて整理
- ログ prefix は統一する
  - 例: `[ClipStartDelay]`
- コードは、後で batch 対応や C方式を足しやすいように、
  UI / path resolve / validation / clip processing を分けて整理すると嬉しい

---

## Suggested Internal Structure
例:

- `Editor/UI/`
- `Editor/Core/`
- `Editor/Utility/`
- `Editor/Diagnostics/`

想定クラス例:
- `AnimationClipStartDelayWindow`
- `AnimationClipStartDelayProcessor`
- `AnimationClipStartDelayPathResolver`
- `AnimationClipStartDelayValidator`
- `AnimationClipStartDelayLog`

名前は多少変えてよいが、責務は分けたい。

---

## README Requirements
README.md は日本語で、最低限以下を含める。

- タイトル
- 概要
- 何ができるか
- 現在対応していること
- 使い方
- Output Location の説明
- Dry Run / 診断
- 制限事項
- ライセンス

README 内では、
「重い MMD モーションを手でずらす代わりに、
非破壊で遅延版クリップを生成するツール」
という用途が伝わるようにする。

---

## Test Checklist
Codex 作業後、Unity 側で確認したいこと:

1. コンパイルエラーがない
2. EditorWindow が開く
3. Source Clip を入れられる
4. Seconds モードで Dry Run が動く
5. Frames モードで Dry Run が動く
6. Generate で新規 clip が作られる
7. 元 clip が変わっていない
8. 生成 clip が指定時間ぶん遅れて始まる
9. clip length が delay 分だけ伸びている
10. 同名ファイル時に安全に止まる
11. Custom で `Assets/` 外を選んだ時に安全に止まる
12. 重い clip でも手動編集より運用しやすい

---

## Done Criteria
- Unity でコンパイルエラーなし
- EditorWindow が開く
- Dry Run が動く
- Generate が動く
- 元 Clip が変化しない
- 生成 Clip が指定分遅れて再生される
- README に使い方と制限事項が書かれている

---

## Work Style Requests
- まず短い plan を出してから作業
- 既存ファイルは必要以上に壊さない
- 非破壊・安全第一
- 作業後に変更ファイル一覧と tree を出す
- まだ commit はしない
