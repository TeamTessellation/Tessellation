using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Database;
using SceneManagement;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;
using Action = System.Action;

namespace Core
{
    public class InitialLoader : MonoBehaviour
    {
        private static bool _initialized = false;
        private static Action _initializationCallback;
        public static bool Initialized => _initialized;
        
        
        [SerializeField,Tooltip("함께 로드할 씬 목록입니다.")] private SceneReference[] scenesToLoad;
        [SerializeField] private SceneReference nextScene;
        
 
        [Header("UI References")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text progressText;
        
        private float _progress;
        
        public float Progress
        {
            get => _progress;
            private set
            {
                _progress = Mathf.Clamp01(value);
                Debug.Log($"Loading Progress: {_progress * 100f}%");
                if (progressBar != null)
                {
                    progressBar.value = _progress;
                }
                if (progressText != null)
                {
                    progressText.text = $"{Mathf.RoundToInt(_progress * 100f)}%";
                }
            }
        }
        
        public bool IsDone { get; private set; }
        
        
        /// <summary>
        /// 초기화가 완료될 때까지 대기합니다.
        /// </summary>
        /// <param name="callback"></param>
        public static void WaitForInitialization(Action callback)
        {
            if (_initialized)
            {
                callback?.Invoke();
            }
            else
            {
                _initializationCallback += callback;
            }
        }
        
        public static async UniTask WaitUntilInitialized()
        {
            if (_initialized)
            {
                return;
            }
            var tcs = new UniTaskCompletionSource();
            WaitForInitialization(() => tcs.TrySetResult());
            await tcs.Task;
        }
        
        /// <summary>
        /// 씬 초기화가 완료되었음을 알립니다.
        /// </summary>
        /// <remarks>
        /// 이 메서드는 반드시 씬이 모두 로드된 후에 호출되어야 합니다.
        /// </remarks>
        public static void NotifySceneInitialized()
        {
            _initialized = true;
            _initializationCallback?.Invoke();
        }
        
        public static void CheckScenesAndInvokeInitialization()
        {
            var scenes = SceneManager.loadedSceneCount;
            for (int i = 0; i < scenes; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name.Contains("InitialLoader"))
                {
                    return;
                }
            }
        }
        
        private async UniTask StartLoadingScene()
        {
            /*
             * 기초 씬 로드
             */
            int toLoadCount = scenesToLoad.Length;
            int loadedCount = 0;
            Progress = 0f;
            IsDone = false;
            async UniTask LoadSceneAsync(SceneReference sceneRef)
            {
                if (!sceneRef.IsSceneLoaded())
                {
                    var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneRef.SceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                    while (!operation.isDone)
                    {
                        await UniTask.Yield();
                    }
                }
                loadedCount++;
                Progress = (float)loadedCount / toLoadCount;
            }
            List<UniTask> loadTasks = new List<UniTask>();
            foreach (var sceneRef in scenesToLoad)
            {
                loadTasks.Add(LoadSceneAsync(sceneRef));
            }
            // DB 로드
            async UniTask LoadDatabaseAsync()
            {
                await UniTask.WaitUntil(() => DatabaseManager.s_IsInstanced);
                var dbManager = FindAnyObjectByType<DatabaseManager>();
                await UniTask.WaitUntil(() => DatabaseManager.s_IsInitialized);
                loadedCount++;
                Progress = (float)loadedCount / toLoadCount;
            }
            loadTasks.Add(LoadDatabaseAsync());
            
            toLoadCount = loadTasks.Count;
            
            await UniTask.WhenAll(loadTasks);
            Progress = 1f;
            IsDone = true;
            _initialized = true;
            _initializationCallback?.Invoke();
        }
        
        private async UniTask Start()
        {
            DontDestroyOnLoad(gameObject);
     
            await StartLoadingScene();

            await UniTask.Delay(100);
            
            SceneManager.LoadScene(nextScene.SceneName);
            Destroy(gameObject);
            
            // StartLoadingScene().ContinueWith(() =>
            // {
            //     SceneManager.LoadScene(nextScene.SceneName);
            //     Destroy(gameObject);
            // }).Forget();
        }
        
        
        
        #if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void OnEnterPlayMode()
        {
            _initialized = false;
            _initializationCallback = null;
        }
        #endif
        
    }
}