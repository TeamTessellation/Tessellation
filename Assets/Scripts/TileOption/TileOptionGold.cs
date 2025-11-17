using Core;
using Cysharp.Threading.Tasks;
using Player;
using Sound;
using UnityEngine;

public class TileOptionGold : TileOptionBase
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
        int baseCoin = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseCoinTileValue];
        PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;
        playerStatus.CurrentCoins += baseCoin;
        
        int baseScore = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseBonusScore];
        int finalScore = ScoreManager.Instance.CalculateTileScore(eTileEventType.LineClear, tile, baseScore);
        ScoreManager.Instance.AddCurrentScore(finalScore);
        
        ShowScoreEffect(finalScore, tile);
        
        SoundManager.Instance.PlaySfx(SoundReference.TileGold);
    }

    public override async UniTask OnTileBurst(Tile tile)
    {
        int baseCoin = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseCoinTileValue];
        PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;
        playerStatus.CurrentCoins += baseCoin;
    }
}
