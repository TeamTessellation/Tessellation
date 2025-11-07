using System;
using UnityEngine;

namespace Stage
{
    [System.Serializable]
    public class StageModel : IComparable
    {
        [field:SerializeField] public int StageLevel {set; get; }
        [field:SerializeField] public int StageTargetScore {set; get; }
        
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is StageModel otherStageModel)
            {
                return this.StageLevel.CompareTo(otherStageModel.StageLevel);
            }
            else
            {
                throw new ArgumentException("Object is not a StageModel");
            }
        }
    }
}