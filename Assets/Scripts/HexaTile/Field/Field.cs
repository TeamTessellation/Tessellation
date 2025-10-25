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

    // Ÿ�� ��ġ, ���� Ŭ���� �� ȣ�� �̺�Ʈ
    /// <summary>
    /// Ÿ�� ��ġ ���� ( ��ġ�� ���۸� �ϰ� �������� ��ġ�� ���� ���� )
    /// </summary>
    public Action<Coordinate> PlaceTileStartEvent;
    /// <summary>
    /// Ÿ�� ��ġ �� ( �������� Ÿ���� �ʿ� ��ġ�� )
    /// </summary>
    public Action<Coordinate> PlaceTileEndEvent;
    /// <summary>
    /// Ÿ�ϼ� ��ġ ���� ( ��ġ�� ���۸� �ϰ� �������� ��ġ�� ���� ���� )
    /// </summary>
    public Action<Coordinate> PlaceTileSetStartEvent;
    /// <summary>
    /// Ÿ�ϼ� ��ġ �� ( �������� Ÿ�ϼ��� �ʿ� ��ġ�� )
    /// </summary>
    public Action<Coordinate> PlaceTileSetEndEvent;
    /// <summary>
    /// ���� Ŭ���� ���� ( ���� ���� ä�� Ư�� ������ ����� ������ )
    /// </summary>
    public Action<Coordinate> LineClearStartEvent;
    /// <summary>
    /// ���� Ŭ���� �� ( �ٿ� �����ϴ� Ÿ���� ���� ������ )
    /// </summary>
    public Action<Coordinate> LineClearEndEvent;

    private void Awake() // �̱����� �ƴ�
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
    /// ��ġ�� �õ��Ѵ� �����ϸ� ��ġ �Ұ����� �� ��ġ X
    /// </summary>
    /// <param name="tileSet">��ġ�� Ÿ�ϼ�</param>
    /// <param name="coor">��ġ�� ���ϴ� ��ġ</param>
    /// <returns>��ġ ���� ����</returns>
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
    /// Tile�� coor�� ��ġ�Ѵ�.
    /// </summary>
    /// <param name="tile">��ġ�� Ÿ��</param>
    /// <param name="coor">��ġ�� ���ϴ� ��ġ</param>
    public void TryPlace(Tile tile, Coordinate coor)
    {
        if(CanPlace(tile, coor))
            SetTileOnCell(tile, coor);
    }

    /// <summary>
    /// Ÿ�ϼ��� coor�� ��ġ �����Ѱ�?
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
    /// Ÿ���� coor�� ��ġ �����Ѱ�?
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="coor"></param>
    /// <returns></returns>
    public bool CanPlace(Tile tile, Coordinate coor)
    {
        if (CheckAbleCoor(coor)) // ���� ���� �ȿ� �ִ��� üũ
        {
            // �ȿ� �ִٸ� Ÿ�� üũ �ؾ߰���~
            if (!_allCell[coor].IsEmpty && tile.TileData.Option != TileOption.Force)
                return false;
        }
        else
            return false;
        return true;
    }

    /// <summary>
    /// ��ġ ������ ������ �ʿ䰡 �����Ű��Ƽ� ���� �Լ� ���� �����ϰھ�
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
    /// coor ��ġ�� tile�� �����´�
    /// </summary>
    /// <param name="coor">���ϴ� ��ġ</param>
    /// <param name="tile">���� �� �Ҵ�</param>
    /// <returns>���� ����</returns>
    public bool TryGetTile(Coordinate coor, out Tile tile)
    {
        tile = GetTile(coor);
        return !_allCell[coor].IsEmpty;
    }

    /// <summary>
    /// coor cell�� �Ҵ� �� tile�� �����´�
    /// </summary>
    /// <param name="coor">���ϴ� ��ġ</param>
    /// <returns>��ġ �� Ÿ�� (������ null)</returns>
    public Tile GetTile(Coordinate coor) => _allCell[coor].Tile;

    /// <summary>
    /// �ش� coor�� �� �ȿ� ��ġ���� �Ǻ��Ѵ�
    /// </summary>
    /// <param name="coor">���ϴ� ��ġ</param>
    public bool CheckAbleCoor(Coordinate coor) => coor.CircleRadius <= _size;
    public static bool CheckAbleCoor(Coordinate coor, int size) => coor.CircleRadius <= size;
}
