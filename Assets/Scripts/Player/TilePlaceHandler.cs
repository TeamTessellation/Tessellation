using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타일 이벤트의 종류를 정의
/// </summary>
public enum eTileEventType
{
    Placed,
    Destroyed,
    LineCompleted,
}

/// <summary>
/// 타일 이벤트를 받아 후처리를 담당
/// 점수 계산, 이펙트 재생, 증강 적용을 트리거한다
/// </summary>
public class TilePlaceHandler : MonoBehaviour
{
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
            case eTileEventType.LineCompleted:
                HandleLineCompleted(tiles);
                break;
        }
    }

    /// <summary>
    /// 타일이 배치되었을 때 후처리 담당
    /// </summary>
    private void HandleTilePlaced(List<int> placedTiles)
    {
        // TODO
        // 1. 기본 점수 정의
        // 2. 조커에 따른 점수 추가
        // 3. 타일 배치 시 발동하는 조커 트리거
        // 4. 기타 조커 트리거
        // * 지속적으로 이펙트 효과 트리거
    }

    /// <summary>
    /// 타일이 배치되었을 때 후처리 담당
    /// </summary>
    private void HandleTileDestroyed(List<int> placedTiles)
    {
        // TODO
        // 1. 기본 점수 정의
        // 2. 조커에 따른 점수 추가
        // 3. 타일 파괴 시 발동하는 조커 트리거
        // 4. 기타 조커 트리거
        // * 지속적으로 이펙트 효과 트리거
    }

    /// <summary>
    /// 타일이 완성되었을 때 후처리 담당
    /// </summary>
    private void HandleLineCompleted(List<int> completedTiles)
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