using System;
using UnityEngine;

namespace Abilities
{
    [Serializable]
    public enum eRarity
    {
        Normal,
        Rare,
        Epic,
        Special,
    }
    
    [Serializable]
    public enum eAbilityType
    {
        ExtraLife,
        ExtraActiveItemRemainingUses,
        AddBombTileset,
        IncreaseExplosionRange,
        BombImmediatelyExplosion,
        ChainExplosion,
        AddExtraScoreTileset,
        AddMultipleTileset,
        AddGoldTileset,
        GoldTilesetCoinScaledExtraScore,
        AdditionalInterest,
        CoinScaledMultiple,
    }
    
    [CreateAssetMenu(fileName = "AbilityData", menuName = "GameData/AbilityData")]
    public class AbilityDataSO : ScriptableObject
    {
        [Header("Basic Info")] 
        public eAbilityType AbilityType;
        public eRarity Rarity;
        public string AbilityName;
        [TextArea(3, 5)] public string Description;
        public Sprite ItemIcon;

        [Space(40)] 
    
        [Header("Shop Settings")] 
        public int AbilityPrice;
        public bool CanAppearInShop;
        [Tooltip("해당 어빌리티 중 하나라도 보유하고있으면 상점에 등장하지 않음")]
        public AbilityDataSO[] ConflictingAbilities;

        [Space(40)] 
    
        [Header("Synthesis Settings")] 
        public bool IsSynthesisItem;
        [Tooltip("해당 어빌리티들을 모두 가지고 있어야 상점에 등장함")]
        public AbilityDataSO[] SynthesisRequirements;
    }
}

