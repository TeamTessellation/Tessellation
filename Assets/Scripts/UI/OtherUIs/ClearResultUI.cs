using System;
using System.Collections.Generic;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Interaction;
using Machamy.Utils;
using Player;
using TMPro;
using UI.Components;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace UI.OtherUIs
{
    public class ClearResultUI : UIBase
    {
        [Header("Clear Result UI Components")]
        [SerializeField] private CanvasGroup clearCanvasGroup;
        [SerializeField] private CanvasGroup entryHolderCanvasGroup;
        [SerializeField] private TMP_Text clearTitleText;
        [SerializeField] List<ClearResultEntry> entries = new List<ClearResultEntry>();
        
        [SerializeField] CanvasGroup totalHolderCanvasGroup;
        [SerializeField] ClearResultEntry totalEntry;
        
        [SerializeField] private RectTransform handTransformMoveTarget;
        [SerializeField] private CanvasGroup skipCanvasGroup;
        
        [SerializeField] Button skipButton;

        [Header("Animation Settings")] 
        [Header("이전 UI 없애기")]
        [SerializeField] private float fieldShrinkDuration = 0.2f;
        [SerializeField] private Ease fieldShrinkEase = Ease.InBack;
        [SerializeField] private float handShrinkDuration = 0.2f;
        [SerializeField] private Ease handShrinkEase = Ease.InBack;
        [SerializeField] private float inventoryShrinkDuration = 0.2f;
        [SerializeField] private Ease inventoryShrinkEase = Ease.InBack;
        [SerializeField] private float itemMoveUpDuration = 0.3f;
        [SerializeField] private Ease itemMoveUpEase = Ease.OutCubic;

        [Header("전체홀더 페이드 인 설정")] 
        [SerializeField] private float fadeInDuration = 0.4f;
        [SerializeField] private float moveUpDistance = 30f;
        [SerializeField] private float moveUpDuration = 0.4f;
        
        [Header("개별 항목 설정")]
        [SerializeField] private float entryDelay = 0.5f;
        [SerializeField] private float entryFadeInDuration = 0.5f;
        [SerializeField] private float totalFadeInDuration = 1.0f;
        [SerializeField] private float entryCountDuration = 1.0f;
        [Header("Total Holder 설정")]
        [SerializeField] private float totalHolderMoveUpDistance = 40f;
        [SerializeField] private float totalHolderMoveUpDuration = 0.5f;
        [SerializeField] private float totalHolderFadeInDuration = 0.5f;
        
        [Header("Total Entry 설정")]
        [SerializeField] private float totalEntryDelay = 0.5f;
        [SerializeField] private float totalEntryFadeInDuration = 1.0f;
        [SerializeField] private float totalEntryCountDuration = 1.5f;
        [Header("Skip 설정")] 
        [SerializeField] private float skipFadeOutDuration = 0.2f;
        
        private bool _isSkipping = false;

        protected override void Awake()
        {
            base.Awake();
            SetInitialState();
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            skipButton.onClick.AddListener(OnSkipButtonPressed);
        }
        private void OnDisable()
        {
            skipButton.onClick.RemoveListener(OnSkipButtonPressed);
        }

        private List<Tween> tweenList = new List<Tween>();
        private CancellationTokenSource _confirmationTokenSource = new CancellationTokenSource();
        // Confirm 버튼(또는 키) 눌림을 외부에서 알리기 위한 플래그
        private bool _confirmRequested;
        // ConfirmHandler 재진입 방지 플래그
        private bool _isHandlingConfirm = false;
        
        private void ConfirmHandler()
        {
            // 재진입 방지: 이미 처리중이면 무시
            if (_isHandlingConfirm) return;
            _isHandlingConfirm = true;
            try
            {
                if (tweenList.Count > 0)
                {
                    var snapshot = new List<Tween>(tweenList);
                    tweenList.Clear();
                    foreach (var tween in snapshot)
                    {
                        if (tween.IsActive() && tween.IsPlaying())
                        {
                            tween.Complete();
                        }
                    }
                }
                else
                {
                    _confirmRequested = true;
                }
            }
            finally
            {
                _isHandlingConfirm = false;
            }
        }
        
        public void SetInitialState()
        {
            foreach (var entry in entries)
            {
                entry.CanvasGroup.ignoreParentGroups = true;
                entry.CanvasGroup.alpha = 0f;
                entry.ValueText.CounterValue = 0;
                entry.ValueText.PaddingChar = ' ';
            }
            totalEntry.CanvasGroup.ignoreParentGroups = true;
            totalEntry.CanvasGroup.alpha = 0f;
            totalHolderCanvasGroup.ignoreParentGroups = true;
            totalHolderCanvasGroup.alpha = 0f;
        }

        private Vector2 _originalItemHolderPos;

        public async UniTask ShowClearResultsAsync(CancellationToken cancellationToken)
        {
            gameObject.SetActive(true);
            SetInitialState();
            
            _isSkipping = false;
            clearCanvasGroup.alpha = 0f;
            _confirmRequested = false;
            tweenList.Clear();
            /*
             * 0. 이전 UI 없애기, 이건 confirm 으로 스킵 불가
             */
            var UM = UIManager.Instance;
            Field field = Field.Instance;

            // 필드 축소
            List<Tween> shrinkTweens = new List<Tween>();
            foreach (var cell in field)
            {
                shrinkTweens.Add(DOTween.To(() => 1, x => cell.SetSize(x), 0f, fieldShrinkDuration).SetEase(fieldShrinkEase));
            }

            // 핸드 축소
            shrinkTweens.Add(HandCanvas.Instance.transform.DOScaleX(0f, handShrinkDuration)
                .SetEase(handShrinkEase));

            foreach (var itemPlaceEntry in UIManager.Instance.InGameUI.ItemPlaceEntries)
            {
                shrinkTweens.Add(itemPlaceEntry.transform.DOScaleX(0f, inventoryShrinkDuration)
                    .SetEase(inventoryShrinkEase));
            }
            
            tweenList.AddRange(shrinkTweens);
            await UniTask.WhenAll(shrinkTweens.ConvertAll(t => t.ToUniTask(cancellationToken: cancellationToken)));
            
            // todo : 아이템 위치 이동
            // _originalItemHolderPos = HandCanvas.Instance.GetComponent<RectTransform>().anchoredPosition;
            // var handMoveTween = HandCanvas.Instance.GetComponent<RectTransform>()
            //     .DOAnchorPos(handTransformMoveTarget.anchoredPosition, itemMoveUpDuration)
            //     .SetEase(itemMoveUpEase);
            // tweenList.Add(handMoveTween);
            // await handMoveTween.ToUniTask(cancellationToken: cancellationToken);
            // 클리어 타이틀 페이드 인
            var clearTitleFadeInTween = clearCanvasGroup.DOFade(1f, 0.5f);
            tweenList.Add(clearTitleFadeInTween);
            await clearTitleFadeInTween.ToUniTask(cancellationToken: cancellationToken);
            
            
            /*
             * 본격적인 애니메이션 시작
             */
            using var handle = UnityEngine.Pool.ListPool<Tween>.Get(out var countTweens);
            InteractionManager.Instance.ConfirmEvent += ConfirmHandler;
            PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;
            
            /*
             * 1. 전체 페이드 인 + 조금 위로 올라감
             */
            
            
            var fadeInTween = entryHolderCanvasGroup.DOFade(1f, fadeInDuration);
            var moveUpTween = entryHolderCanvasGroup.transform
                .DOLocalMoveY(entryHolderCanvasGroup.transform.localPosition.y + moveUpDistance, moveUpDuration)
                .SetEase(Ease.OutCubic);

            
            var sequence = DOTween.Sequence();
            sequence.Append(fadeInTween);
            sequence.Join(moveUpTween);
            await sequence.ToUniTask(cancellationToken: cancellationToken);
            tweenList.Add(sequence);
            
            /*
             * 2. 개별 항목 애니메이션
             */
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                entry.ValueText.CounterValue = 0;
                
                var entryFadeInTween = entry.CanvasGroup.DOFade(1f, entryFadeInDuration);
                await entryFadeInTween.ToUniTask(cancellationToken: cancellationToken);
                tweenList.Add(entryFadeInTween);
                
                int value = playerStatus[entry.VariableKey.ToString()]?.IntValue ?? 0;
                var countTween = entry.ValueText.DoCount(0, value, entryCountDuration, true).SetEase(Ease.OutCubic);
                countTweens.Add(countTween);
                
                // 대기: Delay 또는 Confirm 신호 중 먼저 발생한 쪽으로 넘어감
                var delayTask = UniTask.Delay(TimeSpan.FromSeconds(entryDelay), cancellationToken: cancellationToken);
                var confirmTask = UniTask.WaitUntil(() => _confirmRequested);
                await UniTask.WhenAny(delayTask, confirmTask);
                    
                if (_confirmRequested)
                {
                    _confirmRequested = false;
                }
                
            }

            /*
             * 3. Total Entry 애니메이션
             */
            await UniTask.Delay(TimeSpan.FromSeconds(totalEntryDelay), cancellationToken: cancellationToken);
            // Total Holder 애니메이션
            
            totalEntry.ValueText.CounterValue = 0;
            RectTransform totalHolderRect = totalHolderCanvasGroup.GetComponent<RectTransform>();
            Vector2 initialTotalHolderPos = totalHolderRect.anchoredPosition;
            totalHolderRect.anchoredPosition = new Vector2(initialTotalHolderPos.x, initialTotalHolderPos.y - totalHolderMoveUpDistance);
            var totalHolderMoveUpTween = totalHolderRect.DOAnchorPos(initialTotalHolderPos, totalHolderMoveUpDuration)
                .SetEase(Ease.OutCubic);
            var totalHolderFadeInTween = totalHolderCanvasGroup.DOFade(1f, totalHolderFadeInDuration);
            var totalHolderSequence = DOTween.Sequence();
            totalHolderSequence.Append(totalHolderMoveUpTween);
            totalHolderSequence.Join(totalHolderFadeInTween);
            tweenList.Add(totalHolderSequence);
            await totalHolderSequence.ToUniTask(cancellationToken: cancellationToken);
            // Total Entry 애니메이션
            
            var totalFadeInTween = totalEntry.CanvasGroup.DOFade(1f, totalEntryFadeInDuration);
            tweenList.Add(totalFadeInTween);

            await totalFadeInTween.ToUniTask(cancellationToken: cancellationToken);
            
            
            int totalCoins = playerStatus.StageCoinsObtained;
            var totalCountTween = totalEntry.ValueText.DoCount(0, totalCoins, totalEntryCountDuration, true).SetEase(Ease.OutCubic);
            countTweens.Add(totalCountTween);
            

            
            
            /*
             * 4. 마무리
             */
            
            // 모든 카운터 트윈이 완료될 때까지 대기
            tweenList.AddRange(countTweens);
            await UniTask.WhenAll(countTweens.ConvertAll(t => t.ToUniTask(cancellationToken: cancellationToken)));
            
            // 코인 카운터 업데이트
            CoinCounter coinCounter = UIManager.Instance.InGameUI.CoinCounter;
            var coinUpdateTween = coinCounter.DoCount(coinCounter.CounterValue, coinCounter.CounterValue + totalCoins, 1.0f)
                .SetEase(Ease.OutCubic);
            tweenList.Add(coinUpdateTween);
            await coinUpdateTween.ToUniTask(cancellationToken: cancellationToken);
            
            InteractionManager.Instance.ConfirmEvent -= ConfirmHandler;
        }
        


        public async UniTask WaitForSkipButtonAsync(CancellationToken cancellationToken)
        {
            await UniTask.NextFrame();
            // 대기 로직 구현
            while (!_isSkipping)
            {
                await UniTask.Yield(cancellationToken);
            }
        }
        
        
        public void OnSkipButtonPressed()
        {
            LogEx.Log("Skip button pressed.");
            // 스킵 버튼 눌렀을 때 처리 로직 구현
            _isSkipping = true;
        }
    }
}

