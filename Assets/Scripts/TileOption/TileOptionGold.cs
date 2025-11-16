using Core;
using Cysharp.Threading.Tasks;
using Player;
using UnityEngine;

public class TileOptionGold : TileOptionBase
{
    public override async UniTask OnTilePlaced(Tile tile)
    {
        
    }

    public override async UniTask OnLineCleared(Tile tile)
    {
        int baseCoin = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseCoinTileValue];
        PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;
        playerStatus.CurrentCoins += baseCoin;
    }

    public override async UniTask OnTileRemoved(Tile tile)
    {
        
    }

    public override async UniTask OnTileBurst(Tile tile)
    {
        int baseCoin = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseCoinTileValue];
        PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;
        playerStatus.CurrentCoins += baseCoin;
    }
}
