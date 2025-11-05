using UnityEngine;

namespace Stage
{
    [System.Serializable]
    public class StageModel
    {
        [field:SerializeField] public int StageLevel {set; get; }
        [field:SerializeField] public int StageTargetScore {set; get; }
    }
}