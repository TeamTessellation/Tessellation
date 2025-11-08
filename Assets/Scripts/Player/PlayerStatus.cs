using System;
using Core;
using Stage;
using UnityEngine;

namespace Player
{
    [Serializable]
    public class PlayerStatus
    {
        public static PlayerStatus Current => GameManager.Instance.PlayerStatus;
        
        [SerializeField] private int fieldSize = 4;
        [SerializeField] private int handSize = 5;
        
        
        
        public int FieldSize => fieldSize;
        public int HandSize => handSize;
        public int CurrentScore => ScoreManager.Instance.CurrentScore;
        public int TotalScore => ScoreManager.Instance.TotalScore;
        public int CurrentTurn => TurnManager.Instance.CurrentTurn;
        public int RemainingTurns => TurnManager.Instance.RemainingTurns;
        
    }
}