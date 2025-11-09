using System;
using Core;
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

        public void Reset()
        {
            fieldSize = 4;
            handSize = 3;
            Variables = new VariableContainer();
            Variables.SetInteger(nameof(VariableKey.BestScorePlacement), 0);
            Variables.SetInteger(nameof(VariableKey.TotalScore), 0);
            Variables.SetInteger(nameof(VariableKey.ClearedLines), 0);
            Variables.SetInteger(nameof(VariableKey.AbilityUseCount), 0);
            Variables.SetInteger(nameof(VariableKey.MaxCoinsObtained), 0);
            Variables.SetInteger(nameof(VariableKey.TotalCoins), 0);
        }
        
        public enum VariableKey
        {
            BestScorePlacement,// 최고 득점 배치
            TotalScore,// 총 점수
            ClearedLines,// 지운 줄 수
            AbilityUseCount,// 능력 사용 횟수
            MaxCoinsObtained,// 최대 획득 코인
            TotalCoins// 총 코인
        }
        
        
        
        
        public int FieldSize => fieldSize;
        public int HandSize => handSize;
        public int CurrentScore => ScoreManager.Instance.CurrentScore;
        public int TotalScore => ScoreManager.Instance.TotalScore;
        public int CurrentTurn => TurnManager.Instance.CurrentTurn;
        public int RemainingTurns => TurnManager.Instance.RemainingTurns;

        public Guid Guid { get; init; }
        public void LoadData(GameData data)
        {
            data.PlayerStatus.CopyTo(this);
        }

        public void SaveData(ref GameData data)
        {
            this.CopyTo(data.PlayerStatus);
        }


        public VariableContainer Variables { get; private set; } = new VariableContainer();
        
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
            target.Variables = this.Variables.Clone();
        }
        public void CopyFrom(PlayerStatus source)
        {
            source.CopyTo(this);
        }
    }
}