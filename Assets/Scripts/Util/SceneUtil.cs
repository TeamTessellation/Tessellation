using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using SceneManagement;


namespace System.Runtime.CompilerServices
{
    public static class SceneUtil
    {
        public static async UniTask LoadScenesAsync(IEnumerable<SceneReference> sceneReferences)
        {
            var loadTasks = new List<UniTask>();
            foreach (var sceneRef in sceneReferences)
            {
                if (sceneRef == null) continue;
                // 이미 씬이 로드되어 있는지 확인
                if (sceneRef.IsSceneLoaded())
                {
                    continue;
                }
                loadTasks.Add(UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneRef.SceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive).ToUniTask());
            }
            await UniTask.WhenAll(loadTasks);
        }
        
        public static async UniTask LoadScenesAsync(IEnumerable<string> sceneNames)
        {
            var loadTasks = new List<UniTask>();
            foreach (var sceneName in sceneNames)
            {
                // 이미 씬이 로드되어 있는지 확인
                if (IsSceneLoaded(sceneName))
                {
                    continue;
                }
                loadTasks.Add(UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive).ToUniTask());
            }
            await UniTask.WhenAll(loadTasks);
        }
        
        public static bool IsSceneLoaded(SceneReference sceneReference)
        {
            return IsSceneLoaded(sceneReference.SceneName);
        }
        public static bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene.name == sceneName)
                {
                    return true;
                }
            }
            return false;
        }
    }
    
    public static class SceneExtensions
    {
        public static bool IsSceneLoaded(this SceneReference sceneReference)
        {
            return SceneUtil.IsSceneLoaded(sceneReference);
        }
    }
}