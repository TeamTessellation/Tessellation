using Abilities;
using Core;
using Cysharp.Threading.Tasks;
using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

namespace UI.OtherUIs
{
    public class ItemPlaceEntry : UIBehaviour
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private Image itemImage;
        
        private Button _button;
        
        private AbilityDataSO _abilityData;


        protected override void OnEnable()
        {
            ClearSlot();
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);
            base.OnEnable();
            
            // 인벤토리 콜백함수 등록
            PlayerInventory inventory = GameManager.Instance.PlayerStatus.inventory;
            inventory.OnInventorySlotChanged += OnInventorySlotChanged;
            inventory.RefreshPlaceEntry();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PlayerInventory inventory = GameManager.Instance.PlayerStatus.inventory;
            inventory.OnInventorySlotChanged -= OnInventorySlotChanged;
        }

        private void OnButtonClicked()
        {
            ItemPopupUI popupUI = UIManager.Instance.ItemPopupUI;
            if (_abilityData == null) return;
            
            if (popupUI != null)
            {
                popupUI.Initialize(_abilityData, false, null);
                popupUI.ShowPopUp().Forget();
            }
        }

        private void OnInventorySlotChanged(int slotIdx, AbilityBase abilityBase)
        {
            if (slotIndex != slotIdx) return;
            
            if (abilityBase == null)
            {
                ClearSlot();
                return;
            }
            
            // Debug.Log($"{slotIdx}번째 인덱스 수정, 이름 : {abilityBase.DataSO.ItemName}");

            _abilityData = abilityBase.DataSO;
            RefreshSlot();
        }

        private void RefreshSlot()
        {
            itemImage.enabled = true;
            // 데이터 참고해서 아이템 갱신
            if (itemImage != null)
            {
                itemImage.sprite = _abilityData.ItemIcon;
                // Debug.Log($"Item Sprite Update");
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