using System;
using System.Collections.Generic;
using Machamy.Utils;
using UnityEngine;

namespace Abilities
{
    public static class AbilityFactory
    {
        private static Dictionary<eItemType, Func<AbilityBase>> abiltyFuncs = new()
        {
            { eItemType.ExtraTurn, () => new OnlyPlaceAbility() },
            { eItemType.ExtraActiveItemRemainingUses, () => new OnlyPlaceAbility() },
            { eItemType.AddBombTileset, () => new OnlyPlaceAbility() },
            { eItemType.IncreaseExplosionRange, () => new OnlyPlaceAbility() },
            { eItemType.BombImmediatelyExplosion, () => new OnlyPlaceAbility() },
            { eItemType.ChainExplosion, () => new OnlyPlaceAbility() },
            { eItemType.AddExtraScoreTileset, () => new OnlyPlaceAbility() },
            { eItemType.AddMultipleTileset, () => new OnlyPlaceAbility() },
            { eItemType.AddGoldTileset, () => new OnlyPlaceAbility() },
            { eItemType.GoldTilesetCoinScaledExtraScore, () => new OnlyPlaceAbility() },
            { eItemType.AdditionalInterest, () => new OnlyPlaceAbility() },
            { eItemType.CoinScaledMultiple, () => new OnlyPlaceAbility() },
        };
        
        public static AbilityBase Create(AbilityDataSO abilitydata)
        {
            if (!abiltyFuncs.ContainsKey(abilitydata.ItemType)) return null;

            AbilityBase newAbility = abiltyFuncs[abilitydata.ItemType]();
            newAbility.InitializeData(abilitydata);
            
            return newAbility;
        }
    }
}