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

    public void Reset()
    {
        _data = null;
        Tiles.Clear();
    }

    public void Set(TileSetData data)
    {
        _data = data;
        for (int i = 0; i < data.Data.Count; i++)
        {
            var localPos = data.Data[i].coor.ToWorld();
            var tile = Pool<Tile, TileData>.Get(data.Data[i].tileData);
            tile.transform.localPosition = localPos;
            Tiles.Add(tile);
        }
    }
}
