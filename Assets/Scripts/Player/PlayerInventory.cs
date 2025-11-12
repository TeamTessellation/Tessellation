using System;
using System.Collections.Generic;
using Abilities;
using Machamy.Utils;
using Stage;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // === Properties ===
    [Tooltip("테스팅용, 테스트 원하는 어빌리티를 순서대로 배치")]
    [SerializeField] private List<AbilityDataSO> TestCreateAbilites = new List<AbilityDataSO>();
    
    [Tooltip("현재 적용중인 어빌리티")]
    [SerializeReference] private List<AbilityBase> _abilities = new List<AbilityBase>();
    private int _currentAbilityCount = 0;
    private int _maxAbilityCount = 5;
    
    public InputManager.Item CurrentItem = InputManager.Item.None;
    public int CurrentItemCount = 5;
    public int MaxItemCount = 5;
    
    private TilePlaceHandler _tilePlaceHandler;
        
    // === Functions ===
    private void Awake()
    {
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
            Debug.LogError("TilePlaceHandler가 할당되지 않음! TurnManager 확인..");
        }

#if UNITY_EDITOR
        TestAddAbility();
#endif
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
    /// Will be deprecated
    /// </summary>
    private void TestAddAbility()
    {
        for (int i = 0; i < TestCreateAbilites.Count; i++)
        {
            AddAbility(TestCreateAbilites[i]);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _maxAbilityCount; i++)
        {
            if (_abilities[i] != null)
            {
                _abilities[i].Remove(_tilePlaceHandler);
            }
        }
    }

    public void AddAbility(AbilityDataSO abilityData)
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
            return;
        }
        
        // AbilityFactory 통해서 Ability 생성
        AbilityBase newAbility = AbilityFactory.Create(abilityData);
        if (newAbility == null) return;
        newAbility.Initialize(_tilePlaceHandler);
        
        // 맨 앞 빈곳에 생성한 어빌리티 추가
        for (int i = 0; i < _maxAbilityCount; i++)
        {
            if (_abilities[i] == null)
            {
                _abilities[i] = newAbility;
                _currentAbilityCount++;
                break;
            }
        }
        
        // 빈칸 없도록 어빌리티들을 앞으로 압축하기
        RefreshInventory();
    }

    public void RemoveAbilityByData(AbilityDataSO abilityData)
    {
        for (int i = 0; i < _maxAbilityCount; i++)
        {
            if (_abilities[i] == null) continue;

            if (_abilities[i].dataSO == abilityData)
            {
                RemoveAbility(_abilities[i]);
                _abilities[i] = null;
                _currentAbilityCount--;
            }
        }
    }

    /// <summary>
    /// Inventory 내 Abilities들을 앞으로 압축하고 정보 갱신한다
    /// </summary>
    private void RefreshInventory()
    {
        
        RefreshPriorities();
    }

    private void RemoveAbility(AbilityBase ability)
    {
        ability.Remove(_tilePlaceHandler);
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
        
        RefreshPriorities();
    }

    public void SwapAbilities(int slotIdx1, int slotIdx2)
    {
        if (slotIdx1 < 0 || slotIdx1 >= _currentAbilityCount) return;
        if (slotIdx2 < 0 || slotIdx2 >= _currentAbilityCount) return;
        if (slotIdx1 == slotIdx2) return;

        (_abilities[slotIdx1], _abilities[slotIdx2]) = (_abilities[slotIdx2], _abilities[slotIdx1]);
        
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
