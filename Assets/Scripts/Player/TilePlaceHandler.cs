using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타일 이벤트의 종류를 정의
/// </summary>
public enum eTileEventType
{
    Placed,
    Destroyed,
    Removed,
    LineCompleted,
}

public class ScoreAccumulator
{
    [SerializeField] private float baseScore = 0f;
    [SerializeField] private float bonusScore = 1f;
    [SerializeField] private float mulValue = 0f;

    public float BaseScore
    {
        get => baseScore;
        set
        {
            if (baseScore < value)
            {
                // 이펙트 출력
            }

            baseScore = value;
        }
    }
    
    public float BonusScore
    {
        get => bonusScore;
        set
        {
            if (bonusScore < value)
            {
                // 이펙트 출력
            }

            bonusScore = value;
        }
    }

    public float MulValue
    {
        get => mulValue;
        set
        {
            if (mulValue < value)
            {
                // 이펙트 출력
            }

            mulValue = value;
        }
    }
    
    public int GetFinalScore()
    {
        float FinalScore = (baseScore + bonusScore) * mulValue;
        return Mathf.FloorToInt(FinalScore);
    }
}

/// <summary>
/// 타일 이벤트를 받아 후처리를 담당
/// 점수 계산, 이펙트 재생, 증강 적용을 트리거한다
/// </summary>
public class TilePlaceHandler : MonoBehaviour
{
    // Event Delegate
    public event Action OnTilePlaceCompleted;
    public event Action OnTileDestroyCompleted;
    public event Action OnTileRemoveCompleted;
    public event Action OnLineCompletionCompleted;
    
    /// <summary>
    /// 타일에 변화가 있을 때 호출하는 함수
    /// </summary>
    /// <param name="tileEventType">타일 이벤트 타입</param>
    /// <param name="tiles">현재 타일이 없기 때문에 int로 대체</param>
    public void OnTileEvent(eTileEventType tileEventType , List<int> tiles)
    {
        switch (tileEventType)
        {
            case eTileEventType.Placed:
                HandleTilePlaced(tiles);
                break;
            case eTileEventType.Destroyed:
                HandleTileDestroyed(tiles);
                break;
            case eTileEventType.Removed:
                HandleTileRemoved(tiles);
                break;
            case eTileEventType.LineCompleted:
                HandleLineCompleted(tiles);
                break;
        }
    }

    /// <summary>
    /// 타일이 배치되었을 때 후처리 담당
    /// </summary>
    private void HandleTilePlaced(in List<int> placedTiles) // List<tile>
    {
        int placeScore = 0;
        bool isMultiple = false;

        var accumulator = new ScoreAccumulator();
        
        // 1. 기본 타일 정보 수집
        foreach (var tile in placedTiles)
        {
            placeScore += tile; // tile.score 로 처리해야함

            // if (tile.Option == ETileOptionType.Multiple)
            // {
            //     isMultiple = true;
            // }
            
            // 여기서 각 조커들에 대해 PiecePlaced를 검사할 수도 있다
        }

        // Multiple 적용 (적용 방식 미정)
        placeScore = isMultiple ? placeScore * 2 : placeScore;
        
        // // #Pseudocode
        // // 각 조커들에 대해 TilePlaced를 호출
        // foreach (var Joker in PlayerState.Inventory.Jokers)
        // {
        //     Joker.OnTilePlaced(placedTiles, accumulator);
        // }
        
        // 타일이 배치되었음을 전체 시스템애 알림
        OnTilePlaceCompleted?.Invoke();
    }

    /// <summary>
    /// 타일이 파괴되었을 때 후처리 담당
    /// </summary>
    private void HandleTileDestroyed(in List<int> placedTiles)
    {
        // TODO
        // 1. 기본 점수 정의
        // 2. 조커에 따른 점수 추가
        // 3. 타일 파괴 시 발동하는 조커 트리거
        // 4. 기타 조커 트리거
        // * 지속적으로 이펙트 효과 트리거
    }

    private void HandleTileRemoved(in List<int> placedTiles)
    {
        
    }

    /// <summary>
    /// 줄이 완성되었을 때 후처리 담당
    /// </summary>
    private void HandleLineCompleted(in List<int> completedTiles)
    { 
        // TODO
        // 1. 기본 점수 정의
        // 2. 조커에 따른 점수 추가
        // 3. 타일 완성 시 발동하는 조커 트리거
        // 4. 기타 조커 트리거
        // * 지속적으로 이펙트 효과 트리거
        // * 라인콤보 확인 
    }
}