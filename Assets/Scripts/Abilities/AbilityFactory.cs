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
            { eAbilityType.OnlyPlace , () => new OnlyPlaceAbility() },
            // Add..            
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