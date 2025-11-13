using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public partial class ItemData: IDBData {

        /// <summary> 아이템 아이디 </summary>
        public string itemID;
        /// <summary> 아이템 이름 key </summary>
        public string itemNameID;
        /// <summary> 아이템 설명 key </summary>
        public string ItemInscriptionID;
        /// <summary> (Reference:Enum<eItemRank>) 
         /// 아이템 등급 </summary>
        public string rank;
        /// <summary> 아이템 가격 </summary>
        public int price;
        /// <summary> 아이템 효과 숫자 </summary>
        public List<int> input;
        /// <summary> 효과 발동 우선순위 </summary>
        public int effectPriority;
        /// <summary> 등장 요구 조건 필수 보유 아이템 ID </summary>
        public List<string> requiredItemID;
        /// <summary> 아이템 아이콘 위치 </summary>
        public string iconPath;
    }
}
