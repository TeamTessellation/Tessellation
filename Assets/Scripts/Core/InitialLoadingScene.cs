using Cysharp.Threading.Tasks;
using SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class InitialLoadingScene : MonoBehaviour
    {
        private static bool _initialized = false;
        
        public static bool Initialized => _initialized;
        
        [SerializeField] private SceneReference[] scenesToLoad;
        [SerializeField] private SceneReference nextScene;
        
        private float _progress;
        
        public float Progress
        {
            get => _progress;
            private set
            {
                _progress = Mathf.Clamp01(value);
                Debug.Log($"Loading Progress: {_progress * 100f}%");
            }
        }
        public bool IsDone { get; private set; }
        
        private async UniTask StartLoadingScene()
        {
            /*
             * 기초 씬 로드
             */
            int totalScenes = scenesToLoad.Length;
            int loadedScenes = 0;
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
                loadedScenes++;
                Progress = (float)loadedScenes / totalScenes;
            }
            
            await UniTask.WhenAll(scenesToLoad.Select(LoadSceneAsync));
            Progress = 1f;
            IsDone = true;
            _initialized = true;
        }
        
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            StartLoadingScene().ContinueWith(() =>
            {
                SceneManager.LoadScene(nextScene.SceneName);
                Destroy(gameObject);
            }).Forget();
        }
        
        
    }
}