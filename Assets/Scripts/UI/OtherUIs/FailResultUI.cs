using System;
using System.Collections.Generic;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Machamy.Utils;
using Player;
using Stage;
using TMPro;
using UnityEngine;
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
        [SerializeField] private Button _skipButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _homeButton;
        
        CancellationTokenSource _tokenSource = new CancellationTokenSource();
        
        private void Awake()
        {
            _skipButton.onClick.AddListener(OnSkipButtonClicked);
            _retryButton.onClick.AddListener(OnRetryButtonClicked);
            _homeButton.onClick.AddListener(OnHomeButtonClicked);
            gameObject.SetActive(false);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
        }

        public async UniTask ShowFailResult()
        {
            gameObject.SetActive(true);
            LogEx.Log("Showing Fail Result UI");
            stageNameText.text = StageManager.Instance.CurrentStage.StageName;
            PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;
            

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
                

                
                await UniTask.WhenAll(moveTween.ToUniTask(TweenCancelBehaviour.Complete,token), fadeTween.ToUniTask(TweenCancelBehaviour.Complete,token));
                
                // 카운터 증가. 이동 및 페이드인 완료 후 시작. 다음 엔트리는 기다리지 않음.
                var countTween = entry.FailCountText.DoCount(0, dataValue, countDuration, true).SetEase(countEase).SetDelay(countAfetrMoveDelay);
                counterTweens.Add(countTween);
                
                await UniTask.Delay(System.TimeSpan.FromSeconds(nextMoveDelay), cancellationToken: token);
            }
            LogEx.Log("Waiting for all counters to complete");
            // 모든 카운터 트윈이 완료될 때까지 대기
            await UniTask.WhenAll(counterTweens.ConvertAll(t => t.ToUniTask(TweenCancelBehaviour.Complete,token)));
            await UniTask.Yield(); 
            
            
            LogEx.Log("Fail Result UI display complete");
        }
        
        
        private void OnSkipButtonClicked()
        {
            // 스킵 버튼 클릭 시 로직
            Debug.Log("Skip button clicked. Skipping level...");
            // TODO : 스킵 로직 구현
        }
        private void OnRetryButtonClicked()
        {
            Debug.Log("Retry button clicked. Restarting level...");
            GameManager.Instance.StartGame();
        }
        
        private void OnHomeButtonClicked()
        {
            Debug.Log("Home button clicked. Returning to main menu...");
            GameManager.Instance.ResetGameAndReturnToMainMenu();
        }
        
        
    }
}