// 텍스처 압축 자동화

using UnityEditor;
#if UNITY_WEBGL
public class TextureOptimizer : AssetPostprocessor
{
    // void OnPreprocessTexture()
    // {
    //     var importer = assetImporter as TextureImporter;
    //     
    //     // WebGL 플랫폼 설정
    //     var platformSettings = new TextureImporterPlatformSettings
    //     {
    //         name = "WebGL",
    //         overridden = true,
    //         maxTextureSize = 1024, // 모바일 고려
    //         format = TextureImporterFormat.DXT5,
    //         compressionQuality = 80, // 품질 vs 크기 균형
    //         allowsAlphaSplitting = true
    //     };
    //     
    //     importer.SetPlatformTextureSettings(platformSettings);
    // }
}
#endif