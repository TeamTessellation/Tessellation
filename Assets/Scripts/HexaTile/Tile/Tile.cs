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
    public TileOption Option;
    private Sprite _defualtSprite;
    /// <summary>
    /// �ش� Tile Data
    /// </summary>
    public TileData TileData { get; }
    private TileData _tileData;
    private SpriteRenderer _sr;

    public void ChangeSprite(Sprite sprite)
    {
        _sr.sprite = sprite;
    }

    public void Set(TileData data)
    {
        _sr = GetComponent<SpriteRenderer>();
        _defualtSprite = _sr.sprite;
        _tileData = data;
        gameObject.transform.localScale = Vector3.one * data.Scale;
        Option = data.Option;
    }

    public void Reset()
    {
        _sr.sprite = _defualtSprite;
    }
}
