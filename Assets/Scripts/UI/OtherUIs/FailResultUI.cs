using System;
using System.Collections.Generic;
using System.Threading;
using Ads;
using Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Interaction;
using Machamy.Utils;
using Player;
using SaveLoad;
using Sound;
using Stage;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace UI.OtherUIs
{
    public class FailResultUI : UIBase
    {
        [Header("Fail Result UI Components")]
        [SerializeField] private TMP_Text stageNameText;
        
        [SerializeField] private List<FailResultEntry> failResultEntries = new List<FailResultEntry>();
        
        [Header("Settings")] 
        [Header("이동")]
        [SerializeField] private float moveOffsetX = 1.5f;
        [SerializeField] private float moveDuration = 0.5f;
        [SerializeField] private Ease moveEase = Ease.OutBack;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private Ease fadeEase = Ease.Linear;
        [SerializeField] private float nextMoveDelay = 0.2f;
        [Header("카운터")]
        [SerializeField] private float countAfetrMoveDelay = 0.2f;
        [SerializeField] private float countDuration = 1.0f;
        [SerializeField] private Ease countEase = Ease.OutBack;
        
        
        
        [Space]
        [Header("Buttons")]
        [SerializeField] private CanvasGroup buttonCanvasGroup;
        [SerializeField] private AdsRewardButton _skipButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _homeButton;
        
        CancellationTokenSource _tokenSource = new CancellationTokenSource();
        
        private List<Tween> tweenList = new List<Tween>();
        
        private async UniTask Awake()
        {
            await GameManager.WaitForInit();
            
            _skipButton.Button.onClick.AddListener(OnSkipButtonClicked);
            _retryButton.onClick.AddListener(OnRetryButtonClicked);
            _homeButton.onClick.AddListener(OnHomeButtonClicked);
            gameObject.SetActive(false);
        }

        public void Hide()
        {
            // 모든 활성 트윈 종료
            if (tweenList != null && tweenList.Count > 0)
            {
                foreach (var tween in tweenList)
                {
                    if (tween != null && tween.IsActive())
                    {
                        tween.Kill();
                    }
                }
                tweenList.Clear();
            }
            
            // 이벤트 리스너 제거
            if (InteractionManager.HasInstance)
            {
                InteractionManager.Instance.ConfirmEvent -= OnConfirmed;
            }
            
            // 캔슬레이션 토큰 취소
            if (_tokenSource != null && !_tokenSource.IsCancellationRequested)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _tokenSource = new CancellationTokenSource();
            }
            
            // UI 요소 초기화 - 엔트리들을 원래 위치로 복구하고 알파값 초기화
            for (int i = 0; i < failResultEntries.Count; i++)
            {
                var entry = failResultEntries[i];
                var cg = entry.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 0f;
                }
                entry.FailCountText.CounterValue = 0;
                entry.gameObject.SetActive(false);
            }
            
            // GameObject 비활성화
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (InteractionManager.HasInstance)
            {
                InteractionManager.Instance.ConfirmEvent += OnConfirmed;
            }
        }
        
        private void OnDisable()
        {
            InteractionManager.Instance.ConfirmEvent -= OnConfirmed;
        }
        
        
        
        void OnConfirmed()
        {
            if (gameObject.activeSelf)
            {
                LogEx.Log("Fail Result UI confirmed by player.");
            }
            foreach (var tween in tweenList)
            {
                if (tween.IsActive() && tween.IsPlaying())
                    tween.Complete();
            }
            tweenList.Clear();
        }

        public async UniTask ShowFailResult()
        {
            gameObject.SetActive(true);
            buttonCanvasGroup.alpha = 0f;
            buttonCanvasGroup.gameObject.SetActive(false);
            LogEx.Log("Showing Fail Result UI");
            stageNameText.text = $"stage {StageManager.Instance.CurrentStage.StageName}";
            PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;

            
            SoundManager.Instance.PlaySfx(SoundReference.GameOver);


            
            
            _tokenSource.Cancel();
            _tokenSource = new CancellationTokenSource();
            
            Field.Instance.gameObject.SetActive(false);
            HandCanvas.Instance.gameObject.SetActive(false);
            
            CancellationToken token = _tokenSource.Token;
            var counterTweens = new List<Tweener>();
            
            for(int i = 0; i < failResultEntries.Count; i++)
            {
                failResultEntries[i].gameObject.SetActive(false);
            }
            // 인벤토리 올리고 버튼들 페이드인
            {
                buttonCanvasGroup.alpha = 0f;
                buttonCanvasGroup.gameObject.SetActive(true);
                var seq = DOTween.Sequence();
                seq.Append(UIManager.Instance.InGameUI.MoveInventoryYToShopPosition(moveDuration, moveEase));
                var fadeTween = buttonCanvasGroup.DOFade(1f, fadeDuration).SetEase(fadeEase);
                seq.Append(fadeTween);
                
            }
            
            
            
            // 각 엔트리 애니메이션 실행
            for (int i = 0; i < failResultEntries.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    LogEx.Log("Fail Result UI animation cancelled");
                    break;
                }
                LogEx.Log($"Animating Fail Result Entry {i}");
                var entry = failResultEntries[i];
                var variable = playerStatus[entry.VariableKey];
                int dataValue = 0;
                if (variable != null)
                {
                    dataValue = variable.IntValue;
                }
                else
                {
                    dataValue = 0;
                }
                entry.FailCountText.CounterValue = 0;
                var rect = entry.GetComponent<RectTransform>();
                var cg = entry.GetComponent<CanvasGroup>();
                entry.gameObject.SetActive(true);
                var initialPosition = rect.anchoredPosition;
                rect.anchoredPosition += new Vector2(-entry.MoveVector.x * moveOffsetX * rect.rect.width, 0);
                cg.alpha = 0f;  
                
                // 이동 및 페이드인
                
                var moveTween = rect.DOAnchorPos(initialPosition, moveDuration).SetEase(moveEase);
                var fadeTween = cg.DOFade(1f, fadeDuration).SetEase(fadeEase);
                
                var seq = DOTween.Sequence();
                seq.Append(moveTween);
                seq.Join(fadeTween);
                tweenList.Add(seq);
                
                await seq.ToUniTask(TweenCancelBehaviour.Complete, cancellationToken: token);
                
                // 카운터 증가. 이동 및 페이드인 완료 후 시작. 다음 엔트리는 기다리지 않음.
                var countTween = entry.FailCountText.DoCount(0, dataValue, countDuration, true).SetEase(countEase).SetDelay(countAfetrMoveDelay);
                counterTweens.Add(countTween);
                
                await UniTask.Delay(System.TimeSpan.FromSeconds(nextMoveDelay), cancellationToken: token);
            }
            LogEx.Log("Waiting for all counters to complete");
            // 모든 카운터 트윈이 완료될 때까지 대기
            tweenList.AddRange(counterTweens);
            await UniTask.WhenAll(counterTweens.ConvertAll(t => t.ToUniTask(TweenCancelBehaviour.Complete,token)));
            await UniTask.Yield(); 
            
            
            LogEx.Log("Fail Result UI display complete");
        }
        
        
        private void OnSkipButtonClicked()
        {
            Debug.Log("Skip button clicked. Attempting to show rewarded ad...");
            _skipButton.Button.interactable = false;
            SoundManager.Instance.PlaySfx(SoundReference.UIClick);
            _skipButton.OnAdCompleted_Once += () =>
            {
                Debug.Log("Ad completed successfully. Skipping fail result...");
                StageManager.Instance.SkipFail();
            };
            _skipButton.OnAdSkipped_Once += () =>
            {
                Debug.Log("Ad was skipped. Not skipping fail result.");
            };
            _skipButton.OnAdFailed_Once += () =>
            {
                Debug.Log("Ad failed to show. Not skipping fail result.");
            };
            _skipButton.ShowAd();
        }
        private void OnRetryButtonClicked()
        {
            Debug.Log("Retry button clicked. Restarting level...");
            GameManager.Instance.StartGame();
        }
        
        /// <summary>
        /// 홈 버튼 클릭 시 로직.
        /// 게임 상태를 초기화하고 메인 메뉴로 돌아갑니다.
        /// 세이브도 제거합니다.
        /// </summary>
        private void OnHomeButtonClicked()
        {
            Debug.Log("Home button clicked. Returning to main menu...");
            SaveLoadManager.Instance.RemoveSimpleSave();
            GameManager.Instance.ResetGameAndReturnToMainMenu();
            SoundManager.Instance.PlaySfx(SoundReference.UIClick);
            
        }
        
        
    }
}