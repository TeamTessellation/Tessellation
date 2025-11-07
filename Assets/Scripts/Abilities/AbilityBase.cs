using System;
using Cysharp.Threading.Tasks;
using Machamy.Utils;
using Stage;
using UnityEngine;

namespace Abilities
{
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

    public abstract class AbilityBase
    {
        // === Properties ===
        public eAbilityType AbilityType;
        public int AbilityPriority;
        public int AbilityLevel = 1;
        
        protected virtual bool ReactsToTilePlaced => false;
        protected virtual bool ReactsToLineCleared => false;
        protected virtual bool ReactsToTileRemoved => false;
        protected virtual bool ReactsToTileBurst => false;
        protected virtual bool ReactsToTurnProcessed => false;

        // === Functions ===
        public virtual void Initialize(TilePlaceHandler tilePlaceHandler)
        {
            if (tilePlaceHandler == null)
            {
                LogEx.LogError("TilePlaceHandler에 연결 실패");
                return;
            }

            // TilePlaceHandler의 이벤트에 구독
            tilePlaceHandler.OnTilePlacedAsync += HandleTilePlacedAsync;
            tilePlaceHandler.OnLineClearedAsync += HandleLineClearedAsync;
            tilePlaceHandler.OnTileRemovedAsync += HandleTileRemovedAsync;
            tilePlaceHandler.OnTileBurstAsync += HandleTileBurstAsync;
            tilePlaceHandler.OnTurnProcessedAsync += HandleTurnProcessedAsync;
        }

        public void OnDestroy(TilePlaceHandler tilePlaceHandler)
        {
            if (tilePlaceHandler == null)
            {
                return;
            }
            
            // 이벤트 구독 해제
            tilePlaceHandler.OnTilePlacedAsync -= HandleTilePlacedAsync;
            tilePlaceHandler.OnLineClearedAsync -= HandleLineClearedAsync;
            tilePlaceHandler.OnTileRemovedAsync -= HandleTileRemovedAsync;
            tilePlaceHandler.OnTileBurstAsync -= HandleTileBurstAsync;
            tilePlaceHandler.OnTurnProcessedAsync -= HandleTurnProcessedAsync;
        }

        protected virtual async UniTask HandleTilePlacedAsync(TurnResultInfo info)
        {
            if (ReactsToTilePlaced) await TryActivate(info);
        }
        
        protected virtual async UniTask HandleLineClearedAsync(TurnResultInfo info)
        {
            if (ReactsToLineCleared) await TryActivate(info);
        }
        
        protected virtual async UniTask HandleTileRemovedAsync(TurnResultInfo info)
        {
            if (ReactsToTileRemoved) await TryActivate(info);
        }
        
        protected virtual async UniTask HandleTileBurstAsync(TurnResultInfo info)
        {
            if (ReactsToTileBurst) await TryActivate(info);
        }
        
        protected virtual async UniTask HandleTurnProcessedAsync(TurnResultInfo info)
        {
            if (ReactsToTurnProcessed) await TryActivate(info);
        }

        protected virtual async UniTask TryActivate(TurnResultInfo info)
        {
            if (CheckCanActivate(info))
            {
                await Activate(info);
            }
        }

        protected abstract int ModifyScore(eTileEventType tileEventType, Tile tile, int baseScore);
        protected abstract bool CheckCanActivate(TurnResultInfo info);
        protected abstract UniTask Activate(TurnResultInfo info);
    }
}
