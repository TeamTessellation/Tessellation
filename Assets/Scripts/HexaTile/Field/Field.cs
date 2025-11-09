using Cysharp.Threading.Tasks;
using Stage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using static Field;

public class Field : MonoBehaviour
{
    public struct Line
    {
        public Axis Axis;
        public Coordinate Start;
        public int Number;

        public Line(Axis axis, Coordinate coor)
        {
            Axis = axis;
            Start = coor;

            switch (axis)
            {
                case Axis.X:
                    Number = coor.Pos3D.y;
                    break;
                case Axis.Y:
                    Number = coor.Pos3D.x;
                    break;
                case Axis.Z:
                    Number = coor.Pos3D.z;
                    break;
                default:
                    Number = -1;
                    break;
            }
        }
    }

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

    [HideInInspector] public Transform CellRoot;
    [HideInInspector] public Transform CellBGRoot;

    private static Field _instance;
    private Dictionary<Coordinate, Cell> _allCell;

    public Vector2 TileOffset { get { return transform.position; } set { transform.position = value; } }

#if UNITY_EDITOR
    [Header("For Debug")]
    public int Debug_Size = 4;
#endif

    public int Size { get { return _size; } }

    private int _size = 4;
    private bool _isInit = false;

    private void Awake() // 싱글톤은 아님
    {
        _instance = this;
        InitField();
    }

    public void InitField()
    {
        if (_isInit) return;
        _isInit = true;

        _allCell = new();

        var childs = transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < childs.Length; i++)
        {
            if (childs[i].name == "@CellRoot")
            { CellRoot = childs[i]; continue; }
            else if (childs[i].name == "@CellBGRoot")
            { CellBGRoot = childs[i]; continue; }
        }
    }

    private void RemoveCell(Coordinate coor)
    {
        _allCell[coor].Remove();
        _allCell.Remove(coor);
    }

    /*
    public bool CanPlace(TileSet tileset)
    {

    }    

    public bool CanPlace(Tile tile)
    {

    }
    */

    private void RemoveTile(Coordinate coor)
    {
        //_allCell[coor].UnSet();
        RemoveTileEffect(_allCell[coor]).Forget();
    }

    private async UniTask RemoveTileEffect(Cell cell)
    {
        await UniTask.WaitForSeconds(0.2f);
        cell.UnSet();
    }

    public void SafeRemoveTile(Coordinate coor)
    {
        if (CheckAbleCoor(coor))
            RemoveTileEffect(_allCell[coor]).Forget();
    }


#if UNITY_EDITOR
    [ContextMenu("Debug_ChangeSize")]
    public void Debug_SetFieldBySize() => SetFieldBySize(Debug_Size);
#endif

    public void ResetField(int size)
    {
        SetFieldBySize(size);
        RemoveAllTile();
    }

    private void RemoveAllTile()
    {
        foreach (var cell in _allCell.Values)
        {
            cell.UnSet();
        }
    }

    private void SetFieldBySize(int size)
    {
        // 범위에 벗어나는 타일은 제거 - size가 작아지는 경우
        List<Coordinate> removeTargetCell = new();
        foreach (var cell in _allCell.Keys)
        {
            if (!CheckAbleCoor(cell, size))
                removeTargetCell.Add(cell);
        }
        for (int i = 0; i < removeTargetCell.Count; i++)
            RemoveCell(removeTargetCell[i]);

        // 배경 타일 세팅 & allCell에 Cell이 없다면 할당
        for (int x = -size; x <= size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                int z = -(x + y);
                if (Mathf.Abs(z) > size)
                    continue;

                var coor = new Coordinate(x, y);
                if (!_allCell.ContainsKey(coor))
                { _allCell[coor] = new Cell(); _allCell[coor].Init(coor, CellBGRoot, CellRoot); }
            }
        }
    }

    /// <summary>
    /// to다산 Coor 주변 타일을 가져온다. 없으면 null
    /// </summary>
    /// <param name="coor">해당 좌표</param>
    /// <returns></returns>
    public Dictionary<Direction, Tile> GetAroundTile(Coordinate coor)
    {
        Dictionary<Direction, Tile> result = new();
        for (int i = 0; i <= (int)Direction.LU; i++)
        {
            if (CheckAbleCoor(coor + (Direction)i))
                result[(Direction)i] = GetTile(coor);
            else
                result[(Direction)i] = null;
        }
        return result;
    }

    /// <summary>
    /// 배치를 시도한다 가능하면 배치 불가능할 시 배치 X
    /// </summary>
    /// <param name="tileSet">배치할 타일셋</param>
    /// <param name="coor">배치를 원하는 위치</param>
    /// <returns>배치 성공 여부</returns>
    public bool TryPlace(TileSet tileSet, Coordinate coor, out List<Tile> placeTiles)
    {
        placeTiles = null;
        if (CanPlace(tileSet, coor))
        {
            placeTiles = new();
            for (int i = 0; i < tileSet.Tiles.Count; i++)
            {
                var tileInfo = tileSet.Tiles[i].transform.localPosition;
                Coordinate correctCoor = coor + tileSet.Tiles[i].transform.localPosition.ToCoor();
                SetTileOnCell(tileSet.Tiles[i], correctCoor);
                placeTiles.Add(tileSet.Tiles[i]);
            }
            tileSet.Use();
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
            Coordinate correctCoor = coor + tileSet.Tiles[i].transform.localPosition.ToCoor();
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
            if (!_allCell[coor].IsEmpty && tile.Data.Option != TileOption.Force)
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
        tile.Coor = coor;
        _allCell[coor].Set(tile);
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
