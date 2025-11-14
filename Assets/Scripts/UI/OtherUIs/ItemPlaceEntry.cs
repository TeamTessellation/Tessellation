using Abilities;
using Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.OtherUIs
{
    public class ItemPlaceEntry : UIBehaviour
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private Image itemImage;
        
        private AbilityDataSO _abilityData;
        protected override void OnEnable()
        {
            base.OnEnable();
            PlayerInventory inventory = GameManager.Instance.PlayerStatus.inventory;
            inventory.OnInventorySlotChanged += OnInventorySlotChanged;
            
            ClearSlot();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PlayerInventory inventory = GameManager.Instance.PlayerStatus.inventory;
            inventory.OnInventorySlotChanged -= OnInventorySlotChanged;
        }

        private void OnInventorySlotChanged(int slotIdx, AbilityBase abilityBase)
        {
            if (slotIndex != slotIdx) return;
            if (abilityBase == null)
            {
                ClearSlot();
                return;
            }

            _abilityData = abilityBase.dataSO;
            RefreshSlot();
        }

        private void RefreshSlot()
        {
            itemImage.enabled = true;
            // 데이터 참고해서 아이템 갱신
            if (itemImage != null)
            {
                itemImage.sprite = _abilityData.ItemIcon;
            }
        }

        private void ClearSlot()
        {
            _abilityData = null;
            itemImage.sprite = null;
            itemImage.enabled = false;
        }
    }
}