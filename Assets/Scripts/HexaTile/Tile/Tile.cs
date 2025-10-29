using UnityEngine;

[PoolSize(20)]
public class Tile : MonoBehaviour, IPoolAble<TileData>
{
    /// <summary>
    /// 맵 절대 타일 좌표
    /// </summary>
    public Coordinate Coor;
    /// <summary>
    /// 원래 속했던 TileSet
    /// </summary>
    public TileSet Group;
    /// <summary>
    /// 속한 Cell
    /// </summary>
    public Cell Oner;
    public bool IsPlace => Oner != null;
    public Direction Direction;
    public TileOption Option;
    /// <summary>
    /// 해당 Tile Data
    /// </summary>
    public TileData TileData { get; }
    private TileData _tileData;

    public void Set(TileData data)
    {
        _tileData = data;
        gameObject.transform.localScale = Vector3.one * data.Scale;
        Option = data.Option;
    }

    public void Reset()
    {
    }
}
