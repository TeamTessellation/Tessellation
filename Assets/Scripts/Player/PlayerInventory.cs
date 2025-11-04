using System;
using System.Collections.Generic;
using Abilities;
using Machamy.Utils;
using Stage;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private List<AbilityBase> _abilities = new List<AbilityBase>();
    private int _currentAbilityCount = 0;
    private int _maxAbilityCount;
    
    private TilePlaceHandler _tilePlaceHandler;
    
    private void Awake()
    {
        _maxAbilityCount = 5;
        
        for (int i = 0; i < _maxAbilityCount; i++)
        {
            _abilities.Add(null);
        }
    }

    private void Start()
    {
        _tilePlaceHandler = TurnManager.Instance.GetComponent<TilePlaceHandler>();
        if (_tilePlaceHandler == null)
        {
            LogEx.LogError("TilePlaceHandler에 연결 실패");
        }
    }

    private void OnDestroy()
    {
        if (_tilePlaceHandler == null)
        {
            LogEx.LogError("TilePlaceHandler에 연결 실패");
        }
    }

    public void AddAbility(eAbilityType abilityType)
    {
        if (_currentAbilityCount >= _maxAbilityCount)
        {
            LogEx.LogError("인벤토리가 다 찼음! 아이템 버리셈!");
            return;
        }

        AbilityBase newAbility = AbilityFactory.Create(abilityType);
        
        if (newAbility == null) return;
        
        _abilities[_currentAbilityCount] = newAbility;
        _currentAbilityCount++;
    }

    public void RemoveAbility(int slotIdx)
    {
        if (slotIdx < 0 || slotIdx >= _maxAbilityCount) return;

        for (int i = slotIdx; i < _currentAbilityCount - 1; i++)
        {
            _abilities[i] = _abilities[i + 1];
        }

        _abilities[_currentAbilityCount + 1] = null;
        _currentAbilityCount--;
    }

    public void SwapAbilities(int slotIdx1, int slotIdx2)
    {
        if (slotIdx1 < 0 || slotIdx1 >= _currentAbilityCount) return;
        if (slotIdx2 < 0 || slotIdx2 >= _currentAbilityCount) return;
        if (slotIdx1 == slotIdx2) return;

        (_abilities[slotIdx1], _abilities[slotIdx2]) = (_abilities[slotIdx2], _abilities[slotIdx1]);
    }
}
