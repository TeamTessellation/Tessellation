using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public class mob    {

        /// <summary> 한국어변수 </summary>
        public string roomID;
        /// <summary> 영어변수 </summary>
        public List<string> mobName;
        /// <summary> 숫자 </summary>
        public int number;
        /// <summary> 리스트 </summary>
        public List<int> HP;
        /// <summary> 불리안 </summary>
        public List<int> attackCycle;
        /// <summary> Enum은 미리 코드에서 선언해야함 </summary>
        public List<int> attackDamage;
        /// <summary> (족보를 어떻게 뽑는 지)  </summary>
        public List<int> attackType;
    }
}
