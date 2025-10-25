using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class Field : MonoBehaviour
{
    public static Field Instance {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("@Field").GetComponent<Field>();
                _instance.InitField();
            }
            return _instance;
        }
    }

    private Transform _cellRoot;

    private static Field _instance;

    private Dictionary<Coordinate, Cell> _allCell;
    private int _size = 5;
    private bool _isInit = false;

    // 타일 배치, 라인 클리어 시 호출 이벤트
    /// <summary>
    /// 타일 배치 시작 ( 배치를 시작만 하고 실질적인 배치는 아직 안함 )
    /// </summary>
    public Action<Coordinate> PlaceTileStartEvent;
    /// <summary>
    /// 타일 베치 끝 ( 실질적인 타일을 맵에 배치함 )
    /// </summary>
    public Action<Coordinate> PlaceTileEndEvent;
    /// <summary>
    /// 타일셋 배치 시작 ( 배치를 시작만 하고 실질적인 배치는 아직 안함 )
    /// </summary>
    public Action<Coordinate> PlaceTileSetStartEvent;
    /// <summary>
    /// 타일셋 베치 끝 ( 실질적인 타일셋을 맵에 배치함 )
    /// </summary>
    public Action<Coordinate> PlaceTileSetEndEvent;
    /// <summary>
    /// 라인 클리어 시작 ( 줄을 전부 채워 특정 라인을 지우기 시작함 )
    /// </summary>
    public Action<Coordinate> LineClearStartEvent;
    /// <summary>
    /// 라인 클리어 끝 ( 줄에 존재하는 타일을 전부 제거함 )
    /// </summary>
    public Action<Coordinate> LineClearEndEvent;

    private void Awake() // 싱글톤은 아님
    {
        _instance = this;
        InitField();
    }

    private void InitField()
    {
        if (_isInit) return;
        _isInit = true;

        _allCell = new();
        var childs = transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < childs.Length; i++)
            if (childs[i].name == "@CellRoot")
                { _cellRoot = childs[i]; break; }

        SetFieldBySize(_size);
    }

    private void SetFieldBySize(int size)
    {
        for (int x = -size; x <= size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                int z = -(x + y);
                if (Mathf.Abs(z) > size)
                    continue;

                var coor = new Coordinate(x, y, z);
                if (!_allCell.ContainsKey(coor))
                    _allCell[coor] = new Cell();
                var a = Pool<Tile>.Get();
                a.Coor = coor;
                _allCell[coor].Set(a);
                a.name = coor.ToString();
            }
        }
    }

    /// <summary>
    /// 배치를 시도한다 가능하면 배치 불가능할 시 배치 X
    /// </summary>
    /// <param name="tileSet">배치할 타일셋</param>
    /// <param name="coor">배치를 원하는 위치</param>
    /// <returns>배치 성공 여부</returns>
    public bool TryPlace(TileSet tileSet, Coordinate coor)
    {
        if (CanPlace(tileSet, coor))
        {
            PlaceTileSetStartEvent.Invoke(coor);
            for (int i = 0; i < tileSet.Tiles.Count; i++)
            {
                var tileInfo = tileSet.Tiles[i].transform.localPosition;
                Coordinate correctCoor = coor + tileSet.Tiles[i].transform.localPosition;
                SetTileOnCell(tileSet.Tiles[i], coor);
            }
            PlaceTileSetEndEvent.Invoke(coor);
            return true;
        }
        return false;
    }

    /// <summary> 
    /// Tile을 coor에 배치한다.
    /// </summary>
    /// <param name="tile">배치할 타일</param>
    /// <param name="coor">배치를 원하는 위치</param>
    public void TryPlace(Tile tile, Coordinate coor)
    {
        if(CanPlace(tile, coor))
            SetTileOnCell(tile, coor);
    }

    /// <summary>
    /// 타일셋이 coor에 배치 가능한가?
    /// </summary>
    public bool CanPlace(TileSet tileSet, Coordinate coor)
    {
        for (int i = 0; i < tileSet.Tiles.Count; i++)
        {
            var tileInfo = tileSet.Tiles[i];
            Coordinate correctCoor = coor + tileSet.Tiles[i].transform.localPosition;
            if (!CanPlace(tileSet.Tiles[i], correctCoor))
                return false;
        }
        return true;
    }

    /// <summary>
    /// 타일이 coor에 배치 가능한가?
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="coor"></param>
    /// <returns></returns>
    public bool CanPlace(Tile tile, Coordinate coor)
    {
        if (CheckAbleCoor(coor)) // 원형 범위 안에 있는지 체크
        {
            // 안에 있다면 타일 체크 해야겠지~
            if (!_allCell[coor].IsEmpty && tile.TileData.Option != TileOption.Force)
                return false;
        }
        else
            return false;
        return true;
    }

    /// <summary>
    /// 배치 과정을 통합할 필요가 있을거같아서 따로 함수 만들어서 관리하겠씀
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="coor"></param>
    private void SetTileOnCell(Tile tile, Coordinate coor)
    {
        PlaceTileStartEvent?.Invoke(coor);
        tile.Coor = coor;
        _allCell[coor].Set(tile);
        PlaceTileEndEvent?.Invoke(coor);
    }

    /// <summary>
    /// coor 위치에 tile을 가져온다
    /// </summary>
    /// <param name="coor">원하는 위치</param>
    /// <param name="tile">성공 시 할당</param>
    /// <returns>성공 여부</returns>
    public bool TryGetTile(Coordinate coor, out Tile tile)
    {
        tile = GetTile(coor);
        return !_allCell[coor].IsEmpty;
    }

    /// <summary>
    /// coor cell에 할당 된 tile을 가져온다
    /// </summary>
    /// <param name="coor">원하는 위치</param>
    /// <returns>배치 된 타일 (없으면 null)</returns>
    public Tile GetTile(Coordinate coor) => _allCell[coor].Tile;

    /// <summary>
    /// 해당 coor이 맵 안에 위치한지 판별한다
    /// </summary>
    /// <param name="coor">원하는 위치</param>
    public bool CheckAbleCoor(Coordinate coor) => coor.CircleRadius <= _size;
    public static bool CheckAbleCoor(Coordinate coor, int size) => coor.CircleRadius <= size;
}
