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