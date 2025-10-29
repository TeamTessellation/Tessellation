using Events.Core;

namespace Cardevil.Events
{
    /// <summary>
    /// 
    /// </summary>
    public class ScoreChangedEventArgs : EventArgs<ScoreChangedEventArgs>
    {
        /// <summary>
        /// 이전 점수
        /// </summary>
        public int PreviousScore;
        /// <summary>
        /// 새로운 점수
        /// </summary>
        public int NewScore;
        /// <summary>
        /// 최종 점수(해당 값으로 최종 적용)
        /// </summary>
        public int FinalScore;
        
        public void Init(int previousScore, int newScore)
        {
            PreviousScore = previousScore;
            NewScore = newScore;
            FinalScore = newScore;
        }
        
        public override void Clear()
        {
            PreviousScore = 0;
            NewScore = 0;
        }
    }
}