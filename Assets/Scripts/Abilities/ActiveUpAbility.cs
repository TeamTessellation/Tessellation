using Core;
using Player;

namespace Abilities
{
    public class ActiveUpAbility : AbilityBase
    {
        private InputManager.eActiveItemType _activeItemType;
        private int _itemAmount;
        
        public override void Initialize(TilePlaceHandler tilePlaceHandler)
        {
            switch (DataSO.ItemType)
            {
                case eItemType.GetTilesetDelete:
                    _activeItemType = InputManager.eActiveItemType.Delete;
                    break;
                case eItemType.GetTilesetReroll:
                    _activeItemType = InputManager.eActiveItemType.Reroll;
                    break;
                case eItemType.GetRevert:
                    _activeItemType = InputManager.eActiveItemType.Revert;
                    break;
                case eItemType.GetTilesetRotate:
                    _activeItemType = InputManager.eActiveItemType.Rotate;
                    break;
                case eItemType.GetTilesetChangeOverwrite:
                    _activeItemType = InputManager.eActiveItemType.Overwrite;
                    break;
                case eItemType.GetTilesetCopy:
                    _activeItemType = InputManager.eActiveItemType.Add;
                    break;
                default:
                    _activeItemType = InputManager.eActiveItemType.None;
                    break;
            }

            _itemAmount = (int)DataSO.input[0];
            
            base.Initialize(tilePlaceHandler);
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
            
            playerStatus.inventory.SetActiveItem(InputManager.eActiveItemType.None, 0);
        }
    }
}
