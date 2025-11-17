using Cysharp.Threading.Tasks;

public class TileOptionDouble : TileOptionBase
{
    public override async UniTask OnLineCleared(Tile tile)
    {
        int baseScore = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseBonusScore];
        int finalScore = ScoreManager.Instance.CalculateTileScore(eTileEventType.LineClear, tile, baseScore);
        ScoreManager.Instance.AddCurrentScore(finalScore);
        
        int baseMultiplier = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseMultipleTileValue];
        ScoreManager.Instance.MultiplyMultiplier(baseMultiplier);
    }
}