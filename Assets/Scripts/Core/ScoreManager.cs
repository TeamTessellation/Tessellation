using System;
using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ScoreManager : Singleton<ScoreManager>
{
    public override bool IsDontDestroyOnLoad => true;

    // === Events ===
    // 총점이 변경될 때 호출 (UI 갱신 등)
    public event Action<int> OnCurrentScoreChanged;
    // 새로운 게임이 시작되어 점수 초기화될 때 호출
    public event Action OnScoreReset;
    // 곱 배수가 스택에 추가될 때 호출
    public event Action<float> OnMultiplierAdded;
    
    // === Properties ===
    public int TotalScore { get; private set; }
    public int CurrentScore { get; private set; }
    public int BaseScore { get; private set; }
    public IReadOnlyList<float> MultiplierStack => _multiplierStack;
    
    private List<float> _multiplierStack = new List<float>();

    // === Functions ===
    // Initialize
    protected override void AfterAwake()
    {
        base.AfterAwake();
        Reset();
    }

    public void Reset()
    {
        BaseScore = 0;
        CurrentScore = 0;
        _multiplierStack.Clear();
        OnScoreReset?.Invoke();
    }

    // 타일 배치 등 기본 점수 더할때 사용하는 함수
    public void AddBaseScore(int addScore)
    {
        BaseScore += addScore;
        TotalScore += addScore;
        OnCurrentScoreChanged?.Invoke(TotalScore);
    }

    // 곱타일이나 조커로 인한 곱셈 추가 등 배수를 추가할 때 사용하는 함수
    public void AddMultiplier(float mulValue)
    {
        _multiplierStack.Add(mulValue);
        OnMultiplierAdded?.Invoke(mulValue);
    }

    // 턴 끝나고 
    public async UniTask FinalizeScore()
    {
        int baseScore = BaseScore;

        if (_multiplierStack.Count == 0) return;

        float accumulatedScore = baseScore;

        foreach (float multiplier in _multiplierStack)
        {
            // TODO
            // EffectHandler에게 요청, 이펙트 재생 및 대기한다.
            // await EffectHandler.PlayMultiplierEffect(multiplier)
            
            accumulatedScore *= multiplier;
            OnCurrentScoreChanged?.Invoke(TotalScore);
        }
        _multiplierStack.Clear();
        
        // TODO
        // EffectHandler 요청
        // await EffectHandler.PlayAddTotalScoreEffect()
        CurrentScore = 0;
    }
}
