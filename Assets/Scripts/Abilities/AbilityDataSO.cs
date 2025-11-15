using System;
using UnityEngine;
using UnityEngine.Serialization;

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
    public enum eItemType
    {
        ExtraTurn,
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

    public class AbilityDataGenerator
    {
#if UNITY_EDITOR
        private const string ABILITY_DATA_PATH = "Assets/Resources/AbilityData";

        public static void GenerateAbilityDataFromDataBase()
        {
            
        }
#endif
    }
    
    [CreateAssetMenu(fileName = "AbilityData", menuName = "GameData/AbilityData")]
    public class AbilityDataSO : ScriptableObject
    {
        [Header("Basic Info")] 
        public eItemType ItemType;
        public eRarity Rarity;
        public string ItemName;
        [TextArea(3, 5)] public string Description;
        public Sprite ItemIcon;
        
        [Space(40)] 
    
        [Header("Shop Settings")] 
        public int ItemPrice;
        public bool CanAppearInShop = true;
        [Tooltip("해당 어빌리티 중 하나라도 보유하고있으면 상점에 등장하지 않음")]
        public AbilityDataSO[] ConflictingItems;

        [Space(40)] 
    
        [Header("Synthesis Settings")] 
        public bool IsSynthesisItem;
        [Tooltip("해당 어빌리티들을 모두 가지고 있어야 상점에 등장함")]
        public AbilityDataSO[] SynthesisRequirements;
    }
}

