using UnityEngine;

[PoolSize(20)]
public class Tile : MonoBehaviour, IPoolAble<TileData>
{
    /// <summary>
    /// �� ���� Ÿ�� ��ǥ
    /// </summary>
    public Coordinate Coor;
    /// <summary>
    /// ���� ���ߴ� TileSet
    /// </summary>
    public TileSet Group;
    /// <summary>
    /// ���� Cell
    /// </summary>
    public Cell Oner;
    public bool IsPlace => Oner != null;
    public Direction Direction;
    /// <summary>
    /// �ش� Tile Data
    /// </summary>
    public TileData TileData { get; }
    private TileData _tileData;

    public void Set(TileData data)
    {
        _tileData = data;
    }

    public void Reset()
    {
    }
}
