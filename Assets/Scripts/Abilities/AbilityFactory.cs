using System;
using System.Collections.Generic;
using Machamy.Utils;
using UnityEngine;

namespace Abilities
{
    public static class AbilityFactory
    {
        private static Dictionary<eAbilityType, Func<AbilityBase>> abiltyFuncs = new()
        {
            { eAbilityType.ExtraLife, () => new OnlyPlaceAbility() },
            { eAbilityType.ExtraActiveItemRemainingUses, () => new OnlyPlaceAbility() },
            { eAbilityType.AddBombTileset, () => new OnlyPlaceAbility() },
            { eAbilityType.IncreaseExplosionRange, () => new OnlyPlaceAbility() },
            { eAbilityType.BombImmediatelyExplosion, () => new OnlyPlaceAbility() },
            { eAbilityType.ChainExplosion, () => new OnlyPlaceAbility() },
            { eAbilityType.AddExtraScoreTileset, () => new OnlyPlaceAbility() },
            { eAbilityType.AddMultipleTileset, () => new OnlyPlaceAbility() },
            { eAbilityType.AddGoldTileset, () => new OnlyPlaceAbility() },
            { eAbilityType.GoldTilesetCoinScaledExtraScore, () => new OnlyPlaceAbility() },
            { eAbilityType.AdditionalInterest, () => new OnlyPlaceAbility() },
            { eAbilityType.CoinScaledMultiple, () => new OnlyPlaceAbility() },
        };
        
        public static AbilityBase Create(AbilityDataSO abilitydata)
        {
            if (!abiltyFuncs.ContainsKey(abilitydata.AbilityType)) return null;

            AbilityBase newAbility = abiltyFuncs[abilitydata.AbilityType]();
            newAbility.InitializeData(abilitydata);
            
            return newAbility;
        }
    }
}