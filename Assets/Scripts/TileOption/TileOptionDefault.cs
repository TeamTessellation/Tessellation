using Cysharp.Threading.Tasks;
using Sound;
using UnityEngine;

public class TileOptionDefault : TileOptionBase
{
    public override async UniTask OnTilePlaced(Tile tile)
    {
        int baseScore = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BasePlaceScore];
        int finalScore = ScoreManager.Instance.CalculateTileScore(eTileEventType.Place, tile, baseScore);
        ScoreManager.Instance.AddCurrentScore(finalScore);
        
        ShowScoreEffect(finalScore, tile);
        
        SoundManager.Instance.PlaySfx(SoundReference.TileRelease);
    }
    
    public override async UniTask OnLineCleared(Tile tile)
    {
        int baseScore = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseLineClearScore];
        int finalScore = ScoreManager.Instance.CalculateTileScore(eTileEventType.LineClear, tile, baseScore);
        ScoreManager.Instance.AddCurrentScore(finalScore);
        
        ShowScoreEffect(finalScore, tile);
    }

    public override async UniTask OnTileBurst(Tile tile)
    {
        ScoreManager.Instance.AddCurrentScore(tile.Data.Score);
    }
}
