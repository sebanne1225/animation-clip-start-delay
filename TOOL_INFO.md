# TOOL_INFO

## ツール名

- AnimationClipStartDelay

## package名

- `com.sebanne.animation-clip-start-delay`

## 表示名

- `Sebanne Animation Clip Start Delay`

## 想定用途

- 重い `AnimationClip` を Animation Window で手動編集せず、非破壊で開始遅延版 clip を生成する。
- MMD 由来など、キーが非常に多い clip を安全に複製運用できるようにする。

## 現在対応していること

- 単一 clip 対応
- Seconds / Frames 両対応
- Dry Run と Generate
- Generated / Same As Source / Custom の出力先解決

## 非対応

- 複数 clip 一括処理
- B 方式、C 方式
- 再生プレビュー
- AnimatorController 側の自動補正
- FBX 内蔵 clip の直接編集

## 今後やりたいこと

- batch 対応
- 遅延方式の追加
- 上書き確認やより詳細な出力オプション
- 生成結果の差分確認やプレビュー補助
