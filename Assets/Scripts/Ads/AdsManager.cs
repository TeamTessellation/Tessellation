using Core;
using Machamy.Attributes;
using UnityEngine;
using UnityEngine.Advertisements;

namespace Ads
{
    public class AdsManager : Singleton<EffectManager>, IUnityAdsInitializationListener
    {
        public override bool IsDontDestroyOnLoad => true;

        [SerializeField] private string _androidId;
        [SerializeField] private string _iosId;
        [SerializeField, VisibleOnly] private bool _testMode = true;
        
        private string _gameId;
        
        protected override void AfterAwake()
        {
            base.AfterAwake();
            #if UNITY_EDITOR
            _testMode = true;
            #else
            _testMode = false;
            #endif
            InitializeAds();
        }

        public void InitializeAds()
        {
#if UNITY_IOS
        _gameId = _iosId;
#elif UNITY_ANDROID
        _gameId = _androidId;
#elif UNITY_EDITOR
            _gameId = _androidId; //Only for testing the functionality in the Editor
#endif
            if (!Advertisement.isInitialized && Advertisement.isSupported)
            {
                Advertisement.Initialize(_gameId, _testMode, this);
            }
        }

        public void OnInitializationComplete()
        {
            Debug.Log("Unity Ads initialization complete.");
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
        }
    }
}