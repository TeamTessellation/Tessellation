using Cysharp.Threading.Tasks;
using System.Drawing;
using UnityEngine;

public abstract class TileOptionBase
{
    public virtual async UniTask OnTilePlaced(Tile tile)
    {
        int baseScore = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BasePlaceScore];
        int finalScore = ScoreManager.Instance.CalculateTileScore(eTileEventType.Place, tile, baseScore);
        ScoreManager.Instance.AddCurrentScore(finalScore);
        
        ShowScoreEffect(finalScore, tile);
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
        Vector2 worldPos = tile.transform.position;
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        EffectManager.Instance.ShowScoreEffect(score, screenPos);
    }
}
