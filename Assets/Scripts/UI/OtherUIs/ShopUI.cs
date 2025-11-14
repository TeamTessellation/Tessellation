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
        [SerializeField] private List<Vector3> entryOriginPosition = new List<Vector3>();
        
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
                entryOriginPosition.Add(entries[i].transform.position);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
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
            
            RefreshShopItems();
            
            // 1. 스테이지 글씨 변경

            // Entries들을 위에서 순서대로 왼쪽에서 등장시킨다
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].transform.position = entryOriginPosition[i] + Vector3.left * shopEntryMoveDistance;
            }

            List<Tween> moveTweens = new List<Tween>();

            for (int i = 0; i < entries.Count; i++)
            {
                moveTweens.Add(entries[i].transform.DOMoveX(entryOriginPosition[i].x, shopEntryMoveDuration)
                    .SetEase(shopEntryMoveEase)
                    .SetDelay(shopEntryMoveInterval * i));
            }

            currentTweenList.AddRange(moveTweens);
            
            await UniTask.WhenAll(moveTweens.ConvertAll(t => t.ToUniTask(cancellationToken: cancellationToken)));

            _isAnimating = false;
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
        
        private void OnRerollButtonClicked()
        {
            RefreshShopItems();   
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
            
            await HideShopUIAsync(cancellationToken);
        }

        private async UniTask HideShopUIAsync(CancellationToken cancellationToken)
        {
            Debug.Log("HideSHop");
            UIManager UM = UIManager.Instance;
            List<Tween> moveTweens = new List<Tween>();
            foreach (var entry in UM.InGameUI.ItemPlaceEntries)
            {
                moveTweens.Add(entry.transform.DOMoveY(UM.InGameUI.IngameInventoryPosition.position.y, inventoryMoveDuration)
                    .SetEase(inventoryMoveEase));
            }
            
            currentTweenList.AddRange(moveTweens);

            await UniTask.WhenAll(moveTweens.Select(t => t.ToUniTask(
                cancellationToken: cancellationToken, tweenCancelBehaviour: TweenCancelBehaviour.Complete)));
        }
    }
}
