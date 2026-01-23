using Abilities;
using Core;
using Cysharp.Threading.Tasks;
using Stage;
using UnityEngine;

public class FirstTurnMultipleAbility : AbilityBase
{
    private float _scale;

    public override void Initialize(TilePlaceHandler tilePlaceHandler)
    {
        _scale = DataSO.input[0];
        base.Initialize(tilePlaceHandler);
    }

    protected override bool CheckCanActivate(TurnResultInfo info)
    {
        return (TurnManager.Instance.CurrentTurn == 0);
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
        ScoreManager.Instance.MultiplyMultiplier(_scale);
    }
}
