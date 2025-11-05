
using System.Runtime.CompilerServices;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace SceneManagement{
    
    /// <summary>
    /// 해당 씬이 로드될 때 지정된 씬을 추가로 로드하는 컴포넌트입니다.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class SceneInitializer : MonoBehaviour
    {
        [SerializeField] private SceneReference[] sceneToLoad;

        private async UniTaskVoid Awake()
        {
            
            await SceneUtil.LoadScenesAsync(sceneToLoad);
            
            InitialLoader.NotifySceneInitialized();
            Destroy(gameObject);
        }
        
    }
}

