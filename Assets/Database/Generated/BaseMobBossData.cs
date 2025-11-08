using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public class BaseMobBossData    {

        /// <summary> 몹ID (이름) </summary>
        public string MobID;
        /// <summary> 몹이름(한글명) </summary>
        public string MobKorID;
        /// <summary> 몹유형 </summary>
        public string MobType;
        /// <summary> 기본HP </summary>
        public int BaseHP;
        /// <summary> 챕터계수 </summary>
        public float ChapterFactor;
        /// <summary> 성장계수 </summary>
        public int HPGrowFactor;
        /// <summary> 공격주기 </summary>
        public int AttackCycle;
        /// <summary> 공격 데미지 </summary>
        public int AttackDamage;
        /// <summary> BoolAttackType에 대한 설명! 여기클릭! </summary>
        public bool BoolAttackType;
        public List<int> AttackWeight;
        /// <summary> (Reference:List<Enum<Cardevil.InGame.Enemy.AttackStyle>>) 
         /// 사용 족보 순서(알맞게넣으세요 !오타없이) </summary>
        public string AttackPattern;
        /// <summary> 유도/랜덤 </summary>
        public bool AttackPlayer;
    }
}
