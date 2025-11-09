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
        [field:SerializeField] public string StageName {set; get; }
        // [field:SerializeField] public int StageLevel {set; get; }
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
            string[] parts = StageName.Split('-');
            if (parts.Length != 2)
                throw new FormatException("StageName format is incorrect. Expected format: 'a-b'.");

            if (!int.TryParse(parts[0], out int a) || !int.TryParse(parts[1], out int b))
                throw new FormatException("StageName parts must be integers.");

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
            stageModel.StageName = $"{a}-{b}";
            stageModel.StageTargetScore = CalculateTargetScore(a, b);
            return stageModel;
        }
        
        public static int CalculateTargetScore(int a, int b)
        {
            float[] dataA = { 1, 1.5f, 2, 2 };
            float[] dataB = { 0.1f, 3.75f, 5f, 5f };
            int baseScore = 100;
            return Mathf.FloorToInt(baseScore * dataA[a - 1] * dataB[b - 1]);
        }
    }
}