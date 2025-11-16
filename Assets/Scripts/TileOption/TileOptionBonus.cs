

using Cysharp.Threading.Tasks;

public class TileOptionBonus : TileOptionBase
{
    public override async UniTask OnLineCleared(Tile tile)
    {
        int baseScore = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseBonusScore];
        int finalScore = ScoreManager.Instance.CalculateTileScore(eTileEventType.LineClear, tile, baseScore);
        ScoreManager.Instance.AddCurrentScore(finalScore);
    }

    public override async UniTask OnTileBurst(Tile tile)
    {
        int baseScore = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseLineClearMultiple];
        int finalScore = ScoreManager.Instance.CalculateTileScore(eTileEventType.Burst, tile, baseScore);
        ScoreManager.Instance.AddCurrentScore(finalScore);
    }
}