using System;
using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;
using ExecEvents;
using SaveLoad;
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
public class ScoreManager : Singleton<ScoreManager>, ISaveTarget
{
    public override bool IsDontDestroyOnLoad => true;

    #region Events
    public class TotalScoreChangedEventArgs : ExecEventArgs<TotalScoreChangedEventArgs>
    {
        public int NewTotalScore { get; set; }
        public int PrevTotalScore { get; set; }
    }
    public class CurrentScoreChangedEventArgs : ExecEventArgs<CurrentScoreChangedEventArgs>
    {
        public int NewCurrentScore { get; set; }
    }
    public class ScoreResetEventArgs : ExecEventArgs<ScoreResetEventArgs>
    {
    }
    public class MultiplierAddedEventArgs : ExecEventArgs<MultiplierAddedEventArgs>
    {
        public float NewMultiplier { get; set; }
    }

    #endregion
    
    public delegate int TileScoreModifierDelegate(eTileEventType tileEventType, Tile tile, int baseScore);

    public List<TileScoreModifierDelegate> _tileScoreModifiers = new List<TileScoreModifierDelegate>();

    // === Properties ===
    // 여러 턴에 누적되어 최종 합산된 점수
    public int CurrentStageScore
    {
        get => GameManager.Instance.PlayerStatus.StageScore;
        private set => GameManager.Instance.PlayerStatus.StageScore = value;
    }
    // 현재 턴에 대한 총 점수
    public int TempScore
    {
        get => GameManager.Instance.PlayerStatus.StageTempScore;
        private set => GameManager.Instance.PlayerStatus.StageTempScore = value;
    }
    // 현재 옆에 뜨는 곱, CurrentScore와 같은 수명
    public float Multiplier { get; set; }

    public int TargetScore => StageManager.Instance.CurrentStage.StageTargetScore;
    
    // 체급에 대한 점수
    public enum ScoreValueType
    {
        BasePlaceScore, // 기본 칸당 배치 점수
        BaseLineClearScore, // 기본 칸당 라인클리어 점수
        BaseLineClearMultiple, // 기본 라인당 곱배수
        BaseBurstScore, // 기본 터질 때 점수
        BaseCoinTileValue, // 기본 골드타일 골드
    }

    private Dictionary<ScoreValueType, float> _scoreValues = new();
    public IReadOnlyDictionary<ScoreValueType, float> ScoreValues => _scoreValues;
    

    // === Functions ===
    // Initialize
    protected override void AfterAwake()
    {
        base.AfterAwake();
        Reset();
    }

    public void Reset()
    {
        TempScore = 0;
        CurrentStageScore = 0;
        Multiplier = 1;

        _scoreValues[ScoreValueType.BasePlaceScore] = 1;
        _scoreValues[ScoreValueType.BaseLineClearScore] = 5;
        _scoreValues[ScoreValueType.BaseLineClearMultiple] = 1;
        _scoreValues[ScoreValueType.BaseBurstScore] = 10;
        _scoreValues[ScoreValueType.BaseCoinTileValue] = 1;

        BroadCastScores();
        
        using var resetEvt = ScoreResetEventArgs.Get();
        ExecEventBus<ScoreResetEventArgs>.InvokeMerged(resetEvt).Forget(); 
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
        TempScore += addScore;
        using var currentEvt = CurrentScoreChangedEventArgs.Get();
        currentEvt.NewCurrentScore = TempScore;
        ExecEventBus<CurrentScoreChangedEventArgs>.InvokeMerged(currentEvt).Forget();
    }

    public void AddMultiplier(float addValue)
    {
        Multiplier += addValue;
        using var mulEvt = MultiplierAddedEventArgs.Get();
        mulEvt.NewMultiplier = Multiplier;
        ExecEventBus<MultiplierAddedEventArgs>.InvokeMerged(mulEvt).Forget();
    }
    
    // 타일이나 조커로 인한 곱 연산을 MultiplierStack에 추가할 때 사용하는 함수
    public void MultiplyMultiplier(float mulValue)
    {
        Multiplier *= mulValue;
        using var mulEvt = MultiplierAddedEventArgs.Get();
        mulEvt.NewMultiplier = Multiplier;
        ExecEventBus<MultiplierAddedEventArgs>.InvokeMerged(mulEvt).Forget();
    }
    
    public void BroadCastScores()
    {
        using var totalEvt = TotalScoreChangedEventArgs.Get();
        totalEvt.NewTotalScore = CurrentStageScore;
        ExecEventBus<TotalScoreChangedEventArgs>.InvokeMerged(totalEvt).Forget();    
        using var currentEvt = CurrentScoreChangedEventArgs.Get();
        currentEvt.NewCurrentScore = TempScore;
        ExecEventBus<CurrentScoreChangedEventArgs>.InvokeMerged(currentEvt).Forget(); 
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
        using var totalEvt = TotalScoreChangedEventArgs.Get();
        totalEvt.PrevTotalScore = CurrentStageScore;
        
        TempScore = (int)(TempScore * Multiplier);
        CurrentStageScore += TempScore; // StageManager에서 TotalScore을 갱신 하는 대신, 여기서 갱신하는중
        
        totalEvt.NewTotalScore = CurrentStageScore;
        
        await ExecEventBus<TotalScoreChangedEventArgs>.InvokeMerged(totalEvt);
        
        TempScore = 0;
        Multiplier = 1;
    }

    public Guid Guid { get; init; } = Guid.NewGuid();
    public void LoadData(GameData data)
    {
        // 어차피 PlayerStatus에서 불러오니까 여기선 할 필요 없음
        // this.CurrentScore = data.PlayerStatus.CurrentScore;
        // this.TotalScore = data.PlayerStatus.TotalScore;
    }

    public void SaveData(ref GameData data)
    {
        // data.PlayerStatus.CurrentScore = this.CurrentScore;
        // data.PlayerStatus.TotalScore = this.TotalScore;
    }
}
