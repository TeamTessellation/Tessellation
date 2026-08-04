using System;

[Serializable]
public sealed class BombRules
{
    public const int BaseExplosionRadius = 1;

    public int RangeBonus { get; private set; }
    public int ImmediateExplosionCount { get; private set; }
    public int ChainExplosionCount { get; private set; }

    public int ExplosionRadius => BaseExplosionRadius + Math.Max(0, RangeBonus);
    public bool ExplodesImmediately => ImmediateExplosionCount > 0;
    public bool Chains => ChainExplosionCount > 0;

    public void Apply(Abilities.eItemType itemType, int rangeBonus)
    {
        switch (itemType)
        {
            case Abilities.eItemType.IncreaseExplosionRange:
                RangeBonus += Math.Max(0, rangeBonus);
                break;
            case Abilities.eItemType.BombImmediatelyExplosion:
                RangeBonus += Math.Max(0, rangeBonus);
                ImmediateExplosionCount++;
                break;
            case Abilities.eItemType.ChainExplosion:
                RangeBonus += Math.Max(0, rangeBonus);
                ChainExplosionCount++;
                break;
        }
    }

    public void Remove(Abilities.eItemType itemType, int rangeBonus)
    {
        switch (itemType)
        {
            case Abilities.eItemType.IncreaseExplosionRange:
                RangeBonus = Math.Max(0, RangeBonus - Math.Max(0, rangeBonus));
                break;
            case Abilities.eItemType.BombImmediatelyExplosion:
                RangeBonus = Math.Max(0, RangeBonus - Math.Max(0, rangeBonus));
                ImmediateExplosionCount = Math.Max(0, ImmediateExplosionCount - 1);
                break;
            case Abilities.eItemType.ChainExplosion:
                RangeBonus = Math.Max(0, RangeBonus - Math.Max(0, rangeBonus));
                ChainExplosionCount = Math.Max(0, ChainExplosionCount - 1);
                break;
        }
    }
}

namespace Abilities
{
    [Serializable]
    public sealed class BombModifierAbility : AbilityBase
    {
        [NonSerialized] private TilePlaceHandler _handler;
        private int _rangeBonus;

        public override void Initialize(TilePlaceHandler tilePlaceHandler)
        {
            _handler = tilePlaceHandler;
            _rangeBonus = DataSO.input != null && DataSO.input.Count > 0 ? (int)DataSO.input[0] : 0;
            base.Initialize(tilePlaceHandler);
        }

        protected override void OnAbilityApplied()
        {
            _handler.BombRules.Apply(DataSO.ItemType, _rangeBonus);
            base.OnAbilityApplied();
        }

        protected override void OnAbilityRemoved()
        {
            _handler?.BombRules.Remove(DataSO.ItemType, _rangeBonus);
            base.OnAbilityRemoved();
        }
    }
}
