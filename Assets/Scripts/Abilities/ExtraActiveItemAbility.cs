using Abilities;
using Core;
using Player;
using UnityEngine;

public class ExtraActiveItemAbility : AbilityBase
{
    private int _extraItemCount;
    
    public override void Initialize(TilePlaceHandler tilePlaceHandler)
    {
        base.Initialize(tilePlaceHandler);

        _extraItemCount = (int)DataSO.input[0];
    }
    

    protected override void OnAbilityApplied()
    {
        base.OnAbilityApplied();

        PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;

        int currentMaxItemCount = playerStatus.inventory.MaxItemCount;
        playerStatus.inventory.SetActiveItemCount(currentMaxItemCount + _extraItemCount);
    }

    protected override void OnAbilityRemoved()
    {
        base.OnAbilityRemoved();
        
        PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;

        int currentMaxItemCount = playerStatus.inventory.MaxItemCount;
        playerStatus.inventory.SetActiveItemCount(currentMaxItemCount - _extraItemCount);
    }
}
