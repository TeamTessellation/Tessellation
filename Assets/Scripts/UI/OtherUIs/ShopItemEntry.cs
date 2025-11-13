using Abilities;
using Core;
using Player;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.OtherUIs
{
    public class ShopItemEntry : UIBehaviour
    { 
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text itemRarityText;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text costText;

        private AbilityDataSO _abilityData;
        private ShopUI _shopUI;
        private Button _thisButton;

        protected override void Start()
        {
            base.Start();
            _thisButton = GetComponent<Button>();
            if (_thisButton)
            {
                _thisButton.onClick.AddListener(PurchaseItem);
            }
        }

        public void InitializeData(AbilityDataSO abilityData)
        {
            ResetButton();
            
            _abilityData = abilityData;
            
            if (_shopUI == null)
            {
                _shopUI = GetComponentInParent<ShopUI>();
            }
                
            if(abilityData == null || _shopUI == null) return;
            
            if (itemImage != null)
            {
                itemImage.sprite = abilityData.ItemIcon;
            }

            if (itemRarityText != null)
            {
                switch(abilityData.Rarity)
                {
                    case eRarity.Normal:
                        itemRarityText.color = _shopUI.NormalRarityColor;
                        itemRarityText.text = "일반";
                        break;
                    case eRarity.Rare:
                        itemRarityText.color = _shopUI.RareRarityColor;
                        itemRarityText.text = "희귀";
                        break;
                    case eRarity.Epic:
                        itemRarityText.color = _shopUI.EpicRarityColor;
                        itemRarityText.text = "에픽";
                        break;
                    case eRarity.Special:
                        itemRarityText.color = _shopUI.SpecialRarityColor;
                        itemRarityText.text = "특수";
                        break;
                }
            }

            if (itemNameText != null)
            {
                itemNameText.text = abilityData.AbilityName;
            }

            if (costText != null)
            {
                costText.text = abilityData.AbilityPrice.ToString();
            }
        }

        private void PurchaseItem()
        {
            PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;

            // 구매 가능한지 체크
            bool canPurchase = playerStatus.CurrentCoins >= _abilityData.AbilityPrice;
            if (canPurchase)
            {
                // 인벤토리에 아이템 추가를 시도해본다
                bool canAdd = playerStatus.inventory.AddAbility(_abilityData);
                if (canAdd)
                {
                    // 성공 시 돈 깎기
                    playerStatus.CurrentCoins -= _abilityData.AbilityPrice;
                    
                    // 버튼 비활성화
                    DisableButton();
                }
            }
        }

        private void ResetButton()
        {
            if (_thisButton == null)
            {
                _thisButton = GetComponent<Button>();
            }
            _thisButton.interactable = true;
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
        }
        private void DisableButton()
        {
            _thisButton.interactable = false;
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0.5f;
        }
    }
}

