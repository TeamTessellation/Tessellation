using Abilities;
using Core;
using Player;
using UnityEngine;

public class AdditionalInterestAbility : AbilityBase
{
    private int _maxInterest;

    public override void Initialize(TilePlaceHandler tilePlaceHandler)
    {
        _maxInterest = (int)DataSO.input[0];
        
        base.Initialize(tilePlaceHandler);
    }

    protected override void OnAbilityApplied()
    {
        base.OnAbilityApplied();

        PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;
        
        playerStatus.CurrentExtraInterestMaxCoins = _maxInterest - 5;
    }

    protected override void OnAbilityRemoved()
    {
        base.OnAbilityRemoved();
        
        PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;
        
        playerStatus.CurrentExtraInterestMaxCoins = 5;
    }
}
