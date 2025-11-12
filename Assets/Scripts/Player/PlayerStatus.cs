using System;
using Core;
using Cysharp.Threading.Tasks;
using ExecEvents;
using SaveLoad;
using UnityEngine;
// ReSharper disable ArrangeAccessorOwnerBody
namespace Player
{
    /// <summary>
    /// 플레이어의 상태(스코어, 코인 등)를 관리하는 클래스입니다.
    /// - 내부 데이터는 <see cref="VariableContainer"/>에 저장됩니다.
    /// - 외부 접근/수정은 반드시 이 클래스의 Property를 통해 이루어져야 합니다. (이벤트 관리/변경 감지를 위해)
    ///
    /// 접두사 규칙:
    /// - Total : 해당 게임을 진행하면서 누적된 합계 값
    /// - Best  : 해당 게임에서의 최고값
    /// - BestStage : 진행한 여러 스테이지 중 최고값
    /// - Stage : 현재 플레이 중인 스테이지의 값
    /// - Current : 현재 실시간 값
    /// </summary>
    [Serializable]
    public class PlayerStatus : ISaveTarget
    {
        public static PlayerStatus Current => GameManager.Instance.PlayerStatus;
        
        [SerializeField] private int fieldSize = 4;
        [SerializeField] private int handSize = 3;
        
        [SerializeField] private float coinInterestRate = 0.1f; // 코인 이자율 10%
        [SerializeField] private int maxInterestCoins = 5; // 최대 이자 코인 한도
        
        [SerializeField]
        private VariableContainer variables = new VariableContainer();

        /// <summary>
        /// 내부 VariableContainer 인스턴스입니다. 외부에서 읽을 수 있고 설정은 이 클래스 내부에서만 가능합니다.
        /// Variable에 접근/수정할 때는 이 프로퍼티를 사용하세요.
        /// </summary>
        public VariableContainer Variables
        {
            get => variables;
            private set => variables = value;
        }
        public void Reset()
        {
            fieldSize = 4;
            handSize = 3;
            Variables = new VariableContainer();
            foreach (VariableKey key in Enum.GetValues(typeof(VariableKey)))
            {
                Variables.SetInteger(key.ToString(), 0);
            }
        }
        
        public enum VariableKey
        {
            BestScorePlacement,// 최고 득점 배치
            BestStageScore,// 최고 스테이지 점수
            BestStageClearedLines,// 최고 스테이지 지운 줄 수
            BestStageAbilityUseCount,// 최고 스테이지 능력 사용 횟수
            BestStageCoinsObtained,// 최고 스테이지 획득 코인
            BestStageInterestEarnedCoins,// 최고 스테이지 이자 코인
            
            TotalScore,// 총 점수
            TotalClearedLines,// 총 지운 줄 수
            TotalAbilityUseCount,// 능력 사용 횟수
            TotalObtainedCoins,// 지금까지 얻었던 코인
            TotalInterestEarnedCoins,// 총 얻었던 이자 코인
            
            StageBestPlacement,// 해당 스테이지 최고 배치
            StageScore,// 해당 스테이지 점수
            StageClearedLines,// 해당 스테이지 지운 줄 수
            StageAbilityUseCount,// 해당 스테이지 능력 사용 횟수
            StageCoinsObtained,// 해당 스테이지 획득 코인
            StageInterestEarnedCoins,// 해당 스테이지 이자
            
            CurrentTurn,// 현재 턴
            CurrentRemainingTurns,// 현재 남은 턴
            CurrentCoins,// 현재 코인
            
            
        }
        public enum eActiveItemType
        {
            
        }
        // for 문 사용 편의성
        public static readonly VariableKey BestStart = VariableKey.BestScorePlacement;
        public static readonly VariableKey BestEnd = VariableKey.BestStageInterestEarnedCoins;
        public static readonly VariableKey TotalStart = VariableKey.TotalScore;
        public static readonly VariableKey TotalEnd = VariableKey.TotalInterestEarnedCoins;
        public static readonly VariableKey StageStart = VariableKey.StageBestPlacement;
        public static readonly VariableKey StageEnd = VariableKey.StageInterestEarnedCoins;

        
        
        public int FieldSize => fieldSize;
        public int HandSize => handSize;
        
        public float CoinInterestRate => coinInterestRate;
        public int MaxInterestCoins => maxInterestCoins;

        // --- PlayerStatus Properties (VariableKey에 매핑된 프로퍼티들) ---

        /// <summary>
        /// 현재 스테이지의 점수입니다. (Prefix: Stage)
        /// 사용/수정은 이 프로퍼티를 통해 수행하세요.
        /// </summary>
        public int CurrentStageScore
        {
            get { return Variables.GetVariable(nameof(VariableKey.StageScore)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.StageScore), value); }
        }

