#if UNITY_EDITOR

// AAOのShader Information API
using Anatawa12.AvatarOptimizer.API;
using UnityEditor;
using UnityEngine;

namespace MToonAAOSupport
{
    /// <summary>旧VRM 0.x用VRM/MToonのAAO向けシェーダー情報</summary>
    /// <remarks>
    /// VRM 1.0用のVRM10/MToon10には対応しない
    /// MToonが使用するテクスチャとUVの関係をAAOへ提供する
    /// </remarks>
    [InitializeOnLoad]
    internal sealed class MToonShaderInformation : ShaderInformation
    {
        /// <summary>メッシュのUV0を使用するテクスチャプロパティ一覧</summary>
        /// <remarks>
        /// 旧MToonでは各テクスチャが_MainTex_STによる共通のTilingとOffsetを使用する
        /// 法線マップはシェーダーキーワードによって使用状態が変わるため別途登録する
        /// MatCapはメッシュのUVを使用しないため別途登録する
        /// </remarks>
        private static readonly string[] Uv0TexturePropertyNames =
        {
            "_MainTex",              // 基本色と透明度
            "_ShadeTexture",         // 影部分の色
            "_ReceiveShadowTexture", // 外部の影を受ける強さ
            "_ShadingGradeTexture",  // 明部と暗部の切り替わり方
            "_RimTexture",           // 輪郭付近のリムライト
            "_EmissionMap",          // 自発光する部分
            "_OutlineWidthTexture",  // アウトラインの太さ
            "_UvAnimMaskTexture"     // UVアニメーションの適用範囲
        };

        /// <summary>Unityエディター初期化後のシェーダー情報登録予約</summary>
        static MToonShaderInformation()
        {
            // アセンブリ読み込み中の即時実行を避け、次のEditor更新時に登録する
            EditorApplication.delayCall += Register;
        }

        /// <summary>AAOへ提供するシェーダー情報の種類</summary>
        public override ShaderInformationKind SupportedInformationKind =>
            // テクスチャ、UVチャンネル、UV変換、サンプラー情報を提供する
            ShaderInformationKind.TextureAndUVUsage;

        /// <summary>VRM/MToonのテクスチャ使用情報の登録</summary>
        /// <param name="materialInformation">対象マテリアルの情報取得と登録に使用するコールバック</param>
        public override void GetMaterialInformation(
            MaterialInformationCallback materialInformation)
        {
            // MToonの各UV0テクスチャが共通して使用するTilingとOffset
            // nullの場合はUV変換が動的または不明であることを表す
            Matrix2x3? uvMatrix = CreateUvMatrix(materialInformation);

            // UV0を使用する通常のテクスチャをAAOへ登録する
            foreach (string propertyName in Uv0TexturePropertyNames)
            {
                RegisterUv0Texture(
                    materialInformation,
                    propertyName,
                    uvMatrix
                );
            }

            // 法線マップは_NORMALMAPが有効な場合だけシェーダーから参照される
            // 戻り値がnullの場合も使用される可能性があるためfalse以外で登録する
            if (materialInformation.IsShaderKeywordEnabled("_NORMALMAP") != false)
            {
                RegisterUv0Texture(
                    materialInformation,
                    "_BumpMap",
                    uvMatrix
                );
            }

            // MatCapはカメラ方向と面の法線から座標を計算する
            // メッシュのUV0やUV1を使用しないためNonMeshとして登録する
            materialInformation.RegisterTextureUVUsage(
                "_SphereAdd",
                "_SphereAdd",
                UsingUVChannels.NonMesh,
                null
            );
        }

        /// <summary>VRM/MToonシェーダーへの対応情報登録</summary>
        private static void Register()
        {
            // UniVRMが提供する旧VRM 0.x用MToonシェーダーを名前で検索する
            Shader shader = Shader.Find("VRM/MToon");

            // UniVRMが未導入の場合などは登録せず警告のみ表示する
            if (shader == null)
            {
                Debug.LogWarning(
                    "MToonAAOSupport: VRM/MToonシェーダーが見つかりません"
                );
                return;
            }

            // VRM/MToonと、このShaderInformation実装をAAOへ関連付ける
            ShaderInformationRegistry.RegisterShaderInformation(
                shader,
                new MToonShaderInformation()
            );
        }

        /// <summary>MToon共通UV変換行列の生成</summary>
        /// <param name="materialInformation">対象マテリアルの情報取得用コールバック</param>
        /// <returns>固定されたUV変換行列または動的・不明を表すnull</returns>
        private static Matrix2x3? CreateUvMatrix(
            MaterialInformationCallback materialInformation)
        {
            // UVスクロールや回転は時間によって変化し、固定行列では表現できない
            // 値が不明な場合も安全側に倒して動的なUV変換として扱う
            if (HasUvAnimation(materialInformation))
            {
                return null;
            }

            // Unityの_MainTex_STは次の順序でTilingとOffsetを保持する
            // (Tiling X, Tiling Y, Offset X, Offset Y)
            Vector4? textureTransform =
                materialInformation.GetVector("_MainTex_ST");

            // 取得できたVector4をAAO用の2行3列のUV変換行列へ変換する
            // プロパティが不明な場合は固定変換を確定できないためnullを返す
            return textureTransform is { } value
                ? Matrix2x3.NewScaleOffset(value)
                : null;
        }

        /// <summary>UVアニメーションが使用中または不明かの判定</summary>
        /// <param name="materialInformation">対象マテリアルの情報取得用コールバック</param>
        /// <returns>使用中または不明の場合はtrue</returns>
        private static bool HasUvAnimation(
            MaterialInformationCallback materialInformation)
        {
            // MToonが持つUVスクロール速度と回転速度を取得する
            float? scrollX =
                materialInformation.GetFloat("_UvAnimScrollX");
            float? scrollY =
                materialInformation.GetFloat("_UvAnimScrollY");
            float? rotation =
                materialInformation.GetFloat("_UvAnimRotation");

            // 3つすべてが取得でき、かつ0の場合だけUVアニメーションなしと判断する
            // nullはアニメーションなどによって値を確定できない可能性を表す
            return scrollX is null
                || scrollY is null
                || rotation is null
                || scrollX != 0
                || scrollY != 0
                || rotation != 0;
        }

        /// <summary>UV0依存テクスチャの使用情報登録</summary>
        /// <param name="materialInformation">対象マテリアルの情報登録用コールバック</param>
        /// <param name="propertyName">テクスチャとサンプラーのプロパティ名</param>
        /// <param name="uvMatrix">固定されたUV変換行列または動的・不明を表すnull</param>
        private static void RegisterUv0Texture(
            MaterialInformationCallback materialInformation,
            string propertyName,
            Matrix2x3? uvMatrix)
        {
            materialInformation.RegisterTextureUVUsage(
                // テクスチャが設定されているマテリアルプロパティ
                propertyName,

                // テクスチャのラッピングとフィルタリングに使うサンプラー
                propertyName,

                // 旧MToonの対象テクスチャはメッシュの1組目のUVを使用する
                UsingUVChannels.UV0,

                // _MainTex_STから生成したTilingとOffset
                uvMatrix
            );
        }
    }
}

#endif