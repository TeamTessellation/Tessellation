using System;
using System.Collections.Generic;
using Abilities;
using Core;
using Cysharp.Threading.Tasks;
using Machamy.Utils;
using NUnit.Framework;
using Player;
using SaveLoad;
using Stage;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    [Serializable]
    public class PlayerInventory : ISaveTarget
    {
        
        /// <summary>
        /// 인벤토리 슬롯에 변화가 생겼을 때 호출된다
        /// int : 슬롯 번호 / AbilityBase : 바뀐 아이템 (삭제되었다면 null)
        /// </summary>
        public event Action<int, AbilityBase> OnInventorySlotChanged;
        
        [Tooltip("현재 적용중인 어빌리티")]
        [SerializeReference] private List<AbilityBase> _abilities = new List<AbilityBase>();
        
        [SerializeField] public int _currentAbilityCount = 0;
        [SerializeField] private int _maxAbilityCount = 5;

        public InputManager.eActiveItemType CurrentItem { get; private set; }
        public int MaxItemCount { get; private set; }
        public int currentItemCount;

        [NonSerialized] private TilePlaceHandler _tilePlaceHandler;

        private TilePlaceHandler Handler
        {
            get
            {
                if (_tilePlaceHandler == null)
                {
                    _tilePlaceHandler = TurnManager.Instance.GetComponent<TilePlaceHandler>();

                    if (_tilePlaceHandler == null)
                    {
                        Debug.LogError("TilePlaceHandler가 할당되지 않음! TurnManager 확인..");
                    }
                }
                return _tilePlaceHandler;
            }
        }
            
        // === Functions ===
        public PlayerInventory()
        {
            for (int i = 0; i < _maxAbilityCount; i++)
            {
                _abilities.Add(null);
            }
        }

        /// <summary>
        /// 현재 인벤토리에 할당된 아이템을 전부 삭제한다
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < _maxAbilityCount; i++)
            {
                RemoveAbilityByIndex(i);
            }
        }

        public void RefreshPlaceEntry()
        {
            for (int i = 0; i < 5; i++)
            {
                OnInventorySlotChanged?.Invoke(i, _abilities[i]);
            }
        }
        
        public void SetActiveItem(InputManager.eActiveItemType activeItemType, int itemAmount)
        {
            CurrentItem = activeItemType;
            MaxItemCount = itemAmount;
            currentItemCount = itemAmount;
        }

        public void ResetActiveItemCount()
        {
            currentItemCount = MaxItemCount;
        }
        
        /// <summary>
        /// 최대 아이템 사용 가능 횟수를 갱신한다 
        /// </summary>
        public void SetActiveItemCount(int newMaxItemCount)
        {
            MaxItemCount = newMaxItemCount;
            currentItemCount = MaxItemCount;
        }
        
        private void SetAbility(int index, AbilityBase newAbility)
        {
            if (_abilities[index] != newAbility)
            {
                _abilities[index] = newAbility;
                OnInventorySlotChanged?.Invoke(index, _abilities[index]);
            }
        }
        
        /// <summary>
        /// 현재 플레이어가 가지고 있는 Ability를 AbilityDataSO 배열 형식으로 리턴한다
        /// </summary>
        public List<AbilityDataSO> GetOwnedAbilities()
        {
            List<AbilityDataSO> ownedAbilities = new List<AbilityDataSO>();
            for (int i = 0; i < _maxAbilityCount; i++)
            {
                if (_abilities[i] == null) continue;
                ownedAbilities.Add(_abilities[i].DataSO);
            }

            return ownedAbilities;
        }
        
        /// <summary>
        /// 현재 보유 아이템 정보를 JSON 문자열로 반환합니다.
        /// </summary>
        public string GetItemsAsJson()
        {
            List<string> itemIds = new List<string>();
            for (int i = 0; i < _maxAbilityCount; i++)
            {
                if (_abilities[i] != null)
                {
                    itemIds.Add(_abilities[i].DataSO.ItemID);
                }
            }
            return JsonUtility.ToJson(new ItemListWrapper { items = itemIds });
        }
        
        [Serializable]
        private class ItemListWrapper
        {
            public List<string> items;
        }

        /// <summary>
        /// abilityData를 인자로 받아 인벤토리에 아이템을 추가합니다
        /// 성공할 시 true, 실패했을 시 false를 반환합니다.
        /// </summary>
        public (bool success, string message) AddAbility(AbilityDataSO abilityData)
        {
            // 해당하는 아이템과 충돌하는 아이템이 있는지 확인한다
            if (abilityData.ConflictingItems != null && abilityData.ConflictingItems.Length > 0)
            {
                foreach (AbilityDataSO conflictItem in abilityData.ConflictingItems)
                {
                    if (_abilities == null)
                    {
                        continue;
                    }
                    foreach (var ownedAbility in _abilities)
                    {
                        if (ownedAbility != null && ownedAbility.DataSO.ItemID == conflictItem.ItemID)
                        {
                            return (false, $"{conflictItem.ItemName}, {abilityData.ItemName} 이 둘은\n동시에 보유할 수 없습니다!");
                        }
                    }
                }
            }
            
            // abilityData SynthesisRequirements에 해당하는 어빌리티들을 제거하기
            if (abilityData.SynthesisRequirements != null)
            {
                for (int i = 0; i < abilityData.SynthesisRequirements.Length; i++)
                {
                    RemoveAbilityByData(abilityData.SynthesisRequirements[i]);
                }
            }
            
            // 인벤토리 크기 체킹
            if (_currentAbilityCount >= _maxAbilityCount)
            {
                Debug.Log("인벤토리가 Max입니다");
                return (false, "인벤토리가 가득 찼습니다!\n아이템을 판매해주세요");
            }
            
            // AbilityFactory 통해서 Ability 생성
            AbilityBase newAbility = AbilityFactory.Create(abilityData);
            if (newAbility == null) return (false, $"AbilityFactory에 해당하는 Ability가 등록되지 않았습니다!\nItemName : {abilityData.ItemName}}}");
            newAbility.Initialize(Handler);
            
            // 맨 앞 빈곳에 생성한 어빌리티 추가
            for (int i = 0; i < _maxAbilityCount; i++)
            {
                if (_abilities[i] == null)
                {
                    SetAbility(i, newAbility);
                    _currentAbilityCount++;
                    break;
                }
            }
            
            // 빈칸 없도록 어빌리티들을 앞으로 압축하기
            RefreshInventory();

            return (true, "Success");
        }

        public void RemoveAbilityByData(AbilityDataSO abilityData)
        {
            for (int i = 0; i < _maxAbilityCount; i++)
            {
                if (_abilities[i] == null) continue;

                if (_abilities[i].DataSO == abilityData)
                {
                    _abilities[i].Remove(Handler);
                    SetAbility(i, null);
                    _currentAbilityCount--;
                    break;
                }
            }
            
            RefreshInventory();
        }
        
        public void RemoveAbilityByIndex(int slotIdx)
        {
            if (_abilities[slotIdx] == null) return;
            if (slotIdx < 0 || slotIdx >= _maxAbilityCount) return;

            _abilities[slotIdx].Remove(Handler);
            SetAbility(slotIdx, null);
            _currentAbilityCount--;
            
            RefreshInventory();
        }

        /// <summary>
        /// Inventory 내 Abilities들을 앞으로 압축하고 정보 갱신한다
        /// </summary>
        private void RefreshInventory()
        {
            // 앞으로 압축
            List<AbilityBase> tmp = new List<AbilityBase>();
            for (int i = 0; i < _maxAbilityCount; i++)
            {
                if (_abilities[i] != null)
                {
                    tmp.Add(_abilities[i]);
                }
            }

            // 정보 갱신
            for (int i = 0; i < _maxAbilityCount; i++)
            {
                if (i < tmp.Count)
                {
                    SetAbility(i, tmp[i]);
                }
                else
                {
                    SetAbility(i, null);
                }
            }

            _currentAbilityCount = tmp.Count;
            
            RefreshPriorities();
        }

        public void SwapAbilities(int slotIdx1, int slotIdx2)
        {
            if (slotIdx1 < 0 || slotIdx1 >= _maxAbilityCount) return;
            if (slotIdx2 < 0 || slotIdx2 >= _maxAbilityCount) return;
            if (slotIdx1 == slotIdx2) return;

            AbilityBase temp = _abilities[slotIdx1];
            SetAbility(slotIdx1, _abilities[slotIdx2]);
            SetAbility(slotIdx2, temp);
            
            RefreshPriorities();
        }

        private void RefreshPriorities()
        {
            for (int i = 0; i < _maxAbilityCount; i++)
            {
                if (_abilities[i] != null)
                {
                    _abilities[i].AbilityPriority = i;
                }
            }
        }

        public Guid Guid { get; init; }
        
        public void LoadData(GameData data)
        {
            if (_abilities == null)
            {
                _abilities = new List<AbilityBase>(_maxAbilityCount);
            }

            for (int i = 0; i < _maxAbilityCount; i++)
            {
                _abilities[i] = null;
            }
            
            if (data.InventoryIds != null && data.InventoryIds.Count > 0)
            {
                for (int i = 0; i < data.InventoryIds.Count; i++)
                {
                    string itemId = data.InventoryIds[i];

                    Debug.Log(itemId);
                    AbilityDataSO dataSo = Resources.Load<AbilityDataSO>($"Abilities/AbilityDataSO/{itemId}");

                    AddAbility(dataSo);
                }
            }
            
            RefreshInventory();
        }

        public void SaveData(ref GameData data)
        {
            data.InventoryIds = new List<string>();
            for (int i = 0; i < _abilities.Count; i++)
            {
                if(_abilities[i] != null)
                    data.InventoryIds.Add(_abilities[i].DataSO.ItemID);
            }
        }
    }
}
