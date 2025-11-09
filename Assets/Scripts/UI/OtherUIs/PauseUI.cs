using System;
using System.Collections.Generic;
using System.Threading;
using Collections;
using Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Machamy.Attributes;
using Machamy.Utils;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.OtherUIs
{
    public class PauseUI : UIBase
    {
        // [SerializeField] private Image pauseBackgroundPanel;

        [Header("Popup Elements")]
        [SerializeField] private GameObject pausePopup;
        [SerializeField] private Button soundButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button homeButton;
        
        
        [Space(20)]
        [Header("Transitions")]
        

        [SerializeField]private float onDisableTransitionDuration = 0.5f;
        [SerializeField]private Ease onDisableTransitionEase = Ease.InBack;

        [Space(5)] [Header("Transition Map")]
        [SerializeField] private Transform originalPositionsHolder;
        [SerializeField] private TransitionInfo onEnableTransition;
        [SerializeField] private TransitionInfo onDisableTransition;    
        [SerializeField] private RectTransform center;

        [SerializeField, Tooltip("1차로 나오는 UI들")]
        private List<RectTransform> mainTiles;
        [SerializeField, Tooltip("2차로 나오는 UI들")]
        private List<RectTransform> subTiles;
        
        [SerializeField,VisibleOnly(EditableIn.EditMode)] SerializableDictionary<RectTransform, RectTransform> _originalPositions = new ();


        [Serializable]
        struct TransitionInfo
        {
            // 변수명 짓기 귀찮아서 1,2 붙임
            [FormerlySerializedAs("onEnableDelay1")] [SerializeField,Tooltip("시작 딜레이")] public float delay1; 
            [FormerlySerializedAs("onEnableTransitionDuration1")] [SerializeField,Tooltip("1차 애니메이션 시간")] public float transitionDuration1;
            [FormerlySerializedAs("onEnableTransitionEase1")] [SerializeField,Tooltip("1차 애니메이션 Ease")] public Ease transitionEase1;
            [FormerlySerializedAs("onEnableDelay2")] [SerializeField,Tooltip("2차 딜레이")] public float delay2; 
            [FormerlySerializedAs("onEnableTransitionDuration2")] [SerializeField,Tooltip("2차 애니메이션 시간")] public float transitionDuration2;
            [FormerlySerializedAs("onEnableTransitionEase2")] [SerializeField,Tooltip("2차 애니메이션 Ease")] public Ease transitionEase2;
        }
        
        protected override void Reset()
        {
            base.Reset();
            // pauseBackgroundPanel = transform.GetChild(0).GetComponent<Image>();
            pausePopup = transform.Find("PausePopup").gameObject;
            var buttons = pausePopup.GetComponentsInChildren<Button>();
            
            
            onEnableTransition = new TransitionInfo
            {
                delay1 = 0.1f,
                transitionDuration1 = 0.4f,
                transitionEase1 = Ease.OutBack,
                delay2 = 0.2f,
                transitionDuration2 = 0.5f,
                transitionEase2 = Ease.OutBack
            };
            onDisableTransition = new TransitionInfo
            {
                delay1 = 0.0f,
                transitionDuration1 = 0.2f,
                transitionEase1 = Ease.InBack,
                delay2 = 0.0f,
                transitionDuration2 = 0.2f,
                transitionEase2 = Ease.InBack
            };
            
            
            
            
            void BindButton(ref Button buttonField, int index)
            {
                if (index >= 0 && index < buttons.Length)
                {
                    buttonField = buttons[index];
                }
                else
                {
                    LogEx.LogError("버튼의 인덱스가 범위를 벗어났습니다: " + index);
                }
            }
            BindButton(ref resumeButton, 0);
            BindButton(ref soundButton, 1);
            BindButton(ref homeButton, 2);
            BindButton(ref retryButton, 3);
            
        }
        
        private CancellationTokenSource enabledCancellationTokenSource;
        private CancellationTokenSource disabledCancellationTokenSource;
        private CancellationTokenSource cancellationTokenSource;

        [ContextMenu("test awake")]
        public void TestAwake()
        {
            Awake();
        }
        
        protected override void Awake()
        {
            base.Awake();
            enabledCancellationTokenSource = new CancellationTokenSource();
            disabledCancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                enabledCancellationTokenSource.Token,
                disabledCancellationTokenSource.Token);
            
            // 원래 위치 저장
            var originalHolder = originalPositionsHolder != null ? originalPositionsHolder : new GameObject("OriginalPositionsHolder").transform;
            originalHolder.SetParent(transform, false);
            void StoreOriginal(RectTransform tile)
            {
                var go = new GameObject(tile.name + "_OriginalPosition");
                go.transform.SetParent(originalHolder.transform, false);
                go.transform.localPosition = tile.localPosition;
                _originalPositions[tile] = go.AddComponent<RectTransform>();
            }
            foreach (var tile in mainTiles)
            {
                StoreOriginal(tile);
            }

            foreach (var tile in subTiles)
            {
                StoreOriginal(tile);
            }
        }
        
        private void OnEnable()
        {
            resumeButton.onClick.AddListener(OnResumeButtonClicked);
            homeButton.onClick.AddListener(OnHomeButtonClicked);
            ;
        }
        
        private void OnDisable()
        {
            resumeButton.onClick.RemoveListener(OnResumeButtonClicked);
            homeButton.onClick.RemoveListener(OnHomeButtonClicked);
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
            enabledCancellationTokenSource?.Cancel();
            disabledCancellationTokenSource?.Cancel();
            
            enabledCancellationTokenSource?.Dispose();
            disabledCancellationTokenSource?.Dispose();
            cancellationTokenSource?.Dispose();

            enabledCancellationTokenSource = new CancellationTokenSource();
            disabledCancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                enabledCancellationTokenSource.Token,
                disabledCancellationTokenSource.Token);
        }
        
        
        
        public void Show()
        {
            if (gameObject.activeSelf) return;
            CancelAllTransitions();
            
            gameObject.SetActive(true); 
            
            PlayOnEnableRoutine(enabledCancellationTokenSource.Token).Forget();
        }
        
        public void Hide()
        {
            if (!gameObject.activeSelf) return;
            CancelAllTransitions();
            
            
            HideAndDisableAsync(disabledCancellationTokenSource.Token).Forget();
        }
        
        private void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            enabledCancellationTokenSource?.Cancel();
            enabledCancellationTokenSource?.Dispose();
            disabledCancellationTokenSource?.Cancel();
            disabledCancellationTokenSource?.Dispose();
        }
        
        private async UniTaskVoid PlayOnEnableRoutine(CancellationToken token)
        {
            try
            {
                await PlayOnEnableTransitionsAsync(token);
            }
            catch (OperationCanceledException)
            {
                LogEx.Log("PlayOnEnableRoutine 작업이 취소되었습니다.");
            }
        }
        

        /// <summary>
        /// Hide 애니메이션을 재생하고, 완료되면 스스로 비활성화하는 헬퍼
        /// </summary>
        private async UniTask HideAndDisableAsync(CancellationToken cancellationToken)
        {
            try
            {
                await PlayOnDisableTransitionsAsync(cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    gameObject.SetActive(false);
                }
            }
            catch (OperationCanceledException)
            {
                LogEx.Log("HideAndDisableAsync 작업이 취소되었습니다.");
            }
        }
        
        
        public async UniTask PlayOnEnableTransitionsAsync(CancellationToken cancellationToken)
        {
            using var handle = ListPool<UniTask>.Get(out var transitionTasks);
            foreach (var tile in GetAllTiles())
            {
                tile.gameObject.SetActive(false);
            }
            center.gameObject.SetActive(true);
            // 1차 타일들 애니메이션
            // 중앙 -> 각자 위치로
            await UniTask.Delay(TimeSpan.FromSeconds(onEnableTransition.delay1), 
                delayType: DelayType.UnscaledDeltaTime,
                cancellationToken: cancellationToken);
            
            foreach (var tile in mainTiles)
            {
                tile.gameObject.SetActive(true);
                var startPos = tile.transform.parent.InverseTransformPoint(center.transform.position);
                tile.localPosition = startPos;
                var originalPos = _originalPositions[tile].localPosition;
                var task = tile.DOLocalMove(originalPos, onEnableTransition.transitionDuration1)
                    .SetEase(onEnableTransition.transitionEase1)
                    .SetUpdate(true)
                    .ToUniTask(cancellationToken: cancellationToken);
                transitionTasks.Add(task);
            } 
            await UniTask.WhenAll(transitionTasks);
            transitionTasks.Clear();
    
            // 2차 타일들 애니메이션
            // 짝위치 -> 각자 위치로
            await UniTask.Delay(TimeSpan.FromSeconds(onEnableTransition.delay2), 
                delayType: DelayType.UnscaledDeltaTime,
                cancellationToken: cancellationToken);
    
            for(int i = 0; i < subTiles.Count; i++)
            {
                var tile = subTiles[i];
                tile.gameObject.SetActive(true);
                var startPos = tile.transform.parent.InverseTransformPoint(mainTiles[i % mainTiles.Count].position);
                tile.localPosition = startPos;
                var originalPos = _originalPositions[tile].localPosition;
                var task = tile.DOLocalMove(originalPos, onEnableTransition.transitionDuration2)
                    .SetEase(onEnableTransition.transitionEase2)
                    .SetUpdate(true)
                    .ToUniTask(cancellationToken: cancellationToken);
                transitionTasks.Add(task);
            }
            await UniTask.WhenAll(transitionTasks);
        }
        
        public async UniTask PlayOnDisableTransitionsAsync(CancellationToken cancellationToken)
        {
            using var handle = ListPool<UniTask>.Get(out var transitionTasks);
    
            foreach (var tile in GetAllTiles())
            {
                tile.gameObject.SetActive(true);
            }
            LogEx.Log("PlayOnDisableTransitionsAsync 시작");
            // 1차 타일들 애니메이션
            await UniTask.Delay(TimeSpan.FromSeconds(onDisableTransition.delay1), 
                delayType: DelayType.UnscaledDeltaTime,
                cancellationToken: cancellationToken);
    
            LogEx.Log("1차 타일들 애니메이션 시작");

            for (int i = 0; i < subTiles.Count; i++)
            {
                var tile = subTiles[i];
                var targetPos = tile.transform.parent.InverseTransformPoint(mainTiles[i % mainTiles.Count].position);
                var task = tile.DOLocalMove(targetPos, onDisableTransition.transitionDuration1)
                    .SetEase(onDisableTransition.transitionEase1)
                    .SetUpdate(true)
                    .OnComplete(() => tile.gameObject.SetActive(false))
                    .ToUniTask(cancellationToken: cancellationToken);
                transitionTasks.Add(task);
            }
            await UniTask.WhenAll(transitionTasks);
            transitionTasks.Clear();    
            
            // 2차 타일들 애니메이션
            await UniTask.Delay(TimeSpan.FromSeconds(onDisableTransition.delay2), 
                delayType: DelayType.UnscaledDeltaTime,
                cancellationToken: cancellationToken); 
            LogEx.Log("2차 타일들 애니메이션 시작");
            foreach (var tile in mainTiles)
            { 
                var targetPos = tile.transform.parent.InverseTransformPoint(center.transform.position);
                var task = tile.DOLocalMove(targetPos, onDisableTransition.transitionDuration2)
                    .SetEase(onDisableTransition.transitionEase2)
                    .SetUpdate(true)
                    .OnComplete(() => tile.gameObject.SetActive(false))
                    .ToUniTask(cancellationToken: cancellationToken);
                transitionTasks.Add(task);
            }
            await UniTask.WhenAll(transitionTasks);
        }   
        
        
        public IEnumerable<RectTransform> GetAllTiles()
        {
            yield return center;
            foreach (var tile in mainTiles)
            {
                yield return tile;
            }
            foreach (var tile in subTiles)
            {
                yield return tile;
            }
        }

        public void OnSoundButtonClicked()
        {
            LogEx.Log("사운드 버튼 클릭됨");
        }
        public void OnRetryButtonClicked()
        {
            // Hide();
            LogEx.Log("재시작 버튼 클릭됨");
        }

        public void OnResumeButtonClicked()
        {
            // Hide();
            LogEx.Log("재개 버튼 클릭됨");
            GameManager.Instance.ResumeGame();
        }
        
        public void OnHomeButtonClicked()
        {
            // Hide();
            LogEx.Log("홈 버튼 클릭됨");
            GameManager.Instance.ResetGameAndReturnToMainMenu();
        }

        public void OnPauseButtonClicked()
        {
            // Hide();
            LogEx.Log("일시정지 버튼 클릭됨");
        }
    }
}