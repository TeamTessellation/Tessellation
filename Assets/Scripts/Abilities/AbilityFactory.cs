using System;
using System.Collections.Generic;
using Machamy.Utils;
using UnityEngine;

namespace Abilities
{
    public static class AbilityFactory
    {
        private static Dictionary<eItemType, Func<AbilityBase>> abilityFuncs = new()
        {
            { eItemType.ExtraTurn,                        () => new ExtraTurnAbility() },
            { eItemType.ExtraActiveItemRemainingUses,     () => new ExtraActiveItemAbility() },
            { eItemType.AddBombTileset,                   () => new AdditionalTileAbility() },
            { eItemType.IncreaseExplosionRange,           () => new OnlyPlaceAbility() }, // None
            { eItemType.BombImmediatelyExplosion,         () => new OnlyPlaceAbility() }, // None
            { eItemType.ChainExplosion,                   () => new OnlyPlaceAbility() }, // None
            { eItemType.AddExtraScoreTileset,             () => new AdditionalTileAbility() },
            { eItemType.AddMultipleScoreTileset,          () => new AdditionalTileAbility() },
            { eItemType.AddGoldTileset,                   () => new AdditionalTileAbility() },
            { eItemType.GoldTilesetCoinScaledExtraScore,  () => new GoldToGoldAbility() },
            { eItemType.AdditionalInterest,               () => new OnlyPlaceAbility() }, // None
            { eItemType.CoinScaledMultiple,               () => new OnlyPlaceAbility() }, // None
            { eItemType.GetTilesetDelete,                 () => new ActiveUpAbility() },
            { eItemType.GetTilesetReroll,                 () => new ActiveUpAbility() },
            { eItemType.GetRevert,                        () => new ActiveUpAbility() },
            { eItemType.GetTilesetRotate,                 () => new ActiveUpAbility() },
            { eItemType.GetTilesetChangeOverwrite,        () => new ActiveUpAbility() },
            { eItemType.GetTilesetCopy,                   () => new ActiveUpAbility() },
        };
        
        public static AbilityBase Create(AbilityDataSO abilitydata)
        {
            if (!abilityFuncs.ContainsKey(abilitydata.ItemType)) return null;

            AbilityBase newAbility = abilityFuncs[abilitydata.ItemType]();
            newAbility.InitializeData(abilitydata);
            
            return newAbility;
        }
    }
}