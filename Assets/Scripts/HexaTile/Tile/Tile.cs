using Cysharp.Threading.Tasks;
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
    public Cell Owner;
    public bool IsPlace => Owner != null;
    public Direction Direction;
    public TileOption Option;
    private Sprite _defaultSprite;

    /// <summary>
    /// 해당 Tile Data
    /// </summary>
    public TileData Data { get; private set; }

    public SpriteRenderer Sr { get { return _sr; } private set { _sr = value; } }
    private SpriteRenderer _sr;

    public void ChangeSprite(Sprite sprite)
    {
        _sr.sprite = sprite;
    }

    public void Set(TileData data)
    {
        _sr = GetComponent<SpriteRenderer>();
        _defaultSprite = _sr.sprite;
        Data = data;
        gameObject.transform.localScale = Vector3.one * data.Scale;
        Option = data.Option;
    }

    public void Reset()
    {
        _sr.sprite = _defaultSprite;
    }

    public async UniTask OnTilePlaced()
    {
        int baseScore = Data.Score;
        int finalScore = ScoreManager.Instance.CalculateTileScore(eTileEventType.Place, this, baseScore);
        ScoreManager.Instance.AddCurrentScore(finalScore);

        Vector2 popUpPosition = Coor.ToWorld(Field.Instance.TileOffset);
    }

    public async UniTask OnLineCleared()
    {
        int baseScore = Data.Score * 5;
        int finalScore = ScoreManager.Instance.CalculateTileScore(eTileEventType.LineClear, this, baseScore);
        ScoreManager.Instance.AddCurrentScore(finalScore);
    }
    
    public async UniTask OnTileRemoved()
    {
        
    }
    
    public async UniTask OnTileBurst()
    {
        ScoreManager.Instance.AddCurrentScore(Data.Score);
    }
}
