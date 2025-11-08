using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public class Heal    {

        /// <summary> 방번호 </summary>
        public string HealID;
        /// <summary> 힐량 </summary>
        public List<int> HealAmount;
        /// <summary> 골드 획득량 </summary>
        public int Gold;
        /// <summary> 힐 가중치 </summary>
        public List<int> HealProbabillity;
    }
}
