using Cysharp.Threading.Tasks;
using Sound;
using UnityEngine;

public abstract class TileOptionBase
{
    public virtual async UniTask OnTilePlaced(Tile tile)
    {
    }

    public virtual async UniTask OnLineCleared(Tile tile)
    {
    }

    public virtual async UniTask OnTileRemoved(Tile tile)
    {
    }

    public virtual async UniTask OnTileBurst(Tile tile)
    {
    }

    protected void ShowScoreEffect(int score, Tile tile)
    {
        Vector3 pos = tile.transform.position;
        EffectManager.Instance.ShowScoreEffect(score, pos);
    }
}
