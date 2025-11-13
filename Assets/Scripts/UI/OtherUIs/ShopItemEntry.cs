using Abilities;
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

        private ShopUI _shopUI;

        protected override void Start()
        {
            _shopUI = GetComponentInParent<ShopUI>();
        }

        public void InitializeData(AbilityDataSO abilityData)
        {
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
    }
}

