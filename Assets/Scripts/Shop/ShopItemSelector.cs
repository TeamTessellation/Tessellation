
using System;
using System.Collections.Generic;
using System.Linq;
using Abilities;
using Core;
using Machamy.Utils;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

[Serializable]
public class ShopItemSelector
{
    [Serializable]
    public class RarityWeight
    {
        public eRarity Rarity;
        [Range(0, 100)] public float Weight;
    }

    [SerializeField] private List<RarityWeight> _rarityWeights = new List<RarityWeight>()
    {
        new RarityWeight() { Rarity = eRarity.Normal, Weight = 68f },
        new RarityWeight() { Rarity = eRarity.Rare, Weight = 22f },
        new RarityWeight() { Rarity = eRarity.Epic, Weight = 5f },
        new RarityWeight() { Rarity = eRarity.Special, Weight = 5f }
    };

    private List<AbilityDataSO> _abilities;

    private List<AbilityDataSO> Abilities
    {
        get
        {
            if (_abilities == null)
            {
                _abilities = Resources.LoadAll<AbilityDataSO>("AbilityData").ToList();
            }
            return _abilities;
        }
    }

    /// <summary>
    /// 상점에 표시할 아이템들을 반환
    /// </summary>
    /// <param name="itemCount">상점에 표시할 아이템의 개수</param>
    public List<AbilityDataSO> SelectShopItems(int itemCount = 4)
    {
        // 나타날 수 있는 어빌리티 목록 가져오기
        List<AbilityDataSO> availableAbilities = GetAvailableAbilities();
        
        // 등급별로 분류 (Dictionary<eRarity, List<AbilityDataSO>>)
        var abilitiesByRarity = availableAbilities
            .GroupBy(a => a.Rarity)
            .ToDictionary(g => g.Key, g => g.ToList());

        List<AbilityDataSO> selectedAbilities = new List<AbilityDataSO>();

        // 반복하며 확률계산 및 선택
        for(int i = 0; i < itemCount; i++)
        {
            eRarity selectedRarity = SelectRarityByWeight();
            
            // 해당 등급 아이템 있는지 확인
            if (abilitiesByRarity.TryGetValue(selectedRarity, out List<AbilityDataSO> abilities)
                && abilities.Count > 0)
            {
                int randIdx = UnityEngine.Random.Range(0, abilities.Count);
                AbilityDataSO selected = abilities[randIdx];
                
                selectedAbilities.Add(selected);
                Debug.Log($"뽑힌 아이템 : {selected.AbilityName}");
                abilities.RemoveAt(randIdx);
            }
            // 없다면 다른 등급 아이템 찾는다
            else
            {
                // TODO
                // 다시 뽑는 로직 만들어야하는데 지금은 귀찮다.
                Debug.Log($"{selectedRarity.ToString()}의 어떤 어빌리티를 뽑았다 치자.");
                selectedAbilities.Add(null);
            }
        }

        return selectedAbilities;
    }
    
    

    /// <summary>
    /// 현재 상태에서 상점에 나타날 수 있는 어빌리티들을 거른다
    /// </summary>
    private List<AbilityDataSO> GetAvailableAbilities()
    {
        PlayerInventory playerInventory = GameManager.Instance.PlayerStatus.inventory;
        
        if (playerInventory == null) return new List<AbilityDataSO>();
        List<AbilityDataSO> ownedAbilities = playerInventory.GetOwnedAbilities();
        List<AbilityDataSO> availableAbilities = new List<AbilityDataSO>();

        // 가진것보다 낮은 등급 어빌리티 거르기 위해 Dictionary 생성
        Dictionary<eAbilityType, eRarity> playerAbilityInfo = new Dictionary<eAbilityType, eRarity>();
        foreach (AbilityDataSO ability in ownedAbilities)
        {
            if (!playerAbilityInfo.ContainsKey(ability.AbilityType))
            {
                playerAbilityInfo[ability.AbilityType] = ability.Rarity;
            }
        }
        
        foreach (AbilityDataSO abilityData in Abilities)
        {
            // 상점에 나올 수 없는 아이템 제외
            if (!abilityData.CanAppearInShop) continue;
            
            // 충돌하는 어빌리티가 있다면 제외
            if (HasConflictingAbility(abilityData, ownedAbilities)) continue;
            
            // 같은 타임의 어빌리티 중 낮은건 거르기 (자동으로 같은 아이템은 걸러진다)
            if (playerAbilityInfo.TryGetValue(abilityData.AbilityType, out eRarity currentRarity))
            {
                if (abilityData.Rarity <= currentRarity) continue;
            }
            
            // 만약 합성 아이템이라면 하위 아이템 조건을 만족하는지 확인
            if (abilityData.IsSynthesisItem)
            {
                bool isSatisfied = true;
                foreach (var requireAbility in abilityData.SynthesisRequirements)
                {
                    if (!playerAbilityInfo.ContainsKey(requireAbility.AbilityType) || 
                        requireAbility.Rarity != playerAbilityInfo[requireAbility.AbilityType])
                    {
                        isSatisfied = false;
                        break;
                    }
                }
                if (!isSatisfied) continue;
            }
            
            availableAbilities.Add(abilityData);
        }

        return availableAbilities;
    }

    private bool HasConflictingAbility(AbilityDataSO abilityDataSo, List<AbilityDataSO> ownedAbilities)
    {
        if (abilityDataSo.ConflictingAbilities == null || abilityDataSo.ConflictingAbilities.Length == 0)
            return false;

        foreach (AbilityDataSO conflictAbility in abilityDataSo.ConflictingAbilities)
        {
            if (ownedAbilities.Contains(conflictAbility))
                return true;
        }

        return false;
    }

    private eRarity SelectRarityByWeight()
    {
        float totalWeight = _rarityWeights.Sum(w => w.Weight);
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (RarityWeight weight in _rarityWeights)
        {
            cumulativeWeight += weight.Weight;
            if (randomValue <= cumulativeWeight)
            {
                return weight.Rarity;
            }
        }

        return eRarity.Normal;
    }
}