        /// <summary>
        /// 해당 스테이지에서 기록한 최고 배치(포지션)입니다. (Prefix: Stage)
        /// </summary>
        public int StageBestPlacement
        {
            get { return Variables.GetVariable(nameof(VariableKey.StageBestPlacement)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.StageBestPlacement), value); }
        }

        /// <summary>
        /// 현재 스테이지에서 지운 줄 수입니다. (Prefix: Stage)
        /// </summary>
        public int StageClearedLines
        {
            get { return Variables.GetVariable(nameof(VariableKey.StageClearedLines)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.StageClearedLines), value); }
        }

        /// <summary>
        /// 현재 스테이지에서 사용한 능력(스킬) 횟수입니다. (Prefix: Stage)
        /// </summary>
        public int StageAbilityUseCount
        {
            get { return Variables.GetVariable(nameof(VariableKey.StageAbilityUseCount)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.StageAbilityUseCount), value); }
        }

        /// <summary>
        /// 현재 스테이지에서 획득한 코인량입니다. (Prefix: Stage)
        /// </summary>
        public int StageCoinsObtained
        {
            get { return Variables.GetVariable(nameof(VariableKey.StageCoinsObtained)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.StageCoinsObtained), value); }
        }

        /// <summary>
        /// 현재 스테이지에서 이자로 얻은 코인량입니다. (Prefix: Stage)
        /// </summary>
        public int StageInterestEarnedCoins
        {
            get { return Variables.GetVariable(nameof(VariableKey.StageInterestEarnedCoins)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.StageInterestEarnedCoins), value); }
        }

        // --- Total prefix (게임 단위 누적 값) ---

        /// <summary>
        /// 해당 게임 진행 중 누적된 총 점수입니다. (Prefix: Total)
        /// </summary>
        public int TotalScore
        {
            get { return Variables.GetVariable(nameof(VariableKey.TotalScore)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.TotalScore), value); }
        }

        /// <summary>
        /// 해당 게임 진행 중 누적된 총 지운 줄 수입니다. (Prefix: Total)
        /// </summary>
        public int TotalClearedLines
        {
            get { return Variables.GetVariable(nameof(VariableKey.TotalClearedLines)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.TotalClearedLines), value); }
        }

        /// <summary>
        /// 해당 게임 진행 중 누적된 능력 사용 횟수입니다. (Prefix: Total)
        /// </summary>
        public int TotalAbilityUseCount
        {
            get { return Variables.GetVariable(nameof(VariableKey.TotalAbilityUseCount)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.TotalAbilityUseCount), value); }
        }

        /// <summary>
        /// 지금까지 획득한 총 코인량입니다. (Prefix: Total)
        /// </summary>
        public int TotalObtainedCoins
        {
            get { return Variables.GetVariable(nameof(VariableKey.TotalObtainedCoins)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.TotalObtainedCoins), value); }
        }

        /// <summary>
        /// 지금까지 이자로 얻은 총 코인량입니다. (Prefix: Total)
        /// </summary>
        public int TotalInterestEarnedCoins
        {
            get { return Variables.GetVariable(nameof(VariableKey.TotalInterestEarnedCoins)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.TotalInterestEarnedCoins), value); }
        }

        // --- BestStage / Best prefix (최고값 관련) ---

        /// <summary>
        /// 해당 게임 중 기록된 최고 스테이지 점수입니다. (Prefix: BestStage)
        /// </summary>
        public int BestStageScore
        {
            get { return Variables.GetVariable(nameof(VariableKey.BestStageScore)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.BestStageScore), value); }
        }

        /// <summary>
        /// 해당 게임에서 기록한 최고 득점 배치입니다. (Prefix: Best)
        /// </summary>
        public int BestScorePlacement
        {
            get { return Variables.GetVariable(nameof(VariableKey.BestScorePlacement)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.BestScorePlacement), value); }
        }

        /// <summary>
        /// 해당 게임에서 기록한 최고 스테이지에서의 지운 줄 수입니다. (Prefix: BestStage)
        /// </summary>
        public int BestStageClearedLines
        {
            get { return Variables.GetVariable(nameof(VariableKey.BestStageClearedLines)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.BestStageClearedLines), value); }
        }

        /// <summary>
        /// 해당 게임에서 기록한 최고 스테이지의 능력 사용 횟수입니다. (Prefix: BestStage)
        /// </summary>
        public int BestStageAbilityUseCount
        {
            get { return Variables.GetVariable(nameof(VariableKey.BestStageAbilityUseCount)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.BestStageAbilityUseCount), value); }
        }

        /// <summary>
        /// 해당 게임에서 단일 스테이지 기준으로 가장 많이 획득한 코인량입니다. (Prefix: BestStage)
        /// </summary>
        public int BestStageCoinsObtained
        {
            get { return Variables.GetVariable(nameof(VariableKey.BestStageCoinsObtained)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.BestStageCoinsObtained), value); }
        }

        /// <summary>
        /// 해당 게임에서 단일 스테이지 기준으로 가장 많이 이자로 얻은 코인량입니다. (Prefix: BestStage)
        /// </summary>
        public int BestStageInterestEarnedCoins
        {
            get { return Variables.GetVariable(nameof(VariableKey.BestStageInterestEarnedCoins)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.BestStageInterestEarnedCoins), value); }
        }

        // --- Current prefix (실시간 값) ---

        /// <summary>
        /// 현재 플레이어가 보유한 코인 (실시간 값). (Prefix: Current)
        /// NOTE: 현재는 VariableContainer에 저장되지만, 필요하면 내부 필드로 이동시킬 수 있습니다.
        /// </summary>
        public int CurrentCoins
        {
            get { return Variables.GetVariable(nameof(VariableKey.CurrentCoins)).IntValue; }
            set
            {
                int oldValue = CurrentCoins;
                Variables.SetInteger(nameof(VariableKey.CurrentCoins), value);
                using var evt = CurrentCoinChangedEventArgs.Get();
                evt.OldCurrentCoin = oldValue;
                evt.NewCurrentCoin = value;
                ExecEventBus<CurrentCoinChangedEventArgs>.InvokeMerged(evt).Forget();
            }
        }

        public int CurrentTurn
        {
            get { return Variables.GetVariable(nameof(VariableKey.CurrentTurn)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.CurrentTurn), value); }
        }

        public int RemainingTurns
        {
            get { return Variables.GetVariable(nameof(VariableKey.CurrentRemainingTurns)).IntValue; }
            set { Variables.SetInteger(nameof(VariableKey.CurrentRemainingTurns), value); }
        }
        // => TurnManager.Instance.RemainingTurns;

        public Guid Guid { get; init; }
        public void LoadData(GameData data)
        {

            data.PlayerStatus.CopyTo(this);
            using var evt = ScoreManager.CurrentScoreChangedEventArgs.Get();
            evt.NewCurrentScore = this.CurrentStageScore;    
            ExecEventBus<ScoreManager.CurrentScoreChangedEventArgs>.InvokeMerged(evt).Forget();
            using var totalEvt = ScoreManager.TotalScoreChangedEventArgs.Get();
            totalEvt.NewTotalScore = this.TotalScore;
            ExecEventBus<ScoreManager.TotalScoreChangedEventArgs>.InvokeMerged(totalEvt).Forget();
        }

        public void SaveData(ref GameData data)
        {
            data.PlayerStatus = new PlayerStatus();
            data.PlayerStatus.Reset();
            this.CopyTo(data.PlayerStatus);
        }



        
        public PlayerStatus()
        {
            Guid = System.Guid.NewGuid();
        }
        
        public VariableContainer.Variable this[string key]
        {
            get
            {
                switch (key)
                {
                    case nameof(VariableKey.TotalScore):
                        return new VariableContainer.Variable()
                        {
                            IntValue = TotalScore
                        };
                }

                if (Variables.Items.ContainsKey(key))
                {
                    return Variables.Items[key];
                }
                return null;
            }
        }
        
        public void SaveVariable(string key, int value)
        {
            Variables.SetInteger(key, value);
        }

        public void SaveVariable(string key, float value)
        {
            Variables.SetFloat(key, value);
        }
        
        public PlayerStatus Clone()
        {
            var clone = new PlayerStatus();
            this.CopyTo(clone);
            return clone;
        }
        
        public void CopyTo(PlayerStatus target)
        {
            target.fieldSize = this.fieldSize;
            target.handSize = this.handSize;
            target.coinInterestRate = this.coinInterestRate;
            // target.CurrentScore = this.CurrentScore;
            // target.TotalScore = this.TotalScore;
            
            target.Variables = this.Variables.Clone();
        }
        public void CopyFrom(PlayerStatus source)
        {
            source.CopyTo(this);
        }
    }
    
    public class CurrentCoinChangedEventArgs : ExecEventArgs<CurrentCoinChangedEventArgs>
    {
        public int OldCurrentCoin { get; set; }
        public int NewCurrentCoin { get; set; }
    }
    

}

