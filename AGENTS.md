# AGENTS.md

この repo は `Sebanne Animation Clip Start Delay` の Unity package です。作業時は以下を守ってください。

## 開発ルール

- 元 `AnimationClip` は直接変更せず、必ず新規 asset 生成で扱う。
- Dry Run を先に通し、対象件数、出力先、結果長を確認できる導線を維持する。
- ログは `[ClipStartDelay]` prefix で統一し、失敗理由と回避しやすさを優先する。
- 同名ファイルがある場合は初期状態では上書きせず、安全に停止する。
- `AnimationEvent` と object reference curve を含め、時間情報のズレを残さない。
- Custom 出力先は `Assets/` 配下のみ許可し、asset path として安全に解決する。
- Editor 拡張の責務を `UI / Core / Utility / Diagnostics` に分け、後から機能追加しやすく保つ。
