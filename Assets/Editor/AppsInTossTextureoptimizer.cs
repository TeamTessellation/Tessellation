#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class AppsInTossTextureOptimizer : EditorWindow
{
    [System.Serializable]
    public class TextureOptimizationRule
    {
        public string folderPath;
        public TextureImporterType textureType;
        public int maxSize;
        public TextureImporterFormat androidFormat;
        public TextureImporterFormat iOSFormat;
        public TextureImporterFormat webGLFormat;
        public bool generateMipMaps;
        public FilterMode filterMode;
        public TextureWrapMode wrapMode;
        public int compressionQuality;
    }

    [Header("앱인토스 최적화 규칙")]
    public List<TextureOptimizationRule> optimizationRules = new List<TextureOptimizationRule>();

    [MenuItem("AppsInToss/Texture Optimizer")]
    public static void ShowWindow()
    {
        GetWindow<AppsInTossTextureOptimizer>("Texture Optimizer");
    }

    void OnEnable()
    {
        InitializeDefaultRules();
    }

    void InitializeDefaultRules()
    {
        optimizationRules.Clear();

        // UI 텍스처 규칙
        optimizationRules.Add(new TextureOptimizationRule
        {
            folderPath = "UI",
            textureType = TextureImporterType.Sprite,
            maxSize = 1024,
            androidFormat = TextureImporterFormat.ETC2_RGBA8,
            iOSFormat = TextureImporterFormat.ASTC_6x6,
            webGLFormat = TextureImporterFormat.DXT5,
            generateMipMaps = false,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            compressionQuality = 50
        });

        // 환경 텍스처 규칙
        optimizationRules.Add(new TextureOptimizationRule
        {
            folderPath = "Environment",
            textureType = TextureImporterType.Default,
            maxSize = 512,
            androidFormat = TextureImporterFormat.ETC2_RGB4,
            iOSFormat = TextureImporterFormat.ASTC_6x6,
            webGLFormat = TextureImporterFormat.DXT1,
            generateMipMaps = true,
            filterMode = FilterMode.Trilinear,
            wrapMode = TextureWrapMode.Repeat,
            compressionQuality = 50
        });

        // 캐릭터 텍스처 규칙
        optimizationRules.Add(new TextureOptimizationRule
        {
            folderPath = "Characters",
            textureType = TextureImporterType.Default,
            maxSize = 1024,
            androidFormat = TextureImporterFormat.ETC2_RGBA8,
            iOSFormat = TextureImporterFormat.ASTC_4x4,
            webGLFormat = TextureImporterFormat.DXT5,
            generateMipMaps = true,
            filterMode = FilterMode.Trilinear,
            wrapMode = TextureWrapMode.Clamp,
            compressionQuality = 75
        });

        // 앱인토스 브랜딩 텍스처 규칙 (고품질 유지)
        optimizationRules.Add(new TextureOptimizationRule
        {
            folderPath = "AppsInToss",
            textureType = TextureImporterType.Sprite,
            maxSize = 2048,
            androidFormat = TextureImporterFormat.RGBA32,
            iOSFormat = TextureImporterFormat.RGBA32,
            webGLFormat = TextureImporterFormat.RGBA32,
            generateMipMaps = false,
            filterMode = FilterMode.Trilinear,
            wrapMode = TextureWrapMode.Clamp,
            compressionQuality = 100
        });
    }

    void OnGUI()
    {
        GUILayout.Label("앱인토스 텍스처 최적화", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 전체 최적화 버튼
        if (GUILayout.Button("모든 텍스처 최적화", GUILayout.Height(30)))
        {
            OptimizeAllTextures();
        }

        GUILayout.Space(10);

        // 규칙별 최적화
        foreach (var rule in optimizationRules)
        {
            DrawOptimizationRule(rule);
        }

        GUILayout.Space(10);

        // 통계 표시
        DrawTextureStatistics();
    }

    void DrawOptimizationRule(TextureOptimizationRule rule)
    {
        GUILayout.BeginVertical("box");

        GUILayout.Label($"폴더: {rule.folderPath}", EditorStyles.boldLabel);
        GUILayout.Label($"최대 크기: {rule.maxSize}px");
        GUILayout.Label($"Android: {rule.androidFormat}");
        GUILayout.Label($"iOS: {rule.iOSFormat}");
        GUILayout.Label($"WebGL: {rule.webGLFormat}");

        if (GUILayout.Button($"{rule.folderPath} 폴더 최적화"))
        {
            OptimizeTexturesInFolder(rule);
        }

        GUILayout.EndVertical();
        GUILayout.Space(5);
    }

    void OptimizeAllTextures()
    {
        EditorUtility.DisplayProgressBar("텍스처 최적화", "모든 텍스처를 최적화하는 중...", 0);

        try
        {
            foreach (var rule in optimizationRules)
            {
                OptimizeTexturesInFolder(rule);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("앱인토스 텍스처 최적화 완료");
            ShowNotification(new GUIContent("최적화 완료!"));
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    void OptimizeTexturesInFolder(TextureOptimizationRule rule)
    {
        string folderPath = Path.Combine("Assets", rule.folderPath);

        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning($"폴더를 찾을 수 없습니다: {folderPath}");
            return;
        }

        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
        int optimizedCount = 0;

        for (int i = 0; i < textureGuids.Length; i++)
        {
            string guid = textureGuids[i];
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            EditorUtility.DisplayProgressBar(
                $"{rule.folderPath} 텍스처 최적화",
                $"{Path.GetFileName(assetPath)}",
                (float)i / textureGuids.Length
            );

            if (OptimizeTexture(assetPath, rule))
            {
                optimizedCount++;
            }
        }

        Debug.Log($"{rule.folderPath} 폴더 최적화 완료: {optimizedCount}/{textureGuids.Length}개 텍스처");
    }

    bool OptimizeTexture(string assetPath, TextureOptimizationRule rule)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return false;

        bool wasChanged = false;

        // 기본 설정
        if (importer.textureType != rule.textureType)
        {
            importer.textureType = rule.textureType;
            wasChanged = true;
        }

        if (importer.mipmapEnabled != rule.generateMipMaps)
        {
            importer.mipmapEnabled = rule.generateMipMaps;
            wasChanged = true;
        }

        if (importer.filterMode != rule.filterMode)
        {
            importer.filterMode = rule.filterMode;
            wasChanged = true;
        }

        if (importer.wrapMode != rule.wrapMode)
        {
            importer.wrapMode = rule.wrapMode;
            wasChanged = true;
        }

        // 플랫폼별 설정
        wasChanged |= SetPlatformSettings(importer, "Android", rule.maxSize, rule.androidFormat, rule.compressionQuality);
        wasChanged |= SetPlatformSettings(importer, "iPhone", rule.maxSize, rule.iOSFormat, rule.compressionQuality);
        wasChanged |= SetPlatformSettings(importer, "WebGL", rule.maxSize, rule.webGLFormat, rule.compressionQuality);

        // 앱인토스 특화 설정
        if (assetPath.Contains("AppsInToss") || assetPath.Contains("Toss"))
        {
            importer.userData = "AppsInToss_Asset";
            wasChanged = true;
        }

        if (wasChanged)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            return true;
        }

        return false;
    }

    bool SetPlatformSettings(TextureImporter importer, string platform, int maxSize,
                            TextureImporterFormat format, int quality)
    {
        var platformSettings = importer.GetPlatformTextureSettings(platform);
        bool changed = false;

        if (platformSettings.overridden != true)
        {
            platformSettings.overridden = true;
            changed = true;
        }

        if (platformSettings.maxTextureSize != maxSize)
        {
            platformSettings.maxTextureSize = maxSize;
            changed = true;
        }

        if (platformSettings.format != format)
        {
            platformSettings.format = format;
            changed = true;
        }

        if (platformSettings.compressionQuality != quality)
        {
            platformSettings.compressionQuality = quality;
            changed = true;
        }

        if (changed)
        {
            importer.SetPlatformTextureSettings(platformSettings);
        }

        return changed;
    }

    void DrawTextureStatistics()
    {
        GUILayout.Label("텍스처 통계", EditorStyles.boldLabel);

        var allTextures = AssetDatabase.FindAssets("t:Texture2D");
        long totalSize = 0;
        int uncompressedCount = 0;
        int oversizedCount = 0;

        foreach (string guid in allTextures)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            if (texture != null)
            {
                totalSize += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texture);

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    if (importer.textureCompression == TextureImporterCompression.Uncompressed)
                    {
                        uncompressedCount++;
                    }

                    if (texture.width > 1024 || texture.height > 1024)
                    {
                        oversizedCount++;
                    }
                }
            }
        }

        GUILayout.Label($"전체 텍스처: {allTextures.Length}개");
        GUILayout.Label($"총 메모리 사용량: {totalSize / (1024 * 1024)}MB");
        GUILayout.Label($"압축되지 않은 텍스처: {uncompressedCount}개", uncompressedCount > 0 ? EditorStyles.boldLabel : EditorStyles.label);
        GUILayout.Label($"대형 텍스처 (1024px 초과): {oversizedCount}개", oversizedCount > 0 ? EditorStyles.boldLabel : EditorStyles.label);

        if (uncompressedCount > 0 || oversizedCount > 0)
        {
            EditorGUILayout.HelpBox("일부 텍스처가 최적화되지 않았습니다. 전체 최적화를 실행하세요.", MessageType.Warning);
        }
    }
}
#endif