using System;
using Cysharp.Threading.Tasks;
using Machamy.Utils;
using Stage;
using Unity.VisualScripting;
using UnityEngine;

namespace Abilities
{
    [Serializable]
    public abstract class AbilityBase
    {
        // === Properties ===
        public AbilityDataSO dataSO;
        public int AbilityPriority;
        public int CurrentLevel = 1;
        
        protected virtual bool ReactsToTilePlaced => false;
        protected virtual bool ReactsToLineCleared => false;
        protected virtual bool ReactsToTileRemoved => false;
        protected virtual bool ReactsToTileBurst => false;
        protected virtual bool ReactsToTurnProcessed => false;

        // === Functions ===
        public void InitializeData(AbilityDataSO data)
        {
            dataSO = data;
            CurrentLevel = 1;
        }
        
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

            OnAbilityApplied();
        }

        public void Remove(TilePlaceHandler tilePlaceHandler)
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

            OnAbilityRemoved();
        }

        protected virtual void OnAbilityApplied()
        {
            Debug.Log($"{dataSO.AbilityName} 생성됨");
        }

        protected virtual void OnAbilityRemoved()
        {
            Debug.Log($"{dataSO.AbilityName} 삭제됨");
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
