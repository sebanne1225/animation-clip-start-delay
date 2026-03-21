# Changelog

このファイルは `Sebanne Animation Clip Start Delay` の変更履歴を管理します。

## [0.1.3] - 2026-03-21

### Changed

- README の VCC / VPM 導入手順を `source.json` ベースの案内に整理
- BOOTH 同梱テキストの案内文を公開向けに調整
- release asset や公開導線の文言を現在の運用に合わせて更新

## [0.1.2] - 2026-03-18

### Changed

- 生成時の既定出力先を `Assets/Sebanne/AnimationClipStartDelay/Generated/` に変更し、`Sebanne` と `Generated` の間にツール名を挟んだフォルダ構成へ整理

## [0.1.0] - 2026-03-18

### Added

- `AnimationClip` の開始を遅らせた複製 clip を生成する Unity EditorWindow を追加
- Seconds / Frames 両対応の Delay 指定、Dry Run、Generate を追加
- float curve / object reference curve / AnimationEvent をまとめて後ろへずらす処理を追加
- Generated / Same As Source / Custom の出力先解決と安全な validation を追加
