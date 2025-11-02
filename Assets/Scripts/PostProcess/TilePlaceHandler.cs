using System;
using System.Collections.Generic;
using UnityEngine;

// 전체 턴 결과를 담는 데이터 Info
// 아직 어떻게 활용할지.. 구조 미정
public class TurnResultInfo
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
    public readonly int ClearedLineCount;
    public LineClearEvent(int clearedCount, List<Tile> newTiles) : base(newTiles)
    {
        TileEventType = eTileEventType.LineClear;
        ClearedLineCount = clearedCount;
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
public class TilePlaceHandler : MonoBehaviour
{
    // === Actions ===
    public event Action<TurnResultInfo> OnTilePlacedDelegate;
    public event Action<TurnResultInfo> OnTileRemovedDelegate;
    public event Action<TurnResultInfo> OnLineClearedDelegate;
    public event Action<TurnResultInfo> OnTileBurstDelegate;
    public event Action<TurnResultInfo> OnTurnProcessedDelegate;
    
    // === Properties ===
    private Queue<TileEvent> _eventQueue = new Queue<TileEvent>();
    private TurnResultInfo _turnResultInfo;
    
    // === Functions ===
    
    // 첫 배치 때 불릴 함수
    public void FirstTilePlaced(List<Tile> tiles)
    {
        _turnResultInfo = new TurnResultInfo();
        _eventQueue.Clear();
        
        _eventQueue.Enqueue(new TilePlaceEvent(tiles));

        ProcessTileEventQueue();
    }

    private void ProcessTileEventQueue()
    {
        while (_eventQueue.Count > 0)
        {
            TileEvent currentEvent = _eventQueue.Dequeue();

            switch (currentEvent.TileEventType)
            {
                case eTileEventType.Burst:
                    ProcessTileBurst((TileBurstEvent)currentEvent);
                    break;
                case eTileEventType.Place:
                    ProcessTilePlaced((TilePlaceEvent)currentEvent);
                    break;
                case eTileEventType.Remove:
                    ProcessTileRemoved((TileRemoveEvent)currentEvent);
                    break;
                case eTileEventType.LineClear:
                    ProcessLineCompleted((LineClearEvent)currentEvent);
                    break;
            }
        }
        
        // Queue가 비게 되면 턴 종료. TurnProcessedDelegate 끝
        OnTurnProcessedDelegate?.Invoke(_turnResultInfo);
    }
    

    private void ProcessTilePlaced(TilePlaceEvent placeEvent)
    {
        _turnResultInfo.PlacedTiles.AddRange(placeEvent.Tiles);
        
        // TODO
        // 즐 완성 판정
        int lineClearedCount;
        List<Tile> clearedTiles;
        (lineClearedCount, clearedTiles) = CheckLineCompleted();
        
        // 머시기 한다
        
        OnTilePlacedDelegate?.Invoke(_turnResultInfo);

        if (lineClearedCount > 0)
        {
            _eventQueue.Enqueue(new LineClearEvent(lineClearedCount, clearedTiles));
        }
    }
    
    private void ProcessTileRemoved(TileRemoveEvent removeEvent)
    {
        _turnResultInfo.RemovedTiles.AddRange(removeEvent.Tiles);
        OnTileRemovedDelegate?.Invoke(_turnResultInfo);
    }

    private void ProcessTileBurst(TileBurstEvent burstEvent)
    {
        _turnResultInfo.BurstTiles.AddRange(burstEvent.Tiles);
        OnTileBurstDelegate?.Invoke(_turnResultInfo);
    }
    
    private void ProcessLineCompleted(LineClearEvent lineClearEvent)
    {
        _turnResultInfo.ClearedLineCount += lineClearEvent.ClearedLineCount;
        _turnResultInfo.ClearedTiles.AddRange(lineClearEvent.Tiles);
        OnLineClearedDelegate?.Invoke(_turnResultInfo);
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
