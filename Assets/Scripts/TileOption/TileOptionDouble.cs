using Cysharp.Threading.Tasks;
using Sound;

public class TileOptionDouble : TileOptionBase
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
        
        int baseMultiplier = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseMultipleTileValue];
        ScoreManager.Instance.MultiplyMultiplier(baseMultiplier);
        
        ShowScoreEffect(finalScore, tile);
    }
}