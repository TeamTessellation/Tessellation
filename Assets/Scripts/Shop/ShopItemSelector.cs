
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using Abilities;
using Core;
using Machamy.Utils;
using Player;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;
using ResourceManager = Resource.ResourceManager;

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
                if (ResourceManager.HasInstance)
                {
                    _abilities = ResourceManager.Instance.GetAllResourcesByLabel<AbilityDataSO>("AbilitySO").ToList();
                }
               
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
            AbilityDataSO selectedAbility = SelectRandomAbility(abilitiesByRarity);

            selectedAbilities.Add(selectedAbility);
        }

        return selectedAbilities;
    }

    private AbilityDataSO SelectRandomAbility(Dictionary<eRarity, List<AbilityDataSO>> abilitiesByRarity)
    {
        HashSet<eRarity> excludedRarities = new HashSet<eRarity>();

        while (true)
        {
            eRarity selectedRarity = SelectRarityByWeight(excludedRarities);
            
            // 모든 등급이 제외되었다면..
            if (selectedRarity == eRarity.None)
            {
                return null;
            }

            if (abilitiesByRarity.TryGetValue(selectedRarity, out List<AbilityDataSO> abilities) && abilities.Count > 0)
            {
                int randIdx = UnityEngine.Random.Range(0, abilities.Count);
                AbilityDataSO selected = abilities[randIdx];
                abilities.RemoveAt(randIdx);
                return selected;
            }
            
            // 가져올 아이템이 없다면 제외 리스트에 추가, while문 다시 돈다
            excludedRarities.Add(selectedRarity);
        }
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
        Dictionary<eItemType, eRarity> playerAbilityInfo = new Dictionary<eItemType, eRarity>();
        foreach (AbilityDataSO ability in ownedAbilities)
        {
            if (!playerAbilityInfo.ContainsKey(ability.ItemType))
            {
                playerAbilityInfo[ability.ItemType] = ability.Rarity;
            }
        }
        
        foreach (AbilityDataSO abilityData in Abilities)
        {
            // 상점에 나올 수 없는 아이템 제외
            if (!abilityData.CanAppearInShop) continue;
            
            // 충돌하는 어빌리티가 있다면 제외
            if (HasConflictingAbility(abilityData, ownedAbilities)) continue;
            
            // 같은 타임의 어빌리티 중 낮은건 거르기 (자동으로 같은 아이템은 걸러진다)
            if (playerAbilityInfo.TryGetValue(abilityData.ItemType, out eRarity currentRarity))
            {
                if (abilityData.Rarity <= currentRarity) continue;
            }
            
            // 만약 합성 아이템이라면 하위 아이템 조건을 만족하는지 확인
            if (abilityData.IsSynthesisItem)
            {
                bool isSatisfied = true;
                foreach (var requireAbility in abilityData.SynthesisRequirements)
                {
                    if (!playerAbilityInfo.ContainsKey(requireAbility.ItemType) || 
                        requireAbility.Rarity != playerAbilityInfo[requireAbility.ItemType])
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
        if (abilityDataSo.ConflictingItems == null || abilityDataSo.ConflictingItems.Length == 0)
            return false;

        foreach (AbilityDataSO conflictAbility in abilityDataSo.ConflictingItems)
        {
            if (ownedAbilities.Contains(conflictAbility))
                return true;
        }

        return false;
    }

    private eRarity SelectRarityByWeight(HashSet<eRarity> excludedRarities)
    {
        List<RarityWeight> validWeights = new List<RarityWeight>();

        foreach (var weight in _rarityWeights)
        {
            if (!excludedRarities.Contains(weight.Rarity))
            {
                validWeights.Add(weight);
            }
        }

        // 모든 등급이 제외되었다면 특수 아이템 표시
        if (validWeights.Count == 0)
        {
            return eRarity.None;
        }
        
        float totalWeight = validWeights.Sum(w => w.Weight);
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (RarityWeight weight in validWeights)
        {
            cumulativeWeight += weight.Weight;
            if (randomValue <= cumulativeWeight)
            {
                return weight.Rarity;
            }
        }

        return validWeights[0].Rarity;
    }
}
