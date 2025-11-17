using UnityEditor;


// Build Settings 최적화
public class BuildOptimizer
{
    [MenuItem("AppsInToss/Optimize Build Settings")]
    static void OptimizeBuildSettings()
    {
        // IL2CPP 설정 최적화
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP);
    
        // 코드 최적화 레벨
        PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.WebGL, Il2CppCompilerConfiguration.Release);
    
        // 불필요한 코드 제거
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.High);
    
        // 압축 설정
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.decompressionFallback = true;
    }
}
