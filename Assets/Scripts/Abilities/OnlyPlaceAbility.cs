using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Abilities
{
    public class OnlyPlaceAbility : AbilityBase
    {
        private ScoreManager _scoreManager;
    
        private float _multiplier = 5.0f;


        public override void Initialize(TilePlaceHandler tilePlaceHandler)
        {
            base.Initialize(tilePlaceHandler);

            _scoreManager = ScoreManager.Instance;
            _scoreManager.RegisterScoreModifier(ModifyScore);
        }

        protected override async UniTask HandleTurnProcessedAsync(TurnResultInfo info)
        {
            if (_scoreManager == null) return;

            if (CheckCanActivate(info))
            {
                await Activate(info);
            }
        }

        protected override int ModifyScore(eTileEventType tileEventType, Tile tile, int baseScore)
        {
            if (tileEventType == eTileEventType.Place)
            {
                return baseScore * 2;
            }
            return baseScore;
        }

        protected override bool CheckCanActivate(TurnResultInfo info)
        {
            bool isLineCleared = info.ClearedLineCount == 0 ? false : true;

            if (!isLineCleared)
            {
                return true;
            }
            return false;
        }

        protected override async UniTask Activate(TurnResultInfo info)
        {
            if (_scoreManager == null) return;
        
            Debug.Log($"실행, Priority : {AbilityPriority}");
            _scoreManager.AddMultiplier(_multiplier);
        }
    }
}
