using Abilities;
using Core;
using Player;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.OtherUIs
{
    public class ShopItemEntry : UIBehaviour
    { 
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text itemRarityText;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private Button thisButton;
        
        private AbilityDataSO _abilityData;
        private ShopUI _shopUI;

        protected override void Start()
        {
            base.Start();
            thisButton = GetComponent<Button>();
            if (thisButton)
            {
                thisButton.onClick.AddListener(OnEntryClicked);
            }
        }

        public void InitializeData(AbilityDataSO abilityData)
        {
            if (abilityData == null)
            {
                SetEntryEmpty();
                return;
            }
            
            ResetEntry();
            
            _abilityData = abilityData;
            
            if (_shopUI == null)
            {
                _shopUI = GetComponentInParent<ShopUI>();
            }
            
            if (itemImage != null)
            {
                itemImage.sprite = abilityData.ItemIcon;
                itemImage.SetNativeSize();
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
                itemNameText.text = abilityData.ItemName;
            }

            if (costText != null)
            {
                costText.text = abilityData.ItemPrice.ToString();
            }
        }

        private void OnEntryClicked()
        {
            if (_shopUI.IsAnimating)
            {
                return;
            }
            PurchaseItem();
        }
        
        private void PurchaseItem()
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
                    
                    // 버튼 비활성화
                    DisableButton();
                }
                else
                {
                    Debug.Log(message);
                }
            }
        }

        // 아이템이 null로 들어왔을 때..
        private void SetEntryEmpty()
        {
            DisableButton();
            itemImage.enabled = false;
            itemNameText.text = "NULL";
            costText.text = "0";
            itemRarityText.text = "NULL";
        }

        private void ResetEntry()
        {
            ResetButton();
            itemImage.enabled = true;
        }
        
        private void ResetButton()
        {
            if (thisButton == null)
            {
                thisButton = GetComponent<Button>();
            }
            thisButton.interactable = true;
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
        }
        private void DisableButton()
        {
            thisButton.interactable = false;
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0.5f;
        }
    }
}

