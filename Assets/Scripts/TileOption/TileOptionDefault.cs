using Cysharp.Threading.Tasks;
using UnityEngine;

public class TileOptionDefault : TileOptionBase
{
    public override async UniTask OnTilePlaced(Tile tile)
    {
        int baseScore = tile.Data.Score;
        int finalScore = ScoreManager.Instance.CalculateTileScore(eTileEventType.Place, tile, baseScore);
        ScoreManager.Instance.AddCurrentScore(finalScore);
    }

    public virtual async UniTask OnLineCleared(Tile tile)
    {
        int baseScore = tile.Data.Score * 5;
        int finalScore = ScoreManager.Instance.CalculateTileScore(eTileEventType.LineClear, tile, baseScore);
        ScoreManager.Instance.AddCurrentScore(finalScore);
    }

    public virtual async UniTask OnTileRemoved(Tile tile)
    {

    }

    public virtual async UniTask OnTileBurst(Tile tile)
    {
        ScoreManager.Instance.AddCurrentScore(tile.Data.Score);
    }
}
