using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public class machinePossibillity    {

        /// <summary> 머신레벨 </summary>
        public int MachineLevel;
        /// <summary> 일반가중치 </summary>
        public int NormalProbabliity;
        /// <summary> 희귀가중치 </summary>
        public int RareProbabliity;
        /// <summary> 초희귀가중치 </summary>
        public int EpicProbabliity;
        /// <summary> 전설가중치 </summary>
        public int LegendProbabliity;
    }
}
