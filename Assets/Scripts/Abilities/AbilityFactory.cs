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
            { eItemType.AddBombTileset,                   () => new OnlyPlaceAbility() },
            { eItemType.IncreaseExplosionRange,           () => new OnlyPlaceAbility() },
            { eItemType.BombImmediatelyExplosion,         () => new OnlyPlaceAbility() },
            { eItemType.ChainExplosion,                   () => new OnlyPlaceAbility() },
            { eItemType.AddExtraScoreTileset,             () => new OnlyPlaceAbility() },
            { eItemType.AddMultipleScoreTileset,          () => new OnlyPlaceAbility() },
            { eItemType.AddGoldTileset,                   () => new OnlyPlaceAbility() },
            { eItemType.GoldTilesetCoinScaledExtraScore,  () => new OnlyPlaceAbility() },
            { eItemType.AdditionalInterest,               () => new OnlyPlaceAbility() },
            { eItemType.CoinScaledMultiple,               () => new OnlyPlaceAbility() },
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