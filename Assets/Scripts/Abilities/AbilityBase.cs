using System;
using Cysharp.Threading.Tasks;
using Machamy.Utils;
using Stage;
using UnityEngine;

public enum eAbilityType
{
    OnlyPlace,
    WomboCombo,
    LineClear,
}

public abstract class AbilityBase
{
    // === Properties ===
    protected virtual bool ReactsToTilePlaced => false;
    protected virtual bool ReactsToLineCleared => false;
    protected virtual bool ReactsToTileRemoved => false;
    protected virtual bool ReactsToTileBurst => false;
    protected virtual bool ReactsToTurnProcessed => false;

    private TilePlaceHandler _tilePlaceHandler;
    
    // === Functions ===
    protected virtual void Start()
    {
        _tilePlaceHandler = TurnManager.Instance.GetComponent<TilePlaceHandler>();
        if (_tilePlaceHandler == null)
        {
            LogEx.LogError("TilePlaceHandler에 연결 실패");
            return;
        }
        
        // TilePlaceHandler의 이벤트에 구독
        _tilePlaceHandler.OnTilePlacedAsync += HandleTilePlacedAsync;
        _tilePlaceHandler.OnLineClearedAsync += HandleLineClearedAsync;
        _tilePlaceHandler.OnTileRemovedAsync += HandleTileRemovedAsync;
        _tilePlaceHandler.OnTileBurstAsync += HandleTileBurstAsync;
        _tilePlaceHandler.OnTurnProcessedAsync += HandleTurnProcessedAsync;
    }

    protected void OnDestroy()
    {
        if (_tilePlaceHandler == null)
        {
            LogEx.LogError("TilePlaceHandler에 연결 실패");
            return;
        }
        
        // 이벤트 구독 해제
        _tilePlaceHandler.OnTilePlacedAsync -= HandleTilePlacedAsync;
        _tilePlaceHandler.OnLineClearedAsync -= HandleLineClearedAsync;
        _tilePlaceHandler.OnTileRemovedAsync -= HandleTileRemovedAsync;
        _tilePlaceHandler.OnTileBurstAsync -= HandleTileBurstAsync;
        _tilePlaceHandler.OnTurnProcessedAsync -= HandleTurnProcessedAsync;
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

    protected abstract bool CheckCanActivate(TurnResultInfo info);
    protected abstract UniTask Activate(TurnResultInfo info);
}
