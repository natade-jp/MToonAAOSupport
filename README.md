# MToon AAO Support

旧VRM 0.x用の`VRM/MToon`シェーダーを、AAO Merge Materialで使用するためのShader Informationを提供するUnity Editor拡張です。

This package adds AAO Merge Material support for the legacy VRM 0.x `VRM/MToon` shader.

## 機能

AAOへ、旧`VRM/MToon`が使用するテクスチャとUVの関係を登録します。

対応するテクスチャは次のとおりです。

- Lit Texture
- Shade Texture
- Normal Map
- Receive Shadow Texture
- Shading Grade Texture
- Rim Texture
- Emission Map
- Outline Width Texture
- UV Animation Mask
- MatCap

このパッケージを導入するとShader Informationが自動的に登録されます。アバターへ専用コンポーネントを追加する必要はありません。

## 必要なもの

- Unity 2022.3
- 旧VRM 0.x用の`VRM/MToon`シェーダー
- [AAO: Avatar Optimizer](https://vpm.anatawa12.com/avatar-optimizer/) 1.9.17以降

AAO本体とMToon本体は、このパッケージに含まれていません。

## インストール

### Git URLから導入

先にAAOを公式VPMリポジトリからインストールしてください。

UnityのPackage Managerを開き、`Add package from git URL`から次のURLを指定します。

```text
https://github.com/natade-jp/MToonAAOSupport.git
```

### Assetsへ直接導入

リポジトリの`Editor`フォルダーを、Unityプロジェクトの次の場所へコピーします。

```text
Assets/MToonAAOSupport/Editor
```

## 使用方法

1. 統合対象のRendererへ`AAO Merge Material`を追加します
2. 同じ統合グループへ`VRM/MToon`マテリアルを登録します
3. アトラス上の配置を設定します
4. Playモードまたは`Manual bake avatar`で処理結果を確認します
5. テクスチャ、陰影、発光、MatCap、アウトラインを確認します

通常の編集状態ではAAOの処理が適用されないため、VRCSDK Control Panel上のマテリアル数へ反映されない場合があります。

## 対応範囲

このパッケージが対象にするシェーダーは次のとおりです。

```text
VRM/MToon
```

次のシェーダーには対応していません。

```text
VRM10/MToon10
```

## 制限事項

- VRM 1.0用の`VRM10/MToon10`には対応していません
- UVスクロールやUV回転を使用するマテリアルは十分に検証されていません
- 異なる描画モードやRender Queueのマテリアルは統合しないでください
- マテリアル差し替えアニメーションを使用する構成は対象外です
- AAOまたはUniVRMの更新によって動作しなくなる可能性があります
- 統合後の見た目をPlayモードまたはアップロード後に確認してください

## 動作確認

次の項目を確認しています。

- AAO Merge Materialの未対応シェーダーエラーが解消されること
- Playモードおよびビルド時に処理が行われること
- VRChatへアップロードしたアバターのマテリアルスロット数が減ること

すべてのMToon設定およびUniVRMバージョンでの動作を保証するものではありません。

## ライセンス

このパッケージは[MIT License](LICENSE.md)で公開しています。

このパッケージは、AAO: Avatar OptimizerおよびMToonの公式パッケージではありません。
