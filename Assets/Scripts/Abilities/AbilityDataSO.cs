using System;
using UnityEngine;

namespace Abilities
{
    [Serializable]
    public enum eRarity
    {
        Normal,
        Special,
        Rare,
    }
    
    [Serializable]
    public enum eAbilityType
    {
        // === 기존 ===
        OnlyPlace,
        WomboCombo,
        LineClear,
    
        // === 액티브 아이템 (Active Items) ===
        RemoveTileSet,              // 타일셋 제거
        RedrawHand,                 // 타일셋 다시 뽑기
        Undo,                       // 되돌리기
        RotateTileSet,              // 타일셋 회전
        ConvertToOverwrite,         // 덮어쓰기 타일로 변환
        CopyTileSet,                // 타일셋 복사
    
        // === 폭탄 타일 보조 아이템 (Bomb Synergy) ===
        IncreaseBombRange,          // 폭발 범위 증가
        BombLinesClearAsLine,       // 폭발로 사라진 타일을 줄지우기로 간주
        BombExplodesOnLineClear,    // 지운 줄에 폭탄 포함시 터짐
        ChainBombExplosion,         // 폭탄 연쇄 폭발
    
        // === 체급 증가 (Resource Boost) ===
        IncreaseTurns,              // 턴수 증가
        IncreaseActiveItemUses,     // 액티브 아이템 횟수 증가
        IncreaseTimeLimit,          // 제한 시간 증가
    
        // === 필드 크기 변경 (Field Size Modifier) ===
        DecreaseFieldIncreaseScore, // 필드 크기 감소, 점수 증가
    
        // === 조건부 보상 (Conditional Rewards) ===
        // MoneyPerNLines,          // 줄 n번 지울때마다 돈 (아직 기획중)
    }
    
    [CreateAssetMenu(fileName = "AbilityData", menuName = "GameData/AbilityData")]
    public class AbilityDataSO : ScriptableObject
    {
        [Header("Basic Info")] 
        public eAbilityType AbilityType;
        public eRarity Rarity;
        public string AbilityName;
        [TextArea(3, 5)] public string Description;
        [Tooltip("Rarity == Normal만 레벨을 가진다")] public int MaxLevel;
        public Sprite ItemIcon;

        [Space(40)] 
    
        [Header("Shop Settings")] 
        public int AbilityPrice;
        public bool CanAppearInShop;

        [Space(40)] 
    
        [Header("Synthesis Settings")] 
        public bool IsSynthesisItem;
        public AbilityDataSO[] SynthesisRequirements;
    }
}

