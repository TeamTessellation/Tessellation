using Machamy.Utils;
using SceneManagement;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 싱글톤 패턴을 구현한 MonoBehaviour 기반의 추상 클래스입니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>
        /// 이 싱글톤 인스턴스가 씬 전환 시에도 파괴되지 않도록 설정할지 여부를 나타냅니다.
        /// </summary>
        public abstract bool IsDontDestroyOnLoad { get; }
        public bool destroyPreExisting = false;
        
        protected static T _instance;

        public static bool HasInstance => _instance != null;
        
        public static T Instance
        {
            get
            {
                // Lazy initialization
                if (_instance == null)
                {
                    LogEx.Log("Finding existing instance of " + typeof(T).Name);
                    _instance = FindAnyObjectByType<T>();
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject("@"+typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            
            void SetDontDestroyOnLoad()
            {
                // 매니저 씬이 아닌 경우에만 DontDestroyOnLoad 적용
                if (IsDontDestroyOnLoad)
                {
                    if (gameObject.scene.name != SceneReference.ManagerSceneName)
                    {
                        DontDestroyOnLoad(gameObject);
                    }
                }
            }
            
            if (_instance == null)
            {
                _instance = this as T;
                if (IsDontDestroyOnLoad)
                {
                    SetDontDestroyOnLoad();
                }
                AfterAwake();
            }
            else if (_instance != this)
            {
                LogEx.LogWarning($"Instance of {typeof(T).Name} already exists. Destroying duplicate.");
                if (destroyPreExisting)
                {
                    Destroy(_instance.gameObject);
                    _instance = this as T;
                    // 현재 있는 씬 위치 확인
                    if (IsDontDestroyOnLoad)
                    {
                        SetDontDestroyOnLoad();
                    }
                    AfterAwake();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
        
        
        protected virtual void AfterAwake()
        {
            
        }
    }
}