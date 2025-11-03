using System;
using Cysharp.Threading.Tasks;
using Machamy.Utils;
using Stage;
using UnityEngine;

public class AbilityBase : MonoBehaviour
{
    protected virtual void Start()
    {
        TilePlaceHandler tilePlaceHandler = TurnManager.Instance.GetComponent<TilePlaceHandler>();
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

    protected virtual async UniTask HandleTilePlacedAsync(TurnResultInfo info)
    {
    }
    
    protected virtual async UniTask HandleLineClearedAsync(TurnResultInfo info)
    {
    }
    
    protected virtual async UniTask HandleTileRemovedAsync(TurnResultInfo info)
    {
    }
    
    protected virtual async UniTask HandleTileBurstAsync(TurnResultInfo info)
    {
    }
    
    protected virtual async UniTask HandleTurnProcessedAsync(TurnResultInfo info)
    {
    }

    protected virtual async UniTask Activate()
    {
    }

    protected virtual bool CheckCanActivate(TurnResultInfo info)
    {
        return true;
    }
}
