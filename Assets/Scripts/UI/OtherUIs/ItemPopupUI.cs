using System;
using System.Collections.Generic;
using System.Threading;
using Abilities;
using Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Player;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI.OtherUIs
{
    public class ItemPopupUI : UIBase
    {
        [Header("Item Popup UI Components")] 
        [SerializeField] private CanvasGroup backGroundCanvasGroup;
        [SerializeField] private GameObject itemPopUpFrame;
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI itemDescription;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI buySellText;
        [SerializeField] private Button buySellButton;
        [SerializeField] private Button backButton;

        [Header("Visual Settings")] 
        [SerializeField] private Color backGroundColor;
        
        [Header("Tween Settings")]
        [SerializeField] private float popupDuration = 0.5f;
        [SerializeField] private Ease popupEase = Ease.OutBack;

        private AbilityDataSO _abilityData;
        private bool _isForBuy;
        private bool _isAnimating = false;
        private CancellationTokenSource _tokenSource;
        private List<Tween> _currentTweens = new List<Tween>();

        private Action _onPurchaseCompleted;
        
        protected override void Awake()
        {
            base.Awake();
            
            buySellButton.onClick.AddListener(OnBuySellButtonClicked);
            backButton.onClick.AddListener(OnBackButtonClicked);
            
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _currentTweens.ForEach(t => t?.Kill());
            _currentTweens.Clear();
            
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;
        }

        public void Initialize(AbilityDataSO data, bool isForBuy, Action onPurchasCompleted)
        {
            _abilityData = data;
            _isForBuy = isForBuy;
            _onPurchaseCompleted = onPurchasCompleted;
            
            if (_abilityData == null) return;
    
            if (itemImage != null && _abilityData.ItemIcon != null)
            {
                itemImage.sprite = _abilityData.ItemIcon;
            }
    
            if (itemName != null)
            {
                itemName.text = _abilityData.ItemName;
            }
    
            if (itemDescription != null)
            {
                itemDescription.text = _abilityData.Description;
            }
    
            if (costText != null)
            {
                costText.text = _abilityData.ItemPrice.ToString();
            }
    
            if (buySellText != null)
            {
                buySellText.text = isForBuy ? "구매" : "판매";
            }
        }

        public async UniTask ShowPopUp()
        {
            if (_isAnimating) return;

            _isAnimating = true;
            gameObject.SetActive(true);
            
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();
            
            _currentTweens.ForEach(t => t?.Kill());
            _currentTweens.Clear();

            // 초기 세팅
            itemPopUpFrame.transform.localScale = Vector3.zero;
            backGroundCanvasGroup.alpha = 0f;

            List<Tween> popupTweens = new List<Tween>();
            popupTweens.Add(itemPopUpFrame.transform.DOScale(Vector3.one, popupDuration)
                .SetEase(popupEase));
            popupTweens.Add(backGroundCanvasGroup.DOFade(backGroundColor.a, popupDuration)
                .SetEase(Ease.Linear));
            _currentTweens.AddRange(popupTweens);
            
            await UniTask.WhenAll(popupTweens.ConvertAll(t => t.ToUniTask()));
            _isAnimating = false;
        }

        private void OnBuySellButtonClicked()
        {
            if (_isAnimating) return;
            if (_abilityData == null) return;

            if (_isForBuy)
            {
                BuyItem();
            }
            else
            {
                SellItem();
            }
        }

        private void BuyItem()
        {
            PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;

            // 구매 가능한지 체크
            bool canPurchase = playerStatus.CurrentCoins >= _abilityData.ItemPrice;
            if (canPurchase)
            {
                // 인벤토리에 아이템 추가를 시도해본다
                bool canAdd;
                string message;
                (canAdd, message) = playerStatus.inventory.AddAbility(_abilityData);
                if (canAdd)
                {
                    // 성공 시 돈 깎기
                    playerStatus.CurrentCoins -= _abilityData.ItemPrice;
                    _onPurchaseCompleted?.Invoke();
                    // Hide 하기
                    HidePopup().Forget();
                }
                else
                {
                    Debug.Log(message);
                }
            }
        }

        private void SellItem()
        {
            GameManager.Instance.PlayerStatus.inventory.RemoveAbilityByData(_abilityData);
            GameManager.Instance.PlayerStatus.CurrentCoins += _abilityData.ItemPrice;
        }

        private void OnBackButtonClicked()
        {
            HidePopup().Forget();
        }

        private async UniTask HidePopup()
        {
            if (_isAnimating) return;
            
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();
            
            _currentTweens.ForEach(t => t?.Kill());
            _currentTweens.Clear();

            List<Tween> popupTweens = new List<Tween>();
            popupTweens.Add(itemPopUpFrame.transform.DOScale(Vector3.zero, popupDuration)
                .SetEase(Ease.InBack));
            _currentTweens.AddRange(popupTweens);
            await UniTask.WhenAll(popupTweens.ConvertAll(t => t.ToUniTask()));
            _isAnimating = false;
            
            gameObject.SetActive(false);
        }
    }
}

