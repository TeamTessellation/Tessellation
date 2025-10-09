using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneManagement{
    
    /// <summary>
    /// 해당 씬이 로드될 때 지정된 씬을 추가로 로드하는 컴포넌트입니다.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class SceneInitializer : MonoBehaviour
    {
        [SerializeField] private SceneReference[] sceneToLoad;

        private void Awake()
        {
            foreach (var sceneRef in sceneToLoad)
            {
                if (sceneRef == null) continue;
                if (!sceneRef.IsSceneLoaded())
                {
                    SceneManager.LoadSceneAsync(sceneRef.SceneName, LoadSceneMode.Additive);
                }
            }
            Destroy(gameObject);
        }
        
    }
}

