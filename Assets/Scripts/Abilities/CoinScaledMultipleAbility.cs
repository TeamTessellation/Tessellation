using Abilities;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CoinScaledMultipleAbility : AbilityBase
{

    private int _scaledCoin;
    private float _scale;

    public override void Initialize(TilePlaceHandler tilePlaceHandler)
    {
        _scaledCoin = (int)DataSO.input[0];
        _scale = DataSO.input[1];
        
        base.Initialize(tilePlaceHandler);
    }

    protected override bool CheckCanActivate(TurnResultInfo info)
    {
        return true;
    }

    protected override async UniTask HandleTilePlacedAsync(TurnResultInfo info)
    {
        if (CheckCanActivate(info))
        {
            await Activate(info);
        }
    }
    
    protected override async UniTask HandleLineClearedAsync(TurnResultInfo info)
    {
        if (CheckCanActivate(info))
        {
            await Activate(info);
        }
    }
    
    protected override async UniTask HandleTileRemovedAsync(TurnResultInfo info)
    {
        if (CheckCanActivate(info))
        {
            await Activate(info);
        }
    }
    
    protected override async UniTask HandleTileBurstAsync(TurnResultInfo info)
    {
        if (CheckCanActivate(info))
        {
            await Activate(info);
        }
    }

    protected override async UniTask Activate(TurnResultInfo info)
    {
        int currentCoin = GameManager.Instance.PlayerStatus.CurrentCoins;

        int factor = currentCoin / _scaledCoin;
        
        ScoreManager.Instance.MultiplyMultiplier(factor * _scale);
    }
}
