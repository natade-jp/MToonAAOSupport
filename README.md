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

## トラブルシューティング

### Mip Streamingの警告が表示される

NDMF Consoleのテストビルドなどで、次の警告が表示される場合があります。

```text
他のツールに生成されたと思われるテクスチャアセットに、ミップストリーミングがオフになっています
```

対象として、次のようなAAOの生成テクスチャが表示されます。

```text
AAO Merged Texture (for _MainTex)
AAO Merged Texture (for _ShadeTexture)
AAO Merged Texture (for _BumpMap)
AAO Merged Texture (for _EmissionMap)
```

#### 原因

統合元のテクスチャで`Generate Mipmaps`または`Mip Streaming`が無効になっていると、AAOが生成した統合テクスチャでもMip Streamingが無効になり、NDMFのVRChat向け検査で警告されることがあります。

処理の流れは次のようになります。

```text
元のMToonテクスチャでMip Streamingが無効
    ↓
AAO Merge Materialが統合テクスチャを生成
    ↓
生成されたテクスチャでもMip Streamingが無効
    ↓
NDMF Consoleに警告が表示される
```

MToon AAO Supportは、MToonが使用するテクスチャとUVの関係をAAOへ登録するためのものです。テクスチャの生成やImport Settingsの変更は行いません。そのため、この警告はMToon AAO SupportがMip Streamingを無効にしたことを示すものではありません。

#### 対応方法

統合対象のMToonマテリアルで使用している元テクスチャをProjectウィンドウから選択します。マテリアルではなく、PNGなどから読み込まれた`Texture 2D`を選択してください。

Inspectorの`Advanced`を展開し、次の項目を有効にします。

```text
Generate Mipmaps：オン
Mip Streaming：オン
```

Unityのバージョンによっては、`Mip Streaming`が`Streaming Mip Maps`と表示されることがあります。

変更後はInspector右下の`Apply`を押します。次のテクスチャを使用している場合は、それぞれの元画像を確認してください。

- Lit Texture
- Shade Texture
- Normal Map
- Emission Map
- Rim Texture
- Outline Width Texture
- 統合相手のマテリアルが使用している各テクスチャ

設定後、もう一度NDMF Consoleからテストビルドを実行します。

#### 警告が解消しない場合

次の点を確認してください。

- 統合対象となるすべてのマテリアルの元テクスチャを設定したか
- `Apply`を押してTexture Import Settingsを保存したか
- Asset PostprocessorによってImport Settingsが元に戻されていないか
- AAOを対応する安定版へ更新しているか

`AAO Merged Texture`はビルド時に生成される一時的なテクスチャです。生成テクスチャを直接変更しても次のビルドで作り直されるため、元テクスチャ側を修正してください。

すべての元テクスチャでMip Streamingを有効にしても警告が残る場合は、AAO Merge Materialの生成処理で設定が引き継がれていない可能性があります。この場合は、Unity、AAO、NDMF、UniVRMのバージョンと警告内容を添えてAAO側へ報告してください。

### マテリアル数が減ったか確認できない

AAOは非破壊ツールのため、通常の編集状態では元のマテリアルスロットを変更しません。処理はPlayモードへ入るとき、`Manual bake avatar`を実行するとき、またはアバターをビルドするときに行われます。

VRCSDK Control Panelの事前表示では、AAO適用前のマテリアル数が表示される場合があります。NDMF Console、Manual bake後のアバター、またはアップロード後のVRChat上で確認してください。

### テクスチャメモリが増加する

マテリアルを統合すると、複数の画像から新しいアトラステクスチャが生成されます。アトラスのサイズや配置によっては、マテリアルスロットが減ってもテクスチャメモリが増加する場合があります。

例えば、Main Textureだけでなく、Shade Texture、Normal Map、Emission Mapなどにも個別のアトラスが生成されます。

この場合は、AAO Merge Materialの生成テクスチャサイズを確認してください。サイズを小さくするとメモリ使用量を削減できますが、画質が低下する可能性があります。マテリアルスロット数だけでなく、テクスチャメモリと統合後の画質も含めて判断してください。

## 動作確認

次の項目を確認しています。

- AAO Merge Materialの未対応シェーダーエラーが解消されること
- Playモードおよびビルド時に処理が行われること
- VRChatへアップロードしたアバターのマテリアルスロット数が減ること
- 元テクスチャのMip Streamingを有効にすると、NDMF Consoleの生成テクスチャ警告が解消されること

すべてのMToon設定およびUniVRMバージョンでの動作を保証するものではありません。

## 参考資料

- [AAO Shader Information API](https://vpm.anatawa12.com/avatar-optimizer/ja/docs/developers/shader-information/)
- [AAO Merge Material](https://vpm.anatawa12.com/avatar-optimizer/ja/docs/reference/merge-material/)
- [MToon](https://github.com/Santarh/MToon)
- [Unity Mipmap Streaming system](https://docs.unity3d.com/2022.1/Documentation/Manual/TextureStreaming.html)

## ライセンス

このパッケージは[MIT License](LICENSE.md)で公開しています。

このパッケージは、AAO: Avatar OptimizerおよびMToonの公式パッケージではありません。
