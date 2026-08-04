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
        
        Debug.Log(pos.ToString());
        EffectManager.Instance.ShowScoreEffect(score, pos);
    }

    protected async UniTask ApplyScoreRule(eTileEventType eventType, Tile tile)
    {
        TileScoreRule rule = TileScoreRules.Get(eventType, tile.Data.Option);

        if (rule.AwardsCoin)
        {
            int coin = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseCoinTileValue];
            GameManager.Instance.PlayerStatus.CurrentCoins += coin;
        }

        if (rule.AwardsScore)
        {
            int baseScore = (int)ScoreManager.Instance.ScoreValues[rule.ScoreType];
            int finalScore = ScoreManager.Instance.CalculateTileScore(eventType, tile, baseScore);
            ScoreManager.Instance.AddCurrentScore(finalScore,
                eventType == eTileEventType.LineClear
                    ? ScoreManager.CurrentScoreChangedEventArgs.CurrentScoreChangeType.LineCleared
                    : ScoreManager.CurrentScoreChangedEventArgs.CurrentScoreChangeType.Place);
            ShowScoreEffect(finalScore, tile);
        }

        if (rule.AppliesDoubleMultiplier)
        {
            float multiplier = ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseMultipleTileValue];
            ScoreManager.Instance.MultiplyMultiplier(multiplier);
        }

        if (eventType == eTileEventType.Place)
            SoundManager.Instance.PlaySfx(SoundReference.TileRelease);
        else if (eventType == eTileEventType.LineClear && tile.Data.Option == TileOption.Gold)
            SoundManager.Instance.PlaySfx(SoundReference.TileGold);

        await UniTask.CompletedTask;
    }
}
