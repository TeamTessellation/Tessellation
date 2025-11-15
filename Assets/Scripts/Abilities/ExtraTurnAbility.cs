using Abilities;
using UnityEngine;

public class ExtraTurnAbility : AbilityBase
{
    private int _extraTurn;


    public override void Initialize(TilePlaceHandler tilePlaceHandler)
    {
        base.Initialize(tilePlaceHandler);

        switch (DataSO.Rarity)
        {
            
        }
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
