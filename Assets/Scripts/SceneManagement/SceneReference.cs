using UnityEngine;

namespace SceneManagement
{
    [CreateAssetMenu(fileName = "SceneReference", menuName = "Scene Management/Scene Reference", order = 1)]
    public class SceneReference : ScriptableObject
    {
        public static string MainSceneName = "MainScene";
        public static string ManagerSceneName = "ManagerScene";
        
#if UNITY_EDITOR
        [SerializeField] private UnityEditor.SceneAsset sceneAsset;
#endif
        [SerializeField] private string sceneName;
        public string SceneName => sceneName;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (sceneAsset != null)
            {
                sceneName = sceneAsset.name;
            }
        }
#endif

    }
}