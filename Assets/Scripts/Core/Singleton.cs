﻿using Machamy.Utils;
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
        
        protected static T _instance;

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
        
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (IsDontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
                AfterAwake();
            }
            else if (_instance != this)
            {
                LogEx.LogWarning($"Instance of {typeof(T).Name} already exists. Destroying duplicate.");
                Destroy(gameObject);
            }
        }
        
        protected virtual void AfterAwake()
        {
            
        }
    }
}