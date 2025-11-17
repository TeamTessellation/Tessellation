using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public partial class ItemData: IDBData {

        /// <summary> 아이템 아이디 </summary>
        public string ItemID;
        /// <summary> 아이템 타입 </summary>
        public Abilities.eItemType eItemType;
        /// <summary> 아이템 이름 key </summary>
        public string itemNameID;
        /// <summary> 아이템 설명 key </summary>
        public string DescriptionID;
        /// <summary> 아이템 등장 여부 </summary>
        public bool CanAppearInShop;
        /// <summary> 아이템 등급 </summary>
        public Abilities.eRarity Rarity;
        /// <summary> 아이템 효과 숫자 </summary>
        public List<float> input;
        /// <summary> 아이템 가격 </summary>
        public int ItemPrice;
        /// <summary> 중복 불가 아이템 ID </summary>
        public List<string> ConflictingItems;
        /// <summary> 합성아이템 여부 </summary>
        public bool IsSynthesisItem;
        /// <summary> 등장 요구 조건 필수 보유 아이템 ID </summary>
        public List<string> SynthesisRequirements;
    }
}
