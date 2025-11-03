

using Cysharp.Threading.Tasks;
using Machamy.Utils;
using Unity.VisualScripting;
using UnityEngine;

public class OnlyPlaceAbility : AbilityBase
{
    private ScoreManager _scoreManager;
    
    private float _multiplier = 5.0f;

    protected void Start()
    {
        base.Start();
        
        _scoreManager = ScoreManager.Instance;
    }

    protected override async UniTask HandleTurnProcessedAsync(TurnResultInfo info)
    {
        if (_scoreManager == null) return;

        if (CheckCanActivate(info))
        {
            Activate();
        }
    }

    protected override bool CheckCanActivate(TurnResultInfo info)
    {
        if (!base.CheckCanActivate(info)) return false;

        bool isLineCleared = info.ClearedLineCount == 0 ? false : true;

        if (!isLineCleared)
        {
            LogEx.LogWarning("순수 배치 턴 확인");
            return true;
        }
        
        return false;
    }

    protected override async UniTask Activate()
    {
        if (_scoreManager == null) return;
        
        _scoreManager.AddMultiplier(_multiplier);
    }
}
