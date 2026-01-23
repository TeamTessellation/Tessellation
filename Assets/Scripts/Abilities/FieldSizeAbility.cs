using Abilities;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FieldSizeAbility : AbilityBase
{

    private int _size;
    private float _scale;

    public override void Initialize(TilePlaceHandler tilePlaceHandler)
    {
        _size = (int)DataSO.input[0];
        _scale = DataSO.input[1];
        Field.Instance.SetField(Field.Instance.Size - _size);

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
        ScoreManager.Instance.MultiplyMultiplier(_scale);
    }
}
