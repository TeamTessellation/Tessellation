using Abilities;
using Core;
using Player;
using UnityEngine;

public class GoldToGoldAbility : AbilityBase
{
    private ScoreManager _scoreManager;

    public override void Initialize(TilePlaceHandler tilePlaceHandler)
    {
        _scoreManager = ScoreManager.Instance;
        _scoreManager.RegisterScoreModifier(ModifyScore);
        
        base.Initialize(tilePlaceHandler);
    }

    protected override int ModifyScore(eTileEventType tileEventType, Tile tile, int baseScore)
    {
        // 라인클리어 때.. 그리고 골드 타일일 때
        if (tileEventType == eTileEventType.LineClear && tile.Data.Option == TileOption.Gold)
        {
            return baseScore + GameManager.Instance.PlayerStatus.CurrentCoins;
        }

        return baseScore;
    }
}
