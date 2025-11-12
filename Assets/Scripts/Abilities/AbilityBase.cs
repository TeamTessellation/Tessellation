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
        
        protected virtual bool ReactsToTilePlaced => false;
        protected virtual bool ReactsToLineCleared => false;
        protected virtual bool ReactsToTileRemoved => false;
        protected virtual bool ReactsToTileBurst => false;
        protected virtual bool ReactsToTurnProcessed => false;

        // === Functions ===
        public void InitializeData(AbilityDataSO data)
        {
            dataSO = data;
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

        /// <summary>
        /// 아이템이 인벤토리에 추가될 때 불리는 함수
        /// </summary>
        protected virtual void OnAbilityApplied()
        {
            Debug.Log($"{dataSO.AbilityName} 생성됨");
        }

        /// <summary>
        /// 아이템이 인벤토리에서 제거될 때 불리는 함수0
        /// </summary>
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

        /// <summary>
        /// 여러 상황으로 인해 조커가 불리는데, 해당 요청이 조커를 활성화시킬 수 있는지 파악 첫단계
        /// </summary>
        protected virtual async UniTask TryActivate(TurnResultInfo info)
        {
            if (CheckCanActivate(info))
            {
                await Activate(info);
            }
        }

        /// <summary>
        /// 해당 조커가 타일 점수에 영향을 준다면 어떻게 줄 것인가? 를 판정
        /// </summary>
        protected virtual int ModifyScore(eTileEventType tileEventType, Tile tile, int baseScore)
        {
            return baseScore;
        }

        protected virtual bool CheckCanActivate(TurnResultInfo info)
        {
            return true;
        }
       
        
        protected virtual async UniTask Activate(TurnResultInfo info)
        {
            return;
        }
    }
}
