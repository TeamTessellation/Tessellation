using Abilities;
using Core;
using UnityEngine;

public class ExtraTurnAbility : AbilityBase
{
    private int _extraTurn;

    public override void Initialize(TilePlaceHandler tilePlaceHandler)
    {
        base.Initialize(tilePlaceHandler);

        _extraTurn = (int)DataSO.input[0];
    }
    

    protected override void OnAbilityApplied()
    {
        base.OnAbilityApplied();
    }

    protected override void OnAbilityRemoved()
    {
        base.OnAbilityRemoved();
    }
}
