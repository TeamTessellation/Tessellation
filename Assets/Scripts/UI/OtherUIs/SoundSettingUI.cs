using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sound;
using UnityEngine;
using UnityEngine.UI;

namespace UI.OtherUIs
{
    public class SoundSettingUI : UIBase
    {

        [Serializable]
        public class TransitionSettings
        {
            [Header("들어올때의 설정")]
            [SerializeField] private float fromOffset = -2.5f;
            [SerializeField] private Ease fromMoveEase = Ease.OutBack;
            [SerializeField] private float fromMoveDuration = 0.5f;
            [SerializeField] private float fromFadeDelay = 0.0f;
            [SerializeField] private float fromFadeDuration = 0.5f;
            [SerializeField] private Ease fromFadeEase = Ease.Linear;
            [Header("나갈때의 설정")]
            [SerializeField] private float toOffset = -2.5f;
            [SerializeField] private Ease toMoveEase = Ease.InBack;
            [SerializeField] private float toMoveDuration = 0.5f;
            [SerializeField] private float toFadeDelay = 0.0f;
            [SerializeField] private float toFadeDuration = 0.5f;
            [SerializeField] private Ease toFadeEase = Ease.Linear;
        }
        [Header("UI Components")]
        [SerializeField] private RectTransform soundButtonRectTransform;
        [SerializeField] private RectTransform backButtonRectTransform;
        [SerializeField] private RectTransform soundSliderRectTransform;
        
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        
        [Header("Transition Settings")]
        [SerializeField] private TransitionSettings soundButtonTransitionSettings;
        [SerializeField] private TransitionSettings soundButtonFromPauseTransitionSettings;
        
        [SerializeField] private TransitionSettings backButtonTransitionSettings;
        [SerializeField] private TransitionSettings soundSliderTransitionSettings;

        protected override void Awake()
        {
            base.Awake();
            bgmSlider.onValueChanged.AddListener((value) =>
            {
                SoundManager.Instance.SetMusicVolume(value);
            });
            sfxSlider.onValueChanged.AddListener((value) =>
            {
                SoundManager.Instance.SetSfxVolume(value);
            });
        }

        private void OnEnable()
        {
            bgmSlider.value = SoundManager.Instance.MusicVolume;
            sfxSlider.value = SoundManager.Instance.SfxVolume;
        }

        /// <summary>
        /// 기본적인 표시 동작을 수행합니다.
        /// 각 UI가 왼쪽 오른쪽에서 나옵니다.
        /// ex) 메인화면
        /// </summary>
        public async UniTask ShowDefaultAsync()
        {
            gameObject.SetActive(true);
            
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// 기본적인 숨김 동작을 수행합니다.
        /// ex) 메인화면
        /// </summary>
        public async UniTask HideDefaultAsync()
        {
            gameObject.SetActive(false);
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// 일시정지 화면에서의 표시 동작을 수행합니다.
        /// </summary>
        public async UniTask ShowInPauseAsync()
        {
            gameObject.SetActive(true);
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// 일시정지 화면에서의 숨김 동작을 수행합니다.
        /// </summary>
        public async UniTask HideInPauseAsync()
        {
            gameObject.SetActive(false);
            await UniTask.CompletedTask;
        }
        
    }
}