using UnityEngine;

namespace SceneManagement
{
    [CreateAssetMenu(fileName = "SceneReference", menuName = "Scene Management/Scene Reference", order = 1)]
    public class SceneReference : ScriptableObject
    {
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
        public bool IsSceneLoaded()
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
}