using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    [CreateAssetMenu(fileName = "StageModelContainerSO", menuName = "Stage/StageModelContainerSO")]
    public class StageModelContatinerSO : ScriptableObject
    {
        private List<StageModel> stageModels;
        public List<StageModel> StageModels => stageModels;
        
        private void OnEnable()
        {
            stageModels.Sort();
        }
        
        
    }
}