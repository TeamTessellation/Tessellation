using System;
using System.Threading;
using Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sound;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.OtherUIs
{
    public class SoundSettingUI : UIBase
    {

        [Serializable]
        public class TransitionSettings
        {
            [Header("들어올때의 설정")]
            [SerializeField] public float fromOffset = -2.5f;
            [SerializeField] public Ease fromMoveEase = Ease.OutCubic;
            [SerializeField] public float fromMoveDuration = 0.5f;
            [SerializeField] public float fromFadeDelay = 0.0f;
            [SerializeField] public float fromFadeDuration = 0.5f;
            [SerializeField] public Ease fromFadeEase = Ease.Linear;
            [Header("나갈때의 설정")]
            [SerializeField] public float toOffset = -2.5f;
            [SerializeField] public Ease toMoveEase = Ease.InCubic;
            [SerializeField] public float toMoveDuration = 0.5f;
            [SerializeField] public float toFadeDelay = 0.0f;
            [SerializeField] public float toFadeDuration = 0.5f;
            [SerializeField] public Ease toFadeEase = Ease.Linear;
        }
        [Header("UI Components")]
        [SerializeField] private RectTransform soundButtonRectTransform;
        [SerializeField] private RectTransform backButtonRectTransform;
        [SerializeField] private RectTransform soundSliderRectTransform;
        [Space(5)]
        [SerializeField] private CanvasGroup backgroundImage;
        [SerializeField] private Button backButton;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        
        [Header("Transition Settings")]
        [SerializeField] private TransitionSettings soundButtonTransitionSettings;
        [SerializeField] private TransitionSettings soundButtonFromPauseTransitionSettings;
        
        [SerializeField] private TransitionSettings backButtonTransitionSettings;
        [SerializeField] private TransitionSettings soundSliderTransitionSettings;


        private readonly SerializableDictionary<RectTransform, Vector2> _originalPositions = new();
        
        public enum ReturnType
        {
            ToMainUI,
            ToPauseUI
        }
        private ReturnType returnType = ReturnType.ToMainUI;
        
        private CancellationTokenSource enabledCancellationTokenSource;
        private CancellationTokenSource disabledCancellationTokenSource;
        private CancellationTokenSource cancellationTokenSource;
        
        protected override void Awake()
        {
            base.Awake();
            
            enabledCancellationTokenSource = new CancellationTokenSource();
            disabledCancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                enabledCancellationTokenSource.Token,
                disabledCancellationTokenSource.Token);
            backButton.onClick.AddListener(OnClickBackButton);
            bgmSlider.onValueChanged.AddListener((value) =>
            {
                SoundManager.Instance.SetMusicVolume(value);
            });
            sfxSlider.onValueChanged.AddListener((value) =>
            {
                SoundManager.Instance.SetSfxVolume(value);
            });
            _originalPositions.Clear();
            
            void StoreOriginalPosition(RectTransform rectTransform)
            {
                if (rectTransform != null)
                {
                    _originalPositions.Add(rectTransform, rectTransform.anchoredPosition);
                }
            }
            StoreOriginalPosition(soundButtonRectTransform);
            StoreOriginalPosition(backButtonRectTransform);
            StoreOriginalPosition(soundSliderRectTransform);
      
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (SoundManager.HasInstance)
            {
                bgmSlider.value = SoundManager.Instance.MusicVolume;
                sfxSlider.value = SoundManager.Instance.SfxVolume;
            }

        }
        public void CancelEnabledTransitions()
        {
            enabledCancellationTokenSource.Cancel();
            enabledCancellationTokenSource.Dispose();
            enabledCancellationTokenSource = new CancellationTokenSource();
        }
        public void CancelDisabledTransitions()
        {
            disabledCancellationTokenSource.Cancel();
            disabledCancellationTokenSource.Dispose();
            disabledCancellationTokenSource = new CancellationTokenSource();
        }
        public void CancelAllTransitions()
        {
            enabledCancellationTokenSource.Cancel();
            disabledCancellationTokenSource.Cancel();
            
            enabledCancellationTokenSource.Dispose();
            disabledCancellationTokenSource.Dispose();
            cancellationTokenSource.Dispose();

            enabledCancellationTokenSource = new CancellationTokenSource();
            disabledCancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                enabledCancellationTokenSource.Token,
                disabledCancellationTokenSource.Token);
        }
        
        /// <summary>
        /// 기본적인 표시 동작을 수행합니다.
        /// 각 UI가 왼쪽 오른쪽에서 나옵니다.
        /// ex) 메인화면
        /// </summary>
        public async UniTask ShowDefaultAsync()
        {
            gameObject.SetActive(true);
            returnType = ReturnType.ToMainUI;
            var leftSequence = DOTween.Sequence();
            
            // 왼쪽 UI들의 시작 위치 설정
            soundButtonRectTransform.anchoredPosition = _originalPositions[soundButtonRectTransform] +
                                                Vector2.right * soundButtonTransitionSettings.fromOffset *
                                                soundButtonRectTransform.GetRectSize().x;
            backButtonRectTransform.anchoredPosition = _originalPositions[backButtonRectTransform] +
                                                       Vector2.right * backButtonTransitionSettings.fromOffset *
                                                       backButtonRectTransform.GetRectSize().x;
            
            // 왼쪽 UI들 애니메이션
            leftSequence.Append(soundButtonRectTransform.DOAnchorPos(_originalPositions[soundButtonRectTransform],
                soundButtonTransitionSettings.fromMoveDuration).SetEase(soundButtonTransitionSettings.fromMoveEase));
            leftSequence.Join(backButtonRectTransform.DOAnchorPos(_originalPositions[backButtonRectTransform],
                backButtonTransitionSettings.fromMoveDuration).SetEase(backButtonTransitionSettings.fromMoveEase));
            
            leftSequence.Join(soundButtonRectTransform.GetComponent<CanvasGroup>().DOFade(1.0f,
                soundButtonTransitionSettings.fromFadeDuration).SetEase(soundButtonTransitionSettings.fromFadeEase)
                .SetDelay(soundButtonTransitionSettings.fromFadeDelay));
            leftSequence.Join(backButtonRectTransform.GetComponent<CanvasGroup>().DOFade(1.0f,
                backButtonTransitionSettings.fromFadeDuration).SetEase(backButtonTransitionSettings.fromFadeEase)
                .SetDelay(backButtonTransitionSettings.fromFadeDelay));
            
            var rightSequence = DOTween.Sequence();
            
            // 오른쪽 UI의 시작 위치 설정 
            soundSliderRectTransform.anchoredPosition = _originalPositions[soundSliderRectTransform] +
                                                Vector2.right * soundSliderTransitionSettings.fromOffset *
                                                soundSliderRectTransform.GetRectSize().x;
            
            // 오른쪽 UI 애니메이션 
            rightSequence.Append(soundSliderRectTransform.DOAnchorPos(_originalPositions[soundSliderRectTransform],
                soundSliderTransitionSettings.fromMoveDuration).SetEase(soundSliderTransitionSettings.fromMoveEase));
            rightSequence.Join(soundSliderRectTransform.GetComponent<CanvasGroup>().DOFade(1.0f,
                soundSliderTransitionSettings.fromFadeDuration).SetEase(soundSliderTransitionSettings.fromFadeEase)
                .SetDelay(soundSliderTransitionSettings.fromFadeDelay));

            var allSequence = DOTween.Sequence();
            allSequence.Join(leftSequence);
            allSequence.Join(rightSequence);
            allSequence.Join(backgroundImage.DOFade(1.0f, 0.2f).SetEase(Ease.InOutSine));
            allSequence.SetUpdate(true);
            await allSequence.Play().ToUniTask(cancellationToken: cancellationTokenSource.Token);
        }

        /// <summary>
        /// 기본적인 숨김 동작을 수행합니다.
        /// ex) 메인화면
        /// </summary>
        public async UniTask HideDefaultAsync()
        {
            var leftSequence = DOTween.Sequence();
            
            // 왼쪽 UI들 애니메이션
            leftSequence.Append(soundButtonRectTransform.DOAnchorPos(
                _originalPositions[soundButtonRectTransform] +
                Vector2.right * soundButtonTransitionSettings.toOffset *
                soundButtonRectTransform.GetRectSize().x,
                soundButtonTransitionSettings.toMoveDuration).SetEase(soundButtonTransitionSettings.toMoveEase));
            leftSequence.Join(backButtonRectTransform.DOAnchorPos(
                _originalPositions[backButtonRectTransform] +
                Vector2.right * backButtonTransitionSettings.toOffset *
                backButtonRectTransform.GetRectSize().x,
                backButtonTransitionSettings.toMoveDuration).SetEase(backButtonTransitionSettings.toMoveEase));
            
            leftSequence.Join(soundButtonRectTransform.GetComponent<CanvasGroup>().DOFade(0.0f,
                soundButtonTransitionSettings.toFadeDuration).SetEase(soundButtonTransitionSettings.toFadeEase)
                .SetDelay(soundButtonTransitionSettings.toFadeDelay));
            leftSequence.Join(backButtonRectTransform.GetComponent<CanvasGroup>().DOFade(0.0f,
                backButtonTransitionSettings.toFadeDuration).SetEase(backButtonTransitionSettings.toFadeEase)
                .SetDelay(backButtonTransitionSettings.toFadeDelay));
            
            var rightSequence = DOTween.Sequence();
            
            // 오른쪽 UI 애니메이션
            rightSequence.Append(soundSliderRectTransform.DOAnchorPos(
                _originalPositions[soundSliderRectTransform] +
                Vector2.right * soundSliderTransitionSettings.toOffset *
                soundSliderRectTransform.GetRectSize().x,
                soundSliderTransitionSettings.toMoveDuration).SetEase(soundSliderTransitionSettings.toMoveEase));
            rightSequence.Join(soundSliderRectTransform.GetComponent<CanvasGroup>().DOFade(0.0f,
                soundSliderTransitionSettings.toFadeDuration).SetEase(soundSliderTransitionSettings.toFadeEase)
                .SetDelay(soundSliderTransitionSettings.toFadeDelay));
            
            var allSequence = DOTween.Sequence();
            allSequence.Join(leftSequence);
            allSequence.Join(rightSequence);
            allSequence.Join(backgroundImage.DOFade(0.0f, 0.2f).SetEase(Ease.InOutSine));
            allSequence.SetUpdate(true);
            await allSequence.Play().ToUniTask(cancellationToken: cancellationTokenSource.Token);
            
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 일시정지 화면에서의 표시 동작을 수행합니다.
        /// </summary>
        public async UniTask ShowInPauseAsync(RectTransform prevSoundButtonRect, CancellationToken cancellationToken = default)
        {
            returnType = ReturnType.ToPauseUI;
            gameObject.SetActive(true);
            
            // 사운드 버튼 위치 동기화
            soundButtonRectTransform.GetComponent<CanvasGroup>().alpha = 1.0f;
            backButtonRectTransform.GetComponent<CanvasGroup>().alpha = 0.0f;
            soundButtonRectTransform.position = prevSoundButtonRect.position;
            backButtonRectTransform.anchoredPosition = _originalPositions[soundButtonRectTransform];
            var leftSequence = DOTween.Sequence();
            // 왼쪽 UI들 애니메이션(사운드 이동 -> 그 밑으로 돌아가기 나타남)
            leftSequence.Append(soundButtonRectTransform.DOAnchorPos(_originalPositions[soundButtonRectTransform],
                    soundButtonFromPauseTransitionSettings.fromMoveDuration)
                .SetEase(soundButtonFromPauseTransitionSettings.fromMoveEase));
            leftSequence.Append(backButtonRectTransform.DOAnchorPos(_originalPositions[backButtonRectTransform],
                backButtonTransitionSettings.fromMoveDuration).SetEase(backButtonTransitionSettings.fromMoveEase));
            leftSequence.Join(backButtonRectTransform.GetComponent<CanvasGroup>().DOFade(1.0f,
                backButtonTransitionSettings.fromFadeDuration).SetEase(backButtonTransitionSettings.fromFadeEase)
                .SetDelay(backButtonTransitionSettings.fromFadeDelay));
            
            // 오른쪽은 그대로
            var rightSequence = DOTween.Sequence();
            rightSequence.Append(soundSliderRectTransform.DOAnchorPos(_originalPositions[soundSliderRectTransform],
                soundSliderTransitionSettings.fromMoveDuration).SetEase(soundSliderTransitionSettings.fromMoveEase));
            rightSequence.Join(soundSliderRectTransform.GetComponent<CanvasGroup>().DOFade(1.0f,
                soundSliderTransitionSettings.fromFadeDuration).SetEase(soundSliderTransitionSettings.fromFadeEase)
                .SetDelay(soundSliderTransitionSettings.fromFadeDelay));  
            
            var allSequence = DOTween.Sequence();
            allSequence.Join(leftSequence);
            allSequence.Join(rightSequence);
            allSequence.SetUpdate(true);
            await allSequence.Play().ToUniTask(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 일시정지 화면에서의 숨김 동작을 수행합니다.
        /// </summary>
        public async UniTask HideInPauseAsync(CancellationToken cancellationToken = default)
        {
            var leftSequence = DOTween.Sequence();
            
            // 왼쪽 UI들 애니메이션(돌아가기가 사운드 버튼 밑으로 사라짐)
            // 이후 이동은 PauseUI에서 처리
            leftSequence.Append(backButtonRectTransform.DOAnchorPos(
                _originalPositions[soundButtonRectTransform],
                backButtonTransitionSettings.toMoveDuration).SetEase(backButtonTransitionSettings.toMoveEase));
            leftSequence.Join(backButtonRectTransform.GetComponent<CanvasGroup>().DOFade(0.0f,  
                backButtonTransitionSettings.toFadeDuration).SetEase(backButtonTransitionSettings.toFadeEase)
                .SetDelay(backButtonTransitionSettings.toFadeDelay));

            var rightSequence = DOTween.Sequence();
            // 오른쪽 UI 애니메이션
            rightSequence.Append(soundSliderRectTransform.DOAnchorPos(
                _originalPositions[soundSliderRectTransform] +
                Vector2.right * soundSliderTransitionSettings.toOffset *
                soundSliderRectTransform.GetRectSize().x,
                soundSliderTransitionSettings.toMoveDuration).SetEase(soundSliderTransitionSettings.toMoveEase));
            rightSequence.Join(soundSliderRectTransform.GetComponent<CanvasGroup>().DOFade(0.0f,
                soundSliderTransitionSettings.toFadeDuration).SetEase(soundSliderTransitionSettings.toFadeEase)
                .SetDelay(soundSliderTransitionSettings.toFadeDelay));
            
            var allSequence = DOTween.Sequence();
            allSequence.Join(leftSequence);
            allSequence.Join(rightSequence);
            allSequence.SetUpdate(true);
            await allSequence.Play().ToUniTask(cancellationToken: cancellationToken);
            
            gameObject.SetActive(false);
            
            await UIManager.Instance.PauseUI.SwitchBackFromSoundSettingsAsync(soundButtonRectTransform, cancellationToken);
        }
        
        public void OnClickBackButton()
        {
            if (returnType == ReturnType.ToMainUI)
            {
                CancelAllTransitions();
                HideDefaultAsync().Forget();
            }
            else if (returnType == ReturnType.ToPauseUI)
            {
                CancelAllTransitions();
                HideInPauseAsync().Forget();
            }
        }
        
        
        
        
        
        
        
        
        
        [ContextMenu("Copy From SoundButtonTransitionSettings")]

        private void CopyFromSoundButtonTransitionSettings()
        { 
            // soundButtonFromPauseTransitionSettings.fromOffset = soundButtonTransitionSettings.fromOffset;
            // soundButtonFromPauseTransitionSettings.fromMoveEase = soundButtonTransitionSettings.fromMoveEase;
            // soundButtonFromPauseTransitionSettings.fromMoveDuration = soundButtonTransitionSettings.fromMoveDuration;
            // soundButtonFromPauseTransitionSettings.fromFadeDelay = soundButtonTransitionSettings.fromFadeDelay;
            // soundButtonFromPauseTransitionSettings.fromFadeDuration = soundButtonTransitionSettings.fromFadeDuration;
            // soundButtonFromPauseTransitionSettings.fromFadeEase = soundButtonTransitionSettings.fromFadeEase;
            //
            // soundButtonFromPauseTransitionSettings.toOffset = soundButtonTransitionSettings.toOffset;
            // soundButtonFromPauseTransitionSettings.toMoveEase = soundButtonTransitionSettings.toMoveEase;
            // soundButtonFromPauseTransitionSettings.toMoveDuration = soundButtonTransitionSettings.toMoveDuration;
            // soundButtonFromPauseTransitionSettings.toFadeDelay = soundButtonTransitionSettings.toFadeDelay;
            // soundButtonFromPauseTransitionSettings.toFadeDuration = soundButtonTransitionSettings.toFadeDuration;
            // soundButtonFromPauseTransitionSettings.toFadeEase = soundButtonTransitionSettings.toFadeEase;
             
            backButtonTransitionSettings.fromOffset = soundButtonTransitionSettings.fromOffset;
            backButtonTransitionSettings.fromMoveEase = soundButtonTransitionSettings.fromMoveEase;
            backButtonTransitionSettings.fromMoveDuration = soundButtonTransitionSettings.fromMoveDuration;
            backButtonTransitionSettings.fromFadeDelay = soundButtonTransitionSettings.fromFadeDelay;
            backButtonTransitionSettings.fromFadeDuration = soundButtonTransitionSettings.fromFadeDuration;
            backButtonTransitionSettings.fromFadeEase = soundButtonTransitionSettings.fromFadeEase;
            
            backButtonTransitionSettings.toOffset = soundButtonTransitionSettings.toOffset;
            backButtonTransitionSettings.toMoveEase = soundButtonTransitionSettings.toMoveEase;
            backButtonTransitionSettings.toMoveDuration = soundButtonTransitionSettings.toMoveDuration;
            backButtonTransitionSettings.toFadeDelay = soundButtonTransitionSettings.toFadeDelay;
            backButtonTransitionSettings.toFadeDuration = soundButtonTransitionSettings.toFadeDuration;
            backButtonTransitionSettings.toFadeEase = soundButtonTransitionSettings.toFadeEase;
            
            soundSliderTransitionSettings.fromOffset = soundButtonTransitionSettings.fromOffset;
            soundSliderTransitionSettings.fromMoveEase = soundButtonTransitionSettings.fromMoveEase;
            soundSliderTransitionSettings.fromMoveDuration = soundButtonTransitionSettings.fromMoveDuration;
            soundSliderTransitionSettings.fromFadeDelay = soundButtonTransitionSettings.fromFadeDelay;
            soundSliderTransitionSettings.fromFadeDuration = soundButtonTransitionSettings.fromFadeDuration;
            soundSliderTransitionSettings.fromFadeEase = soundButtonTransitionSettings.fromFadeEase;  
            
            soundSliderTransitionSettings.toOffset = soundButtonTransitionSettings.toOffset;
            soundSliderTransitionSettings.toMoveEase = soundButtonTransitionSettings.toMoveEase;
            soundSliderTransitionSettings.toMoveDuration = soundButtonTransitionSettings.toMoveDuration;
            soundSliderTransitionSettings.toFadeDelay = soundButtonTransitionSettings.toFadeDelay;
            soundSliderTransitionSettings.toFadeDuration = soundButtonTransitionSettings.toFadeDuration;
            soundSliderTransitionSettings.toFadeEase = soundButtonTransitionSettings.toFadeEase;
        }
    }
}