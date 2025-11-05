using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Machamy.Utils;
using Player;
using ExecEvents;
using Stage;
using Unity.VisualScripting;
using UnityEngine;

// 전체 턴 결과를 담는 데이터 Info
// 아직 어떻게 활용할지.. 구조 미정
public class TurnResultInfo : ExecEventArgs<TurnResultInfo>
{
    public readonly List<Tile> PlacedTiles; // 이번턴에 배치한 타일에 대한 정보
    public readonly List<Tile> RemovedTiles; // 이번턴에 삭제될 타일에 대한 정보
    public readonly List<Tile> BurstTiles; // 폭발되어 사라질 타일에 대한 정보 
    public readonly List<Tile> ClearedTiles; // 완성되어 사라질 타일에 대한 정보
    public int ClearedLineCount; // 이번턴에 완성된 줄의 수

    public TurnResultInfo()
    {
        PlacedTiles = new List<Tile>();
        RemovedTiles = new List<Tile>();
        BurstTiles = new List<Tile>();
        ClearedTiles = new List<Tile>();
        
        ClearedLineCount = 0;
    }
}

public enum eTileEventType
{
    Place,
    Remove,
    Burst,
    LineClear,
}

// 타일에 대한 모든 이벤트를 TileEvent 클래스로 정의
public class TileEvent
{
    public readonly List<Tile> Tiles;
    public eTileEventType TileEventType;
    
    public TileEvent(List<Tile> newTiles)
    {
        if (newTiles != null)
        {
            Tiles = newTiles;
        }
        else Tiles = new List<Tile>();
    }
}

public class TilePlaceEvent : TileEvent
{ 
    public TilePlaceEvent(List<Tile> newTiles) : base(newTiles)
    {
        TileEventType = eTileEventType.Place;
    }
}

public class TileRemoveEvent : TileEvent
{
    public TileRemoveEvent(List<Tile> newTiles) : base(newTiles)
    {
        TileEventType = eTileEventType.Remove;
    }
}

public class LineClearEvent : TileEvent
{
    public readonly List<Field.Line> ClearedLine;
    public readonly int ClearedLineCount;
    public LineClearEvent(List<Field.Line> removedLine, List<Tile> removedTile) : base(removedTile)
    {
        TileEventType = eTileEventType.LineClear;
        ClearedLine = removedLine;
        ClearedLineCount = removedLine.Count;
    }
}

public class TileBurstEvent : TileEvent
{
    public TileBurstEvent(List<Tile> newTiles) : base(newTiles)
    {
        TileEventType = eTileEventType.Burst;
    }
}


// 플레이어 입력 후처리 해주는 클래스
public class TilePlaceHandler : MonoBehaviour, IPlayerInputHandler
{
    // === Delegate ===
    public event Func<TurnResultInfo, UniTask> OnTilePlacedAsync;
    public event Func<TurnResultInfo, UniTask> OnLineClearedAsync;
    public event Func<TurnResultInfo, UniTask> OnTileRemovedAsync;
    public event Func<TurnResultInfo, UniTask> OnTileBurstAsync;
    public event Func<TurnResultInfo, UniTask> OnTurnProcessedAsync;
    
    // === Properties ===
    private Queue<TileEvent> _eventQueue = new Queue<TileEvent>();
    private TurnResultInfo _turnResultInfo;
    
    // === Functions ===
    public async UniTask HandlePlayerInput(PlayerInputData inputData, CancellationToken token)
    {
        if (inputData.Type == PlayerInputData.InputType.TilePlace)
        {
            await FirstTilePlaced(inputData.PlacedTile, token);
        }
        else if (inputData.Type == PlayerInputData.InputType.UseItem)
        {
            // TODO
            // ProcessUseItem(어쩌고) .. 
        }
    }
    
    // 첫 배치 때 불릴 함수
    public async UniTask FirstTilePlaced(List<Tile> tiles, CancellationToken token)
    {
        _turnResultInfo = new TurnResultInfo();
        _eventQueue.Clear();
        
        _eventQueue.Enqueue(new TilePlaceEvent(tiles));
        
        await ProcessTileEventQueue(token);
    }
    
    private async UniTask ProcessTileEventQueue(CancellationToken token)
    {
        while (_eventQueue.Count > 0)
        {
            TileEvent currentEvent = _eventQueue.Dequeue();
            LogEx.Log($"Process {currentEvent.TileEventType.ToString()}");

            switch (currentEvent.TileEventType)
            {
                case eTileEventType.Place:
                    await ProcessTilePlaced((TilePlaceEvent)currentEvent, token);
                    break;
                case eTileEventType.LineClear:
                    await ProcessLineCompleted((LineClearEvent)currentEvent, token);
                    break;
                case eTileEventType.Burst:
                    await ProcessTileBurst((TileBurstEvent)currentEvent, token);
                    break;
                case eTileEventType.Remove:
                    await ProcessTileRemoved((TileRemoveEvent)currentEvent, token);
                    break;
            }
        }
        
        // Queue가 비게 되면 턴 종료. TurnProcessedDelegate 끝
        await InvokeTileEventAsync(OnTurnProcessedAsync, _turnResultInfo, token);
    }
    

    private async UniTask ProcessTilePlaced(TilePlaceEvent placeEvent, CancellationToken token)
    {
        _turnResultInfo.PlacedTiles.AddRange(placeEvent.Tiles);
        
        // 패시브 아이템 효과나 점수 추가 등의 로직이 전부 종료될 때까지 대기한다
        await InvokeTileEventAsync(OnTilePlacedAsync, _turnResultInfo, token);
        
        // TODO
        // if (lineClearedCount > 0)
        // {
        //     _eventQueue.Enqueue(new LineClearEvent(lineClearedCount, clearedTiles));
        // }
    }
    
    private async UniTask ProcessLineCompleted(LineClearEvent lineClearEvent, CancellationToken token)
    {
        Debug.Log("Process Line Complete");
        _turnResultInfo.ClearedLineCount += lineClearEvent.ClearedLineCount;
        _turnResultInfo.ClearedTiles.AddRange(lineClearEvent.Tiles);
        
        await InvokeTileEventAsync(OnLineClearedAsync, _turnResultInfo, token);
    }

    private async UniTask ProcessTileRemoved(TileRemoveEvent removeEvent, CancellationToken token)
    {
        _turnResultInfo.RemovedTiles.AddRange(removeEvent.Tiles);
        
        await InvokeTileEventAsync(OnTileRemovedAsync, _turnResultInfo, token);
    }

    private async UniTask ProcessTileBurst(TileBurstEvent burstEvent, CancellationToken token)
    {
        _turnResultInfo.BurstTiles.AddRange(burstEvent.Tiles);
        
        await InvokeTileEventAsync(OnTileBurstAsync, _turnResultInfo, token);
    }
    
    private async UniTask InvokeTileEventAsync(Func<TurnResultInfo, UniTask> eventDelegate,
        TurnResultInfo info, CancellationToken token)
    {
        if (eventDelegate == null) return;
        
        await ExecEventBus<TurnResultInfo>.InvokeMerged(info);
        
        foreach (var handler in eventDelegate.GetInvocationList()
                     .Cast<Func<TurnResultInfo, UniTask>>())
        {
            await handler(info).AttachExternalCancellation(token); // 하나씩 순차 실행
        }
    }
    
    private (int, List<Tile>) CheckLineCompleted()
    {
        int clearedLineCount = 0;
        
        List<Tile> clearedTiles = new List<Tile>();
     
        // TODO
        // xyz 3축 사용해서 이래저래 확인하고..

        return (clearedLineCount, clearedTiles);
    }
}