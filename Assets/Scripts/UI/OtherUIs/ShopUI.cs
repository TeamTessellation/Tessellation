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
        // TODO..

        [Space(20)] 
        
        [Header("Buttons")] 
        [SerializeField] private Button _rerollButton;
        [SerializeField] private Button _skipButton;
        
        [SerializeField] private ShopItemSelector _shopItemSelector = new ShopItemSelector();

        private bool _isSkipping = false;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private List<Tween> currentTweenList = new List<Tween>();

        protected override void Awake()
        {
            if (currentTweenList == null) currentTweenList = new List<Tween>();
            
            _rerollButton.onClick.AddListener(OnRerollButtonClicked);
            _skipButton.onClick.AddListener(OnSkipButtonClicked);
            gameObject.SetActive(false);
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
        }

        public async UniTask WaitForSkipButtonAsync(CancellationToken cancellationToken)
        {
            await UniTask.NextFrame();

            while (!_isSkipping)
            {
                await UniTask.Yield(cancellationToken);
            }
        }
    }
}
