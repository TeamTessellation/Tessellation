using Abilities;
using Core;
using Player;
using UnityEngine;

public class ExtraTurnAbility : AbilityBase
{
    private int _extraTurn;

    public override void Initialize(TilePlaceHandler tilePlaceHandler)
    {
        _extraTurn = (int)DataSO.input[0];

        for (int i = 0; i < DataSO.input.Count; i++)
        {
            Debug.Log($"Input[{i}] : {DataSO.input[i]}");
        }
        
        base.Initialize(tilePlaceHandler);
    }
    

    protected override void OnAbilityApplied()
    {
        base.OnAbilityApplied();
        

        PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;

        Debug.Log($"{_extraTurn}");
        
        playerStatus.CurrentExtraTurns = _extraTurn;
        
        Debug.Log($"{playerStatus.CurrentExtraTurns}");
    }

    protected override void OnAbilityRemoved()
    {
        base.OnAbilityRemoved();
        
        PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;

        playerStatus.CurrentExtraTurns = 0;
    }
}
