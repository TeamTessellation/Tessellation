using Cysharp.Threading.Tasks;

using SaveLoad;
using Stage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sound;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Field : MonoBehaviour, ISaveTarget, IEnumerable<Cell>
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
    private List<GameObject> _silhouetteTiles;

    public Vector2 TileOffset { get { return transform.position; } set { transform.position = value; } }

#if UNITY_EDITOR
    [Header("For Debug")]
    public int Debug_Size = 4;
#endif

    public int Size { get { return _size; } }

    public Guid Guid { get; init; }

    private int _size = 4;
    private bool _isInit = false;

    private void Awake() // 싱글톤은 아님
    {
        _instance = this;
        InitField();
        SaveLoadManager.RegisterPendingSavable(this);
    }

    public bool TryPlaceAllTileSet(List<HandBox> handBoxs, bool canRotate, int count)
    {
        for (int i = 0; i < handBoxs.Count; i++)
        {
            if (!handBoxs[i].IsUsed)
            {
                foreach (var cell in _allCell)
                {
                    if (cell.Value.IsEmpty)
                    {
                        if (canRotate)
                        {
                            if (CanPlace(handBoxs[i].HoldTileSet, cell.Key, count))
                                return true;
                        }
                        else
                        {
                            if (CanPlace(handBoxs[i].HoldTileSet, cell.Key))
                                return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public Cell GetCellByCoordinate(Coordinate coor)
    {
        if (CheckAbleCoor(coor) && !_allCell[coor].IsEmpty)
        {
            return _allCell[coor];
        }

        return null;
    }
    
    public bool TryPlaceAllTileSet(List<HandBox> handBoxs)
    {
        for (int i = 0; i < handBoxs.Count; i++)
        {
            if (!handBoxs[i].IsUsed)
            {
                foreach(var cell in _allCell)
                {
                    if (cell.Value.IsEmpty)
                    {
                        if (CanPlace(handBoxs[i].HoldTileSet, cell.Key))
                            return true;
                    }
                }
            }
        }
        return false;
    }

    public void InitField()
    {
        if (_isInit) return;
        _isInit = true;

        _allCell = new();
        _silhouetteTiles = new();

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

    public void ClearSilhouette()
    {
        for (int i = 0; i < _silhouetteTiles.Count; i++)
        {
            Pool.Return(_silhouetteTiles[i]);
        }
        _silhouetteTiles.Clear();
    }

    public void ShowSilhouette(TileSet tileSet, Coordinate coor)
    {
        if (!CanPlace(tileSet, coor))
            return;

        ClearSilhouette();

        HashSet<Coordinate> coors = new();
        for (int i = 0; i < tileSet.Tiles.Count; i++)
        {
            var correctCoor = coor + tileSet.Tiles[i].transform.localPosition.ToCoor();
            if (coors.Contains(correctCoor))
                continue;
            coors.Add(correctCoor);
            GenerateSilhouetteTile(correctCoor);
        }
    }

    public void ShowSilhouette(Tile tile, Coordinate coor)
    {
        if (!CanPlace(tile, coor))
            return;

        ClearSilhouette();
        GenerateSilhouetteTile(coor);
    }

    private void GenerateSilhouetteTile(Coordinate coor)
    {
        var silhouetteObj = Pool.Get("SilhouetteTile");
        silhouetteObj.transform.position = coor.ToWorld(TileOffset);
        _silhouetteTiles.Add(silhouetteObj);
    }

    private void RemoveTile(Coordinate coor)
    {
        //_allCell[coor].UnSet();
        ActiveTileEffect(_allCell[coor]).Forget();
    }

    private async UniTask ActiveTileEffect(Cell cell, Action<Tile> remainAction = null)
    {
        await cell.Tile.ActiveEffect(cell.UnSet, remainAction);
    }

    public async UniTask RemoveTileEffect(Cell cell, Action<Tile> remainAction = null)
    {
        await cell.Tile.RemoveEffect();
    }

    public async UniTask SafeRemoveTile(Coordinate coor, Action<Tile> remainAction = null, float sfx_pitch = 1f)
    {
        if (CheckAbleCoor(coor) && !_allCell[coor].IsEmpty)
        {
            var cell = _allCell[coor];
            SoundManager.Instance.PlaySfx(SoundReference.TileClear, pitch:sfx_pitch);
            await ActiveTileEffect(cell, remainAction);
        }
        await UniTask.CompletedTask;
    }


#if UNITY_EDITOR
    [ContextMenu("Debug_ChangeSize")]
    public void Debug_SetFieldBySize() => SetFieldBySize(Debug_Size);
#endif

    public void ResetField(int size)
    {
        SetFieldBySize(size);
        ResetAllTiles();
    }

    private void RemoveAllTile()
    {
        foreach (var cell in _allCell.Values)
        {
            cell.UnSet();
        }
    }

    /// <summary>
    /// 모든 타일을 초기화 상태로 되돌린다.
    /// </summary>
    private void ResetAllTiles()
    {
        foreach (var cell in _allCell.Values)
        {
            cell.UnSet();
            cell.ResetSize();
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

    public bool CanPlace(TileSet tileSet, Coordinate coor, int rotate)
    {
        for (int i = 0; i < tileSet.Tiles.Count; i++)
        {
            var tileInfo = tileSet.Tiles[i];
            Coordinate rotateCoor = tileSet.Tiles[i].transform.localPosition.ToCoor();
            for (int j = 0; j < rotate; j++)
            {
                rotateCoor = rotateCoor.RotateR60();
            }
            Coordinate correctCoor = coor + rotateCoor;
            if (!CanPlace(tileSet.Tiles[i], correctCoor))
                return false;
        }
        return true;
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

    public void LoadData(GameData data)
    {
        ResetField(data.FieldSize);
        for (int i = 0; i < data.FieldTileData.Count; i++)
        {
            var tileData = data.FieldTileData[i];
            TryPlace(Pool<Tile, TileData>.Get(tileData.TileData), tileData.Coor);
        }
    }

    public void SaveData(ref GameData data)
    {
        data.FieldSize = _size;
        data.FieldTileData = new();
        foreach(var cell in _allCell)
        {
            if (cell.Value.IsEmpty)
                continue;

            OffsetTileData tileData = new();
            tileData.Coor = cell.Key;
            tileData.TileData = cell.Value.Tile.Data;
            data.FieldTileData.Add(tileData);
        }
    }

    public IEnumerator<Cell> GetEnumerator()
    {
        foreach (var cell in _allCell.Values)
        {
            yield return cell;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
