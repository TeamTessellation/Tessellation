using Core;
using Cysharp.Threading.Tasks;
using Player;
using UnityEngine;

public class TileOptionBoom : TileOptionBase
{
    public override async UniTask OnTilePlaced(Tile tile)
    {
        int baseScore = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BasePlaceScore];
        int finalScore = ScoreManager.Instance.CalculateTileScore(eTileEventType.Place, tile, baseScore);
        ScoreManager.Instance.AddCurrentScore(finalScore);
        
        ShowScoreEffect(finalScore, tile);
    }

    public override async UniTask OnTileBurst(Tile tile)
    {
        Debug.Log("우와 타일이 폭발했당");
        
        int baseCoin = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseCoinTileValue];
        PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;
        playerStatus.CurrentCoins += baseCoin;
    }
}
