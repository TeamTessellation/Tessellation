using Machamy.Utils;

namespace Abilities
{
    public static class AbilityFactory
    {
        public static AbilityBase Create(eAbilityType abilityType)
        {
            AbilityBase newAbility;
            
            switch (abilityType)
            {
                case eAbilityType.OnlyPlace:
                {
                    newAbility = new OnlyPlaceAbility();
                    break;
                }
                case eAbilityType.WomboCombo:
                    LogEx.LogWarning($"미구현 Ability: {abilityType}");
                    return null;
            
                case eAbilityType.LineClear:
                    LogEx.LogWarning($"미구현 Ability: {abilityType}");
                    return null;
            
                default:
                    LogEx.LogError($"알 수 없는 Ability 타입: {abilityType}");
                    return null;
            }

            newAbility.AbilityType = abilityType;
            return newAbility;
        }
    }
}