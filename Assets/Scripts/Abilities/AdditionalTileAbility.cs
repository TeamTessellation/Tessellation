using Abilities;
using UnityEngine;

public class AdditionalTileAbility : AbilityBase
{
    private TileOption _tileOption;
    private int _tileCount;
    
    public override void Initialize(TilePlaceHandler tilePlaceHandler)
    {
        switch (DataSO.ItemType)
        {
            case eItemType.AddGoldTileset:
                _tileOption = TileOption.Gold;
                break;
            case eItemType.AddBombTileset:
                _tileOption = TileOption.Boom;
                break;
            case eItemType.AddExtraScoreTileset:
                _tileOption = TileOption.Bonus;
                break;
            case eItemType.AddMultipleScoreTileset:
                _tileOption = TileOption.Double;
                break;
        }

        _tileCount = (int)DataSO.input[0];
        
        base.Initialize(tilePlaceHandler);
    }

    protected override void OnAbilityApplied()
    {
        base.OnAbilityApplied();
        
        HandManager.Instance.AddOptionalDeck(_tileOption, _tileCount);
    }

    protected override void OnAbilityRemoved()
    {
        base.OnAbilityRemoved();
        
        HandManager.Instance.RemoveOptionalDeck(_tileOption, _tileCount);
    }
}
