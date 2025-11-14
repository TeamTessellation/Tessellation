
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;


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
            if(InitialLoader.NotStartedInitialization)
            {
                // 아직 초기화가 시작되지 않은 경우, 씬을 로드합니다.
                await SceneUtil.LoadScenesAsync(sceneToLoad);

                await UniTask.Delay(200);
                InitialLoader.NotifySceneInitialized();
            }
            Destroy(gameObject);
        }
        
    }
}

