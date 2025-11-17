using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class AppsInTossAudioOptimizer : EditorWindow
{
    [System.Serializable]
    public class AudioOptimizationRule
    {
        public string folderPath;
        public AudioImporterSampleSettings mobileSettings;
        public AudioImporterSampleSettings webGLSettings;
        public bool force3D;
        public bool enableCompression;
        public float compressionQuality;
    }
    
    public List<AudioOptimizationRule> audioRules = new List<AudioOptimizationRule>();
    
    [MenuItem("AppsInToss/Audio Optimizer")]
    public static void ShowWindow()
    {
        GetWindow<AppsInTossAudioOptimizer>("Audio Optimizer");
    }
    
    void OnEnable()
    {
        InitializeAudioRules();
    }
    
    void InitializeAudioRules()
    {
        audioRules.Clear();
        
        // 음악 파일 규칙
        audioRules.Add(new AudioOptimizationRule
        {
            folderPath = "Audio/Music",
            mobileSettings = new AudioImporterSampleSettings
            {
                loadType = AudioClipLoadType.Streaming,
                compressionFormat = AudioCompressionFormat.Vorbis,
                quality = 0.7f,
                sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate
            },
            webGLSettings = new AudioImporterSampleSettings
            {
                loadType = AudioClipLoadType.Streaming,
                compressionFormat = AudioCompressionFormat.Vorbis,
                quality = 0.5f,
                sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate
            },
            force3D = false,
            enableCompression = true,
            compressionQuality = 70f
        });
        
        // 효과음 규칙
        audioRules.Add(new AudioOptimizationRule
        {
            folderPath = "Audio/SFX",
            mobileSettings = new AudioImporterSampleSettings
            {
                loadType = AudioClipLoadType.DecompressOnLoad,
                compressionFormat = AudioCompressionFormat.ADPCM,
                quality = 1.0f,
                sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate
            },
            webGLSettings = new AudioImporterSampleSettings
            {
                loadType = AudioClipLoadType.CompressedInMemory,
                compressionFormat = AudioCompressionFormat.Vorbis,
                quality = 0.8f,
                sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate
            },
            force3D = false,
            enableCompression = true,
            compressionQuality = 80f
        });
        
        // 앱인토스 특화 오디오 (토스 사운드)
        audioRules.Add(new AudioOptimizationRule
        {
            folderPath = "Audio/Toss",
            mobileSettings = new AudioImporterSampleSettings
            {
                loadType = AudioClipLoadType.DecompressOnLoad,
                compressionFormat = AudioCompressionFormat.PCM,
                quality = 1.0f,
                sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate
            },
            webGLSettings = new AudioImporterSampleSettings
            {
                loadType = AudioClipLoadType.DecompressOnLoad,
                compressionFormat = AudioCompressionFormat.PCM,
                quality = 1.0f,
                sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate
            },
            force3D = false,
            enableCompression = false, // 토스 브랜드 오디오는 고품질 유지
            compressionQuality = 100f
        });
    }
    
    void OnGUI()
    {
        GUILayout.Label("앱인토스 오디오 최적화", EditorStyles.boldLabel);
        
        if (GUILayout.Button("모든 오디오 최적화"))
        {
            OptimizeAllAudio();
        }
        
        foreach (var rule in audioRules)
        {
            DrawAudioRule(rule);
        }
        
        DrawAudioStatistics();
    }
    
    void OptimizeAllAudio()
    {
        foreach (var rule in audioRules)
        {
            OptimizeAudioInFolder(rule);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("오디오 최적화 완료");
    }
    
    void OptimizeAudioInFolder(AudioOptimizationRule rule)
    {
        string folderPath = Path.Combine("Assets", rule.folderPath);
        string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { folderPath });
        
        foreach (string guid in audioGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            OptimizeAudioClip(assetPath, rule);
        }
    }
    
    void OptimizeAudioClip(string assetPath, AudioOptimizationRule rule)
    {
        AudioImporter importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
        if (importer == null) return;
        
        // 모바일 플랫폼 설정
        importer.SetOverrideSampleSettings("Android", rule.mobileSettings);
        importer.SetOverrideSampleSettings("iOS", rule.mobileSettings);
        
        // WebGL 설정
        importer.SetOverrideSampleSettings("WebGL", rule.webGLSettings);
        
        // 3D 설정
        if (rule.force3D)
        {
            importer.threeD = true;
        }
        
        AssetDatabase.ImportAsset(assetPath);
    }
    
    void DrawAudioRule(AudioOptimizationRule rule)
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label($"폴더: {rule.folderPath}");
        GUILayout.Label($"모바일 형식: {rule.mobileSettings.compressionFormat}");
        GUILayout.Label($"WebGL 형식: {rule.webGLSettings.compressionFormat}");
        
        if (GUILayout.Button($"{rule.folderPath} 최적화"))
        {
            OptimizeAudioInFolder(rule);
        }
        
        GUILayout.EndVertical();
    }
    
    void DrawAudioStatistics()
    {
        var allAudio = AssetDatabase.FindAssets("t:AudioClip");
        long totalSize = 0;
        int uncompressedCount = 0;
        
        foreach (string guid in allAudio)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            
            if (clip != null)
            {
                totalSize += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(clip);
                
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer?.defaultSampleSettings.compressionFormat == AudioCompressionFormat.PCM)
                {
                    uncompressedCount++;
                }
            }
        }
        
        GUILayout.Label("오디오 통계", EditorStyles.boldLabel);
        GUILayout.Label($"전체 오디오 클립: {allAudio.Length}개");
        GUILayout.Label($"총 메모리 사용량: {totalSize / (1024 * 1024)}MB");
        GUILayout.Label($"압축되지 않은 클립: {uncompressedCount}개");
    }
}
#endif