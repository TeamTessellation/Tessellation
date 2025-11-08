using System;
using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;
using Stage;
using UnityEngine;

/* ================================================================================
 * 플레이어가 보는 점수에는 2가지 종류의 점수가 있다.
 * 현재 턴에 대해 누적되는 점수(CurrentScore), 그리고 여러 턴에 걸쳐 누적된 총합 점수(TotalScore).
 * CurrentScore : 누적되는 점수는 타일 배치나 폭파, 고정 점수 추가등으로 얻는 점수 
 * MultiplierStack : 조커 효과나 타일로 인한 곱 연산이 들어올 때 미리 채워넣는 스택
 *
 * CurrentScore을 우선적으로 계산한 후, 마지막에 MultiplierStack을 하나씩 비워가며 CurrentScore을 증가시킨다.
 *
 * 점수의 증가는 게임 상 이펙트 효과를 동반하기 때문에 중간중간 이벤트를 보내고 대기한다.
 ================================================================================ */
public class ScoreManager : Singleton<ScoreManager>
{
    public override bool IsDontDestroyOnLoad => true;

    // === Events ===
    // 총 점수가 변경될 때 호출
    public event Func<int, UniTask> OnTotalScoreChangedAsync;
    // 현재 점수가 변경될 때 호출 (UI 갱신 등)
    public event Func<int, UniTask> OnCurrentScoreChangedAsync;
    // 새로운 게임이 시작되어 점수 초기화될 때 호출
    public event Func<UniTask> OnScoreResetAsync;
    // 곱 배수가 스택에 추가될 때 호출
    public event Func<float, UniTask> OnMultiplierAddedAsync;

    public delegate int TileScoreModifierDelegate(eTileEventType tileEventType, Tile tile, int baseScore);

    public List<TileScoreModifierDelegate> _tileScoreModifiers = new List<TileScoreModifierDelegate>();
    
    // === Properties ===
    // 여러 턴에 누적되어 최종 합산된 점수
    public int TotalScore { get; private set; }
    // 현재 턴에 대한 총 점수
    public int CurrentScore { get; private set; }
    
    // 현재 점수 옆에 곱셈 표시 뜨는거 저장하는 리스트
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
        CurrentScore = 0;
        TotalScore = 0;
        _multiplierStack.Clear();
        OnScoreResetAsync?.Invoke();
    }

    public void RegisterScoreModifier(TileScoreModifierDelegate modifier)
    {
        if (!_tileScoreModifiers.Contains(modifier))
        {
            _tileScoreModifiers.Add(modifier);
        }
    }

    public void UnRegisterScoreModifier(TileScoreModifierDelegate modifier)
    {
        if (!_tileScoreModifiers.Contains(modifier))
        {
            _tileScoreModifiers.Remove(modifier);
        }
    }
    
    // 더하기 연산을 CurrentStack에 적용
    public void AddCurrentScore(int addScore)
    {
        CurrentScore += addScore;
        OnCurrentScoreChangedAsync?.Invoke(CurrentScore);
    }

    // 타일이나 조커로 인한 곱 연산을 MultiplierStack에 추가할 때 사용하는 함수
    public void AddMultiplier(float mulValue)
    {
        _multiplierStack.Add(mulValue);
        OnMultiplierAddedAsync?.Invoke(mulValue);
    }

    public int CalculateTileScore(eTileEventType tileEventType, Tile tile, int baseScore)
    {
        int finalScore = baseScore;
        foreach (var modifier in _tileScoreModifiers)
        {
            finalScore = modifier.Invoke(tileEventType, tile, baseScore);
        }

        return finalScore;
    }

    // CurrentScore에 곱계산을 추가하여 TotalScore에 더한다
    public async UniTask FinalizeScore()
    {
        if (_multiplierStack.Count == 0) return;

        float accumulatedScore = CurrentScore;

        foreach (float multiplier in _multiplierStack)
        {
            // TODO
            // EffectHandler에게 요청, 이펙트 재생 및 대기한다.
            // await EffectHandler.PlayMultiplierEffect(multiplier)
            
            accumulatedScore *= multiplier;
            CurrentScore = (int)accumulatedScore;
            OnCurrentScoreChangedAsync?.Invoke(CurrentScore);
        }
        _multiplierStack.Clear();
        
        // TODO
        // EffectHandler 요청
        // await EffectHandler.PlayAddTotalScoreEffect()
        TotalScore += CurrentScore;
        CurrentScore = 0;
        
        OnTotalScoreChangedAsync?.Invoke(TotalScore);
    }
}
