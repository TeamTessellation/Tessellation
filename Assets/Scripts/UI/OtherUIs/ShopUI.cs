using System;
using System.Collections.Generic;
using System.Threading;
using Abilities;
using Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Interaction;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

namespace UI.OtherUIs
{
    public class ShopUI : UIBase
    {
        [Header("Shop UI Components")] 
        [SerializeField] private CanvasGroup shopCanvasGroup;

        [SerializeField] private List<ShopItemEntry> entries = new List<ShopItemEntry>();
        
        [Header("Shop Settings")]
        [SerializeField] private int itemCount = 4;

        [Header("Visual Settings")] 
        public Color NormalRarityColor;
        public Color RareRarityColor;
        public Color EpicRarityColor;
        public Color SpecialRarityColor;

        [Header("Tween Settings")] 
        [Header("Show")] 
        [SerializeField] private float shopEntryMoveDuration = 0.5f;
        [SerializeField] private float shopEntryMoveDistance = 40f;
        [SerializeField] private float shopEntryMoveInterval = 0.2f;
        [SerializeField] private Ease shopEntryMoveEase = Ease.OutBack;
         
        [Header("Hide")]
        [SerializeField] private float inventoryMoveDuration = 0.4f;
        [SerializeField] private Ease inventoryMoveEase = Ease.InBack;
        [SerializeField] private float buttonShrinkDuration = 0.2f;
        [SerializeField] private Ease buttonShrinkEase = Ease.InBack;

        [Space(20)] 
        
        [Header("Buttons")] 
        [SerializeField] private Button _rerollButton;
        [SerializeField] private Button _skipButton;
        
        [SerializeField] private ShopItemSelector _shopItemSelector = new ShopItemSelector();
        [SerializeField] private List<Vector2> entryOriginPosition = new List<Vector2>();
        
        private bool _isSkipping = false;
        private bool _isAnimating = true;
        public bool IsAnimating => _isAnimating;
        
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private List<Tween> currentTweenList = new List<Tween>();
        
        protected override void Awake()
        {
            if (currentTweenList == null) currentTweenList = new List<Tween>();
            
            _rerollButton.onClick.AddListener(OnRerollButtonClicked);
            _skipButton.onClick.AddListener(OnSkipButtonClicked);
            gameObject.SetActive(false);

            for (int i = 0; i < entries.Count; i++)
            {
                entryOriginPosition.Add(entries[i].GetComponent<RectTransform>().anchoredPosition);
            }
        }

        public void Hide()
        {
            // 모든 활성 트윈 종료
            if (currentTweenList != null && currentTweenList.Count > 0)
            {
                foreach (var tween in currentTweenList)
                {
                    if (tween != null && tween.IsActive())
                    {
                        tween.Kill();
                    }
                }
                currentTweenList.Clear();
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
            
            // UI 요소 초기화
            if (shopCanvasGroup != null)
            {
                shopCanvasGroup.alpha = 0f;
            }
            
            // 엔트리 위치 초기화
            for (int i = 0; i < entries.Count && i < entryOriginPosition.Count; i++)
            {
                var rect = entries[i].GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = entryOriginPosition[i];
                }
            }
            
            // 플래그 초기화
            _isSkipping = false;
            _isAnimating = false;
            
            // GameObject 비활성화
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            InteractionManager.Instance.ConfirmEvent += OnConfirmed;
        }
        private void OnDisable()
        {
            InteractionManager.Instance.ConfirmEvent -= OnConfirmed;
        }

        public async UniTask ShowShopItemAsync(CancellationToken cancellationToken)
        {
            gameObject.SetActive(true);

            _isSkipping = false;
            currentTweenList.Clear();
            shopCanvasGroup.alpha = 1f;
            RefreshShopItems();
    
            await PlayEntryAnimation(cancellationToken);
        }

        private void RefreshShopItems()
        {
            if (_shopItemSelector != null)
            {
                List<AbilityDataSO> selectedItems = _shopItemSelector.SelectShopItems(itemCount);
                for (int i = 0; i < selectedItems.Count; i++)
                {
                    entries[i].InitializeData(selectedItems[i]);
                }
            }
        }

        private async UniTask PlayEntryAnimation(CancellationToken cancellationToken)
        {
            _isAnimating = true;
            currentTweenList.Clear();
    
            // Entries들을 보이지 않는 왼쪽으로 이동
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].GetComponent<RectTransform>().anchoredPosition = entryOriginPosition[i] + Vector2.left * shopEntryMoveDistance;
            }

            // 이동 애니메이션
            List<Tween> moveTweens = new List<Tween>();
            for (int i = 0; i < entries.Count; i++)
            {
                moveTweens.Add(entries[i].GetComponent<RectTransform>().DOAnchorPosX(entryOriginPosition[i].x, shopEntryMoveDuration)
                    .SetEase(shopEntryMoveEase)
                    .SetDelay(shopEntryMoveInterval * i));
            }
            
            currentTweenList.AddRange(moveTweens);
            await UniTask.WhenAll(moveTweens.ConvertAll(t => t.ToUniTask()));
            _isAnimating = false;
        }
        
        private void OnRerollButtonClicked()
        {
            OnConfirmed();
            
            RefreshShopItems();

            PlayEntryAnimation(_tokenSource.Token).Forget();
        }

        private void OnSkipButtonClicked()
        {
            _isSkipping = true;
        }

        /// <summary>
        /// 아무 키나 입력이 들어왔을 때 실행되는 로직
        /// </summary>
        private void OnConfirmed()
        {
            foreach (var tween in currentTweenList)
            {
                if(tween.IsActive() && tween.IsPlaying())
                    tween.Complete();
            }
            currentTweenList.Clear();
            _isAnimating = false;
        }

        public async UniTask WaitForSkipButtonAsync(CancellationToken cancellationToken)
        {
            await UniTask.NextFrame();

            while (!_isSkipping)
            {
                await UniTask.Yield(cancellationToken);
            }
            if (cancellationToken.IsCancellationRequested) return;
            if(isHiding) return;
            await HideShopUIAsync(cancellationToken);
        }

        bool isHiding = false;
        private async UniTask HideShopUIAsync(CancellationToken cancellationToken)
        {
            Debug.Log("HideSHop");
            UIManager UM = UIManager.Instance;
            if (isHiding) return;
            isHiding = true;
            List<Tween> moveTweens = new List<Tween>();
            // foreach (var entry in UM.InGameUI.ItemPlaceEntries)
            // {
            //     moveTweens.Add(entry.transform.DOMoveY(UM.InGameUI.IngameInventoryPosition.position.y, inventoryMoveDuration)
            //         .SetEase(inventoryMoveEase));
            // }
            var tween = UM.InGameUI.MoveInventoryYToIngamePosition(inventoryMoveDuration, inventoryMoveEase);
            moveTweens.Add(tween);
            
            currentTweenList.AddRange(moveTweens);

            await UniTask.WhenAll(moveTweens.Select(t => t.ToUniTask(
                cancellationToken: cancellationToken, tweenCancelBehaviour: TweenCancelBehaviour.Complete)));
        }
    }
}
