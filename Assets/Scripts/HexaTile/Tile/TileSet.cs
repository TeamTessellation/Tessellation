using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tile ���� �ٷ���
/// ���� � ����� ���� �Ǿ������� �� �� �ִ�
/// </summary>
[PoolSize(5)]
public class TileSet : MonoBehaviour, IPoolAble<TileSetData>
{
    public List<Tile> Tiles = new();
    public Direction Rotation;
    private TileSetData _data;
    private Transform _tileRoot;

    public void Awake()
    {
        _tileRoot = transform.GetChild(0);
    }

    public void Reset()
    {
        _data = null;
        for (int i = 0; i < Tiles.Count; i++)
            Pool<Tile>.Return(Tiles[i]);
        Tiles.Clear();
    }

    public void Set(TileSetData data)
    {
        _data = data;
        for (int i = 0; i < data.Data.Count; i++)
        {
            var localPos = data.Data[i].Coor.ToWorld();
            var tile = Pool<Tile, TileData>.Get(data.Data[i].TileData);
            tile.transform.localPosition = localPos;
            transform.localScale = Vector2.one * data.Size;
            tile.transform.SetParent(_tileRoot, false);
            Tiles.Add(tile);
        }
    }
}
