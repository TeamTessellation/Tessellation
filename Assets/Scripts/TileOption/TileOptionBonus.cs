

using Cysharp.Threading.Tasks;
using Sound;

public class TileOptionBonus : TileOptionBase
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
        int baseScore = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseBonusScore];
        int finalScore = ScoreManager.Instance.CalculateTileScore(eTileEventType.LineClear, tile, baseScore);
        ScoreManager.Instance.AddCurrentScore(finalScore);
        
        ShowScoreEffect(finalScore, tile);
    }

    public override async UniTask OnTileBurst(Tile tile)
    {
        int baseScore = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseLineClearMultiple];
        int finalScore = ScoreManager.Instance.CalculateTileScore(eTileEventType.Burst, tile, baseScore);
        ScoreManager.Instance.AddCurrentScore(finalScore);
        
        SoundManager.Instance.PlaySfx(SoundReference.TileBomb);
    }
}