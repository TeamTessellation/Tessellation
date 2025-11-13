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
    // === Settings ===
    [Header("Settings")] 
    [SerializeField] private float tileRemoveInterval = 0.1f;
    
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
            PushExtraQueue();
        }
        
        // Queue가 비게 되면 턴 종료. TurnProcessedDelegate 끝
        await InvokeTileEventAsync(OnTurnProcessedAsync, _turnResultInfo, token);
        _turnResultInfo.Dispose();
    }

    private void PushExtraQueue()
    {
        // 라인 완성 확인
        LineClearHandler lineClearHandler = new LineClearHandler();
        List<Field.Line> clearLines = lineClearHandler.CheckLineClear(_turnResultInfo.PlacedTiles);
        List<Tile> clearedTiles = lineClearHandler.GetTilesFromLines(clearLines);
        if (clearLines.Count > 0)
        {
            _eventQueue.Enqueue(new LineClearEvent(clearLines, clearedTiles));
        }
    }
    
    private async UniTask ProcessTilePlaced(TilePlaceEvent placeEvent, CancellationToken token)
    {
        foreach (var tile in placeEvent.Tiles)
        {
            await tile.TileOptionBase.OnTilePlaced(tile);
        }
        
        _turnResultInfo.PlacedTiles.AddRange(placeEvent.Tiles);
        
        await InvokeTileEventAsync(OnTilePlacedAsync, _turnResultInfo, token);
       
        await UniTask.Delay(1000);
    }
    
    private async UniTask ProcessLineCompleted(LineClearEvent lineClearEvent, CancellationToken token)
    {
        foreach (var tile in lineClearEvent.Tiles)
        {
            await tile.TileOptionBase.OnLineCleared(tile);
        }

        // 1차 이펙트 처리
        LineClearHandler lineClearHandler = new LineClearHandler();

        await lineClearHandler.ClearLinesAsync(lineClearEvent.ClearedLine, tileRemoveInterval);

        // TODO : 2차 이펙트 처리


        _turnResultInfo.ClearedLineCount += lineClearEvent.ClearedLineCount;
        _turnResultInfo.ClearedTiles.AddRange(lineClearEvent.Tiles);
        
        await InvokeTileEventAsync(OnLineClearedAsync, _turnResultInfo, token);
    }

    private async UniTask ProcessTileRemoved(TileRemoveEvent removeEvent, CancellationToken token)
    {
        foreach (var tile in removeEvent.Tiles)
        {
            await tile.TileOptionBase.OnTileRemoved(tile);
        }
        
        _turnResultInfo.RemovedTiles.AddRange(removeEvent.Tiles);
        
        await InvokeTileEventAsync(OnTileRemovedAsync, _turnResultInfo, token);
    }

    private async UniTask ProcessTileBurst(TileBurstEvent burstEvent, CancellationToken token)
    {
        foreach (var tile in burstEvent.Tiles)
        {
            await tile.TileOptionBase.OnTileBurst(tile);
        }
        
        _turnResultInfo.BurstTiles.AddRange(burstEvent.Tiles);
        
        await InvokeTileEventAsync(OnTileBurstAsync, _turnResultInfo, token);
    }
    
    private async UniTask InvokeTileEventAsync(Func<TurnResultInfo, UniTask> eventDelegate,
        TurnResultInfo info, CancellationToken token)
    {
        if (eventDelegate == null) return;
        
        await ExecEventBus<TurnResultInfo>.InvokeMerged(info);
        PlayerStatus playerStatus = PlayerStatus.Current;
        
        playerStatus.StageClearedLines += info.ClearedLineCount;
        
        foreach (var handler in eventDelegate.GetInvocationList()
                     .Cast<Func<TurnResultInfo, UniTask>>())
        {
            await handler(info).AttachExternalCancellation(token); // 하나씩 순차 실행
        }
    }
}