using System;
using System.Collections.Generic;
using System.Threading;
using Abilities;
using Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Interaction;
using NUnit.Framework;
using Sound;
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
        [SerializeField] private TextMeshProUGUI rerollCostText;

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
        [SerializeField] private float shopEntrySoundDelayRatio = 0.6f;
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
        [SerializeField] private float rerollCooldown = 1.2f;
        private int _rerollCount = 0;
        private bool _isRerollOnCooldown = false;
        
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
            _rerollCount = 0;
            rerollCostText.text = (3 + _rerollCount * 2).ToString();
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
                // 각 아이템이 서로 불가 아이템인지 확인

                bool IsPossible(List<AbilityDataSO> candidates)
                {
                    for (int i = 0; i < candidates.Count; i++)
                    {
                        var itemA = candidates[i];
                        for (int j = i + 1; j < candidates.Count; j++)
                        {
                            var itemB = candidates[j];
                            // itemA의 ConflictingItems에 itemB가 있는지 확인
                            foreach (var conflictingItem in itemA.ConflictingItems)
                            {
                                if (conflictingItem == itemB)
                                {
                                    return false;
                                }
                            }
                            // itemB의 ConflictingItems에 itemA가 있는지 확인
                            foreach (var conflictingItem in itemB.ConflictingItems)
                            {
                                if (conflictingItem == itemA)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    return true;
                }
                
                while (!IsPossible(selectedItems))
                {
                    selectedItems = _shopItemSelector.SelectShopItems(itemCount);
                }
                
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
            eRarity mostRareRarity = eRarity.None;
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e.AbilityData != null && e.AbilityData.Rarity > mostRareRarity)
                {
                    mostRareRarity = e.AbilityData.Rarity;
                }
                e.GetComponent<RectTransform>().anchoredPosition = entryOriginPosition[i] + Vector2.left * shopEntryMoveDistance;
            }

            // 이동 애니메이션
            List<Tween> moveTweens = new List<Tween>();
            
            bool hasPlayedRaritySound = false;
            void PlayRaritySound(eRarity rarity)
            {
                if (hasPlayedRaritySound) return;
                hasPlayedRaritySound = true;
                switch (rarity)
                {
                    case eRarity.Rare:
                        SoundManager.Instance.PlaySfx(SoundReference.AppearRare);
                        
                        break;
                    case eRarity.Epic:
                        SoundManager.Instance.PlaySfx(SoundReference.AppearEpic);
                        break;
                    case eRarity.Special:
                        SoundManager.Instance.PlaySfx(SoundReference.AppearSpecial);
                        break;
                    case eRarity.None:
                    case eRarity.Normal:
                        break;
                            
                }
            }
            
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                // 해당 트윈이 가장 높은 등급이고, 아직 소리가 재생되지 않았다면, 적당한 지점에서 소리 재생

                Tween moveTween = e.GetComponent<RectTransform>()
                    .DOAnchorPosX(entryOriginPosition[i].x, shopEntryMoveDuration)
                    .SetEase(shopEntryMoveEase)
                    .SetDelay(shopEntryMoveInterval * i);
                moveTween.OnUpdate(() =>
                {
                    float progress = moveTween.ElapsedPercentage();
                    if (progress >= shopEntrySoundDelayRatio)
                    {
                        if (e.AbilityData != null && e.AbilityData.Rarity == mostRareRarity)
                        {
                            PlayRaritySound(e.AbilityData.Rarity);
                        }
                    }
                });
                moveTweens.Add(moveTween);
                
                
                
                
            }
            
            currentTweenList.AddRange(moveTweens);
            await UniTask.WhenAll(moveTweens.ConvertAll(t => t.ToUniTask()));
            _isAnimating = false;
        }
        
        private void OnRerollButtonClicked()
        {
            // 쿨다운 체크
            if (_isRerollOnCooldown) return;
            
            // 재화 체크
            int currentRerollCost = 3 + _rerollCount * 2;
            if (GameManager.Instance.PlayerStatus.CurrentCoins >= currentRerollCost)
            {
                // 재화 소모 (CurrentCoins)
                GameManager.Instance.PlayerStatus.CurrentCoins -= currentRerollCost;
                SoundManager.Instance.PlaySfx(SoundReference.Reroll);
                _rerollCount++;
                rerollCostText.text = (3 + _rerollCount * 2).ToString();
            }
            else return;
            
            OnConfirmed();
            
            RefreshShopItems();

            PlayEntryAnimation(_tokenSource.Token).Forget();
            StartRerollCooldown(_tokenSource.Token).Forget();
        }

        private async UniTask StartRerollCooldown(CancellationToken token)
        {
            _isRerollOnCooldown = true;
            await UniTask.Delay(TimeSpan.FromSeconds(rerollCooldown), cancellationToken: token);
            _isRerollOnCooldown = false;
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
            if (_isSkipping == true) return;
            
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
