using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tile 묶음 꾸러미
/// 서로 어떤 얘들이 연결 되었는지만 알 수 있다
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
