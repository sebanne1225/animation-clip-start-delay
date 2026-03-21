# Publishing Checklist

## Scope

- Tool: `Animation Clip Start Delay`
- Package: `com.sebanne.animation-clip-start-delay`
- Menu: `Tools/Sebanne/Animation Clip Start Delay`
- GitHub repo: `https://github.com/sebanne1225/animation-clip-start-delay`
- VPM source: `https://sebanne1225.github.io/sebanne-listing/source.json`

---

## GitHub

### Ready

- `README.md`, `package.json`, `CHANGELOG.md`, `LICENSE` は存在
- package 構成は Unity package 形式で揃っている
- GitHub remote は `sebanne1225/animation-clip-start-delay`

### Missing / Follow-up

- GitHub Release 用の文面テンプレが未作成
- README / release 用のスクリーンショット素材が未作成

---

## VPM

### Ready

- package 名と version は設定済み
- repo root に package 配布対象ファイルが揃っている

### Missing / Follow-up

- release tag と `package.json.version` を一致させる最終確認が必要
- listing 側で package の見え方を実 project で確認する

---

## BOOTH

### Ready

- ツール概要と使い方の元文面は README にある
- MIT License を同梱可能
- repo 内の `BOOTH_PACKAGE/` は作業用の固定名で運用する
- BOOTH に貼る最終フォルダ名 / zip 名は `package.json.version` ベースで `AnimationClipStartDelay_Booth_Package_v0.1.3` を推奨する

### Missing / Follow-up

- BOOTH 配布 zip 用の案内テキスト一式が未作成
- サムネイル画像が未作成
- BOOTH 商品説明文の短文 / 長文テンプレが未作成
- Dry Run を先に行う運用を画像付きで示す素材が未作成

---

## Immediate

- GitHub Release / BOOTH 用のサムネまたはスクリーンショットを最低 1 枚用意する

## Later

- release note テンプレを作る
- BOOTH 同梱テキストを整える
- `package.json` に `documentationUrl` と `changelogUrl` を追加するか検討する
- `Samples~` を残すなら公開用サンプルを入れる

## Preflight

- Unity で Dry Run / Generate を再確認する
- 同名ファイル停止、Custom 出力先制限、object reference curve、`AnimationEvent` のズレなしを確認する
- GitHub Release tag と `package.json.version` を一致させる
- release asset zip の中身を確認する
- VCC で listing 追加後に package が見えることを確認する
- BOOTH 商品説明と README の用語差分を最終確認する
