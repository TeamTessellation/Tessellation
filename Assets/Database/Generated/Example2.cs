using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public class Example2    {

        /// <summary> 방ID </summary>
        public string RoomID;
        /// <summary> 방별 몹의 수 </summary>
        public int MobCount;
        /// <summary> 나올 수 있는 몬스터들 리스트 </summary>
        public List<string> MobList;
        /// <summary> 성장 계수 피곱셈 </summary>
        public int RoomSequence;
        /// <summary> HP 비율 </summary>
        public List<float> HPRate;
        /// <summary> 비율 결정 확률 </summary>
        public List<float> HPRateProbabillity;
        /// <summary> 나오게 될 슬롯머신의 레벨 </summary>
        public int MachineLevel;
    }
}
