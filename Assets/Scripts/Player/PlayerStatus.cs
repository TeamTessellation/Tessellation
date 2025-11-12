using System;
using Core;
using Cysharp.Threading.Tasks;
using ExecEvents;
using SaveLoad;
using Stage;
using UnityEngine;

namespace Player
{
    [Serializable]
    public class PlayerStatus : ISaveTarget
    {
        public static PlayerStatus Current => GameManager.Instance.PlayerStatus;
        
        [SerializeField] private int fieldSize = 4;
        [SerializeField] private int handSize = 3;
        [field:SerializeField]public VariableContainer Variables { get; private set; } = new VariableContainer();
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
            
            TotalScore,// 총 점수
            TotalClearedLines,// 총 지운 줄 수
            TotalAbilityUseCount,// 능력 사용 횟수
            MaxCoinsObtained,// 최대 획득 코인
            TotalCoins,// 총 코인
            
            StageBestPlacement,// 해당 스테이지 최고 배치
            StageScore,// 해당 스테이지 점수
            StageClearedLines,// 해당 스테이지 지운 줄 수
            StageAbilityUseCount,// 해당 스테이지 능력 사용 횟수
            StageCoinsObtained,// 해당 스테이지 획득 코인
            
        }

        public enum eActiveItemType
        {
            
        }
        
        
        public int FieldSize => fieldSize;
        public int HandSize => handSize;

        public int CurrentScore
        {
            get => Variables.GetVariable(nameof(VariableKey.StageScore)).IntValue;
            set => Variables.SetInteger(nameof(VariableKey.StageScore), value);
        }
        
        public int TotalScore
        {
            get => Variables.GetVariable(nameof(VariableKey.TotalScore)).IntValue;
            set => Variables.SetInteger(nameof(VariableKey.TotalScore), value);
        }
        
        public int CurrentTurn => TurnManager.Instance.CurrentTurn;
        public int RemainingTurns => TurnManager.Instance.RemainingTurns;

        public Guid Guid { get; init; }
        public void LoadData(GameData data)
        {

            data.PlayerStatus.CopyTo(this);
            using var evt = ScoreManager.CurrentScoreChangedEventArgs.Get();
            evt.NewCurrentScore = this.CurrentScore;    
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
                        break;
                    default:
                        break;
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
            // target.CurrentScore = this.CurrentScore;
            // target.TotalScore = this.TotalScore;
            
            target.Variables = this.Variables.Clone();
        }
        public void CopyFrom(PlayerStatus source)
        {
            source.CopyTo(this);
        }
    }
}