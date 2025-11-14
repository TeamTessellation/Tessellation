using System;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// 스테이지 모델 클래스입니다.
    /// struct로 바꿀 수도 있음.
    /// </summary>
    [System.Serializable]
    public class StageModel : IComparable
    {
        public static StageModel FirstStageModel => CreateModel(1, 1);
        
        [field:SerializeField] public int[] StageIdentifiers {set; get; } = Array.Empty<int>();
        [field:SerializeField] public string StageName {set; get; }
        [field:SerializeField] public int StageWorld {set; get; }
        [field:SerializeField] public int StageLevel {set; get; }
        [field:SerializeField] public int StageTargetScore {set; get; }
        [field:SerializeField] public int StageTurnLimit {set; get; } = 10;

        public bool IsInfiniteTurn => StageTurnLimit <= 0;
        
        
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is StageModel otherStageModel)
            {
                return this.StageTargetScore.CompareTo(otherStageModel.StageTargetScore);
            }
            else
            {
                throw new ArgumentException("Object is not a StageModel");
            }
        }

        public StageModel GetNextStageModel()
        {
            int a = StageWorld;
            int b = StageLevel;

            if (b < 4)
            {
                b += 1;
            }
            else
            {
                a += 1;
                b = 1;
            }

            return CreateModel(a, b);
        }

        /// <summary>
        /// StageModel 생성 함수
        /// a - b 형식의 스테이지 네이밍과 타겟 스코어를 생성합니다.
        /// </summary>
        /// <param name="a">정수</param>
        /// <param name="b">4인경우 보스</param>
        /// <returns></returns>
        public static StageModel CreateModel(int a, int b)
        {
            if(a == 0 || b == 0)
                throw new ArgumentException("Parameters 'a' and 'b' must be greater than 0.");
            StageModel stageModel = new StageModel();
            if (b == 4)
            {
                stageModel.StageName = $"{a}-?";
            }
            else
            {
                stageModel.StageName = $"{a}-{b}";
            }
            stageModel.StageWorld = a;
            stageModel.StageLevel = b;
            
            stageModel.StageTargetScore = CalculateTargetScore(a, b);
            stageModel.StageIdentifiers = new int[] { a, b };
            return stageModel;
        }
        
        public static int CalculateTargetScore(int a, int b)
        {
            // a (월드)에 따른 배율 (2.5의 거듭제곱): 2.5^0, 2.5^1, 2.5^2
            // b (레벨)에 따른 배율: Level 1, Level 2 (x1.5), Level 3 (x2.0), Boss (x2.0)
            if (a < 1 || b < 1)
            {
                return 100;
            }
            float[] levelMultipliers = { 1.0f, 1.5f, 2.0f, 2.0f };
    
            int baseScore = 200;
     
            // Stage Multiplier 계산: 2.5^(a-1)
            float stageMultiplier = Mathf.Pow(2.5f, a - 1);
            
            float levelMultiplier = levelMultipliers[b - 1];
            
            float targetScore = baseScore * stageMultiplier * levelMultiplier;
    
            return Mathf.RoundToInt(targetScore);
        }
    }
}