using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

/// <summary>
/// Tile 묶음 꾸러미
/// 서로 어떤 얘들이 연결 되었는지만 알 수 있다
/// </summary>
[PoolSize(5)]
public class TileSet : MonoBehaviour, IPoolAble<TileSetData>
{
    public List<Sprite> RandomTileSprite;
    public List<Tile> Tiles = new();
    public Direction Rotation;
    public TileSetData Data { get; private set; }
    private Transform _tileRoot;
    private SortingGroup _sg;

    public void Awake()
    {
        _tileRoot = transform.GetChild(0);
        _sg = _tileRoot.GetComponent<SortingGroup>();
    }

    public void Use() => Tiles.Clear();

    public void Reset()
    {
        Data = null;
        for (int i = 0; i < Tiles.Count; i++)
            Pool<Tile>.Return(Tiles[i]);
        Tiles.Clear();
    }

    /// <summary>
    /// 60도 만큼 우측으로 회전시킨다.
    /// </summary>
    [ContextMenu("Rotate")]
    public void Rotate()
    {
        for(int i = 0; i < Tiles.Count; i++)
        {
            Tiles[i].transform.localPosition = Tiles[i].transform.localPosition.ToCoor().RotateR60().ToWorld();
        }
    }

    public void SetOrderInTop()
    {
        SetOrder("InGameUI", 1);
    }

    public void SetOrderInHand()
    {
        SetOrder("Field", 0);
    }

    private void SetOrder(string layer, int order)
    {
        _sg.sortingOrder = order;
        _sg.sortingLayerName = layer;
    }

    public void Set(TileSetData data)
    {
        Data = data;
        var randomSprite = RandomTileSprite[Random.Range(0, RandomTileSprite.Count)];
        SetOrderInHand();

        for (int i = 0; i < data.Data.Count; i++)
        {
            var localPos = data.Data[i].Coor.ToWorld();
            var tile = Pool<Tile, TileData>.Get(data.Data[i].TileData);
            tile.transform.localPosition = localPos;
            transform.localScale = Vector2.one * data.Size;
            tile.transform.SetParent(_tileRoot, false);
            tile.ChangeSprite(randomSprite);
            Tiles.Add(tile);
        }
    }
}
