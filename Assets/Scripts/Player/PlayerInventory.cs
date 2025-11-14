using System;
using System.Collections.Generic;
using Abilities;
using Core;
using Cysharp.Threading.Tasks;
using Machamy.Utils;
using NUnit.Framework;
using Stage;
using UnityEngine;

[Serializable]
public class PlayerInventory
{
    
    /// <summary>
    /// 인벤토리 슬롯에 변화가 생겼을 때 호출된다
    /// int : 슬롯 번호 / AbilityBase : 바뀐 아이템 (삭제되었다면 null)
    /// </summary>
    public event Action<int, AbilityBase> OnInventorySlotChanged;
    
    [Tooltip("현재 적용중인 어빌리티")]
    [SerializeReference] private List<AbilityBase> _abilities = new List<AbilityBase>();
    
    [SerializeField] private int _currentAbilityCount = 0;
    [SerializeField] private int _maxAbilityCount = 5;
    
    public InputManager.Item CurrentItem = InputManager.Item.None;
    public int CurrentItemCount = 5;
    public int MaxItemCount = 5;
    
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

    private void SetAbility(int index, AbilityBase newAbility)
    {
        if (_abilities[index] != newAbility)
        {
            _abilities[index] = newAbility;
            OnInventorySlotChanged?.Invoke(index, _abilities[index]);
        }
    }
    
    public List<AbilityDataSO> GetOwnedAbilities()
    {
        List<AbilityDataSO> ownedAbilities = new List<AbilityDataSO>();
        for (int i = 0; i < _maxAbilityCount; i++)
        {
            if (_abilities[i] == null) continue;
            ownedAbilities.Add(_abilities[i].dataSO);
        }

        return ownedAbilities;
    }

    /// <summary>
    /// abilityData를 인자로 받아 인벤토리에 아이템을 추가합니다
    /// 성공할 시 true, 실패했을 시 false를 반환합니다.
    /// </summary>
    /// <param name="abilityData"></param>
    /// <returns></returns>
    public bool AddAbility(AbilityDataSO abilityData)
    {
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
            return false;
        }
        
        // AbilityFactory 통해서 Ability 생성
        AbilityBase newAbility = AbilityFactory.Create(abilityData);
        if (newAbility == null) return false;
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

        return true;
    }

    public void RemoveAbilityByData(AbilityDataSO abilityData)
    {
        for (int i = 0; i < _maxAbilityCount; i++)
        {
            if (_abilities[i] == null) continue;

            if (_abilities[i].dataSO == abilityData)
            {
                _abilities[i].Remove(Handler);
                SetAbility(i, null);
                _currentAbilityCount--;
                break;
            }
        }
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
    
}
