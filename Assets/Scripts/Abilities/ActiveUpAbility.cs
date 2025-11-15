using Core;
using Player;

namespace Abilities
{
    public class ActiveUpAbility : AbilityBase
    {
        private InputManager.Item _activeItemType;
        private int _itemAmount;
        
        public ActiveUpAbility(InputManager.Item activeItemType, int itemAmount)
        {
            _activeItemType = activeItemType;
            _itemAmount = itemAmount;
        }
        
        protected override void OnAbilityApplied()
        {
            base.OnAbilityApplied();
            
            PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;

            playerStatus.inventory.SetActiveItem(_activeItemType, _itemAmount);
        }

        protected override void OnAbilityRemoved()
        {
            base.OnAbilityRemoved();

            PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;
            
            playerStatus.inventory.SetActiveItem(InputManager.Item.None, 0);
        }
    }
}
