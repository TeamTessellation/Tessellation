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
        public static AbilityBase Create(eAbilityType abilityType)
        {
            AbilityDataSO abilityDataSo = Resources.Load<AbilityDataSO>($"Prefabs/Ability/{abilityType.ToString()}");
            if (!abilityDataSo)
            {
                LogEx.LogWarning($"미구현 Ability : {abilityType}");
                return null;
            }
            // //
            // // if(!abilityda)
            // // AbilityBase newAbility;
            // // switch (abilityType)
            // // {
            // //     case eAbilityType.OnlyPlace:
            // //     {
            // //         newAbility = new OnlyPlaceAbility();
            // //         break;
            // //     }
            // }

            return null;
        }
    }
}