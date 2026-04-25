# CLAUDE

> この文書は開発用のハンドオフ / 現況メモです。エンドユーザー向けの導入や使い方は `README.md` を参照してください。

## Goal

- `Animation Clip Start Delay` を package repo として維持し、重い `AnimationClip` を非破壊で遅延複製できる状態を保つ。
- handoff には初期仕様書ではなく、repo 内で今どこまで実装済みかと、次に触る時の判断材料だけを残す。

## Current State

- repo root が package の source of truth です。`package.json`、`Editor/`、`Runtime/`、`Documentation~/`、`Samples~/`、`README.md`、`CHANGELOG.md`、release workflow が root に揃っています。
- `package.json` は `0.1.3`、ローカル HEAD は `991bbad` (`main`) です。tag は `0.1.3` まであります。
- main UI は `Editor/UI/AnimationClipStartDelayWindow.cs` にあり、menu は `Tools/Sebanne/Animation Clip Start Delay` です。
- 実装済み機能は「単一 `AnimationClip`」「`Seconds` / `Frames` 指定」「`Dry Run`」「`Generate`」「`Generated / Same As Source / Custom` 出力先」です。
- Generate は元 clip を直接変更せず、新規 `AnimationClip` asset を生成します。float curve / object reference curve / `AnimationEvent` をまとめて後ろへずらします。
- 出力先解決と validation は `AnimationClipStartDelayPathResolver` / `AnimationClipStartDelayValidator` に分かれていて、`Custom` は `Assets/` 配下のみ許可、同名 asset がある場合は停止します。
- custom 出力先は `EditorPrefs` で保持しています。Runtime 側は asmdef だけの最小構成です。
- README / CHANGELOG / `BOOTH_PACKAGE` / release workflow は、現行の VCC listing 導線と package zip 配布を前提に揃っています。

## Current Direction

- repo 内に明確な blocker はありません。次に触る時は、機能拡張を進める回か、公開面 / 配布面の整備を進める回かを最初に切り分けると安全です。
- 機能拡張をやるなら、候補は複数 clip 一括処理、別 delay 方式、overwrite 方針、preview 系です。今の repo はそこまでは入っていません。
- 既存の責務分離は悪くないので、次回も `Window / Validator / PathResolver / Processor / Log` を崩さずに増やす方針でよいです。

## Current Blocker

- hard blocker はありません。
- 未実装のまま残している論点は、複数 clip 一括処理、clip 長維持モード、初期値 hold モード、preview、既存ファイル overwrite 対応です。

## Rules

- 非破壊
- 元 clip を直接書き換えない
- 新規 asset 生成のみ
- まず `Dry Run`
- `Custom` 出力先は `Assets/` 配下限定
- 既存同名 asset は上書きしない
- まず短い plan を出してから作業
- まだ commit / push はしない

## Key Files

- `Editor/UI/AnimationClipStartDelayWindow.cs`
- `Editor/Core/AnimationClipStartDelayProcessor.cs`
- `Editor/Core/AnimationClipStartDelayPathResolver.cs`
- `Editor/Core/AnimationClipStartDelayValidator.cs`
- `Editor/Core/AnimationClipStartDelayModels.cs`
- `Editor/Utility/AnimationClipStartDelayEditorPrefs.cs`
- `Editor/Diagnostics/AnimationClipStartDelayLog.cs`
- `README.md`
- `.github/workflows/release.yml`

## Resume Notes

- package: `com.sebanne.animation-clip-start-delay`
- version: `0.1.3`
- latest tag: `0.1.3`
- HEAD: `991bbad` (`main`)
- release asset 名: `com.sebanne.animation-clip-start-delay-0.1.3.zip`
- 既定出力先: `Assets/Sebanne/AnimationClipStartDelay/Generated/`
