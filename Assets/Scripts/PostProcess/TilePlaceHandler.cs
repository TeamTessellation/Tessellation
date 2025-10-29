using System;
using System.Collections.Generic;
using UnityEngine;

// 턴 결과를 담는 데이터 Info
public class TurnResultInfo
{
    public List<Tile> PlacedTiles; // 이번턴에 배치한 타일에 대한 정보
    public List<Tile> RemovedTiles; // 이번턴에 삭제될 타일에 대한 정보
    public List<Tile> BurstTiles; // 폭발되어 사라질 타일에 대한 정보 
    public List<Tile> ClearedTiles; // 완성되어 사라질 타일에 대한 정보
    
    public int ClearedLineCount; // 이번턴에 완성된 줄의 수
}

/*
 * 
 */
public class TilePlaceHandler : MonoBehaviour
{
    // * Delegates
    
    public event Action<TurnResultInfo> OnTilePlacedDelegate;

    public event Action<TurnResultInfo> OnTileRemovedDelegate;

    public event Action<TurnResultInfo> OnLineClearedDelegate;

    public event Action<TurnResultInfo> OnTileBurstDelegate;

    public event Action<TurnResultInfo> OnTurnProcessedDelegate;
    
    // end Delegates
    
    // Field
    private TurnResultInfo _turnResultInfo;
    
    // * Function
    
    // Ingame에서 Delegate 생성해서 해당 함수를 부르게 해도 좋다
    public void ProcessTilePlacement(List<Tile> tiles)
    {
        // 1. _turnResultInfo 초기화
        _turnResultInfo = new TurnResultInfo();
        _turnResultInfo.PlacedTiles = tiles;
        
        // 2. 타일 배치 처리
        ProcessTilePlaced();
        OnTilePlacedDelegate?.Invoke(_turnResultInfo);
        
        // 3. 타일 삭제 처리
        ProcessTileRemoved();
        OnTileRemovedDelegate?.Invoke(_turnResultInfo);
        
        // 4. 라인 클리어 처리
        CheckLineCompleted();
        if (_turnResultInfo.ClearedLineCount >= 0)
        {
            ProcessLineCompleted();
            OnLineClearedDelegate?.Invoke(_turnResultInfo);
        }
        
        // 5. 타일 폭발 처리
        ProcessTileBurst();
        OnTileBurstDelegate?.Invoke(_turnResultInfo);
        
        // 6. 턴 종료 Broadcast
        OnTurnProcessedDelegate?.Invoke(_turnResultInfo);
    }

    private void ProcessTilePlaced()
    {
        
    }
    
    private void ProcessTileRemoved()
    {
        
    }

    private void ProcessTileBurst()
    {
        
    }
    
    private void ProcessLineCompleted()
    {
        
    }

    private void CheckLineCompleted()
    {
        int clearedLineCount = 0;
        
        List<Tile> clearedTiles = new List<Tile>();
     
        // TODO
        // xyz 3축 사용해서 이래저래 확인하고..

        _turnResultInfo.ClearedLineCount = clearedLineCount;
        _turnResultInfo.ClearedTiles = clearedTiles;
    }
    
    // end Functions
}
