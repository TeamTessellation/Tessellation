using Unity.Services.Analytics;

namespace DataAnalysis.Events
{
    public class GameStartEvent : Event
    {
        public GameStartEvent() : base("game_start")
        {
        }
    }
    
    public class ItemAcquiredEvent : Event
    {
        public ItemAcquiredEvent() : base("item_acquired")
        {
        }
        
        public string ItemId
        {
            set => SetParameter("itemId", value);
        }
    }
    
    public class GameOverEvent : Event
    {
        public GameOverEvent() : base("game_over")
        {
        }
        
        public bool IsWin
        {
            set => SetParameter("isWin", value);
        }
        
        public int PlayTime
        {
            set => SetParameter("playTime", value);
        }
        
        public string PlayerName
        {
            set => SetParameter("playerName", value);
        }
    }
    
    public class LevelStartEvent : Event
    {
        public LevelStartEvent() : base("level_start")
        {
        }
        
        public int LevelNumber
        {
            set => SetParameter("levelNumber", value);
        }
    }
    public class LevelCompleteEvent : Event
    {
        public LevelCompleteEvent() : base("level_complete")
        {
        }
        
        public int LevelNumber
        {
            set => SetParameter("levelNumber", value);
        }
        
        public int EarnedScore
        {
            set => SetParameter("earnedScore", value);
        }
    }
    
    /// <summary>
    /// 스테이지 클리어 시 수집하는 이벤트
    /// </summary>
    public class StageClearEvent : Event
    {
        public StageClearEvent() : base("stage_clear")
        {
        }
        
        /// <summary>현재 스테이지 (예: "1-1")</summary>
        public string CurrentStageName
        {
            set => SetParameter("currentStageName", value);
        }
        
        /// <summary>게임 시작 시 랜덤 시드 값</summary>
        public uint GameRandomSeed
        {
            set => SetParameter("gameRandomSeed", value);
        }
        
        /// <summary>스테이지 시작 시 랜덤 시드 값</summary>
        public uint StageRandomSeed
        {
            set => SetParameter("stageRandomSeed", value);
        }
        
        /// <summary>해당 스테이지 최고 배치</summary>
        public int StageBestPlacement
        {
            set => SetParameter("stageBestPlacement", value);
        }
        
        /// <summary>해당 스테이지 점수</summary>
        public int StageScore
        {
            set => SetParameter("stageScore", value);
        }
        
        /// <summary>해당 스테이지 지운 줄 수</summary>
        public int StageClearedLines
        {
            set => SetParameter("stageClearedLines", value);
        }
        
        /// <summary>해당 스테이지 남은 턴 수</summary>
        public int RemainingTurns
        {
            set => SetParameter("remainingTurns", value);
        }
        
        /// <summary>해당 스테이지 사용한 액티브 아이템 사용 횟수</summary>
        public int StageAbilityUseCount
        {
            set => SetParameter("stageAbilityUseCount", value);
        }
        
        /// <summary>해당 스테이지 획득 코인</summary>
        public int StageCoinsObtained
        {
            set => SetParameter("stageCoinsObtained", value);
        }
        
        /// <summary>현재 보유 아이템 (JSON 문자열)</summary>
        public string CurrentItems
        {
            set => SetParameter("currentItems", value);
        }
        
        /// <summary>현재 보유 코인</summary>
        public int CurrentCoins
        {
            set => SetParameter("currentCoins", value);
        }
        
        /// <summary>현재 남은 액티브 아이템 사용 횟수</summary>
        public int RemainingActiveItemCount
        {
            set => SetParameter("remainingActiveItemCount", value);
        }
    }
    
    /// <summary>
    /// 게임 종료 시 수집하는 이벤트
    /// </summary>
    public class GameEndEvent : Event
    {
        public GameEndEvent() : base("game_end")
        {
        }
        
        /// <summary>게임이 종료된 스테이지 (예: "2-3")</summary>
        public string EndedStage
        {
            set => SetParameter("endedStage", value);
        }
        
        /// <summary>게임 시작 시 랜덤 시드 값</summary>
        public uint GameRandomSeed
        {
            set => SetParameter("gameRandomSeed", value);
        }
        
        /// <summary>스테이지 시작 시 랜덤 시드 값</summary>
        public uint StageRandomSeed
        {
            set => SetParameter("stageRandomSeed", value);
        }
        
        /// <summary>마지막 스테이지 최고 배치</summary>
        public int LastStageBestPlacement
        {
            set => SetParameter("lastStageBestPlacement", value);
        }
        
        /// <summary>마지막 스테이지 점수</summary>
        public int LastStageScore
        {
            set => SetParameter("lastStageScore", value);
        }
        
        /// <summary>마지막 스테이지 지운 줄 수</summary>
        public int LastStageClearedLines
        {
            set => SetParameter("lastStageClearedLines", value);
        }
        
        /// <summary>마지막 스테이지 남은 턴 수</summary>
        public int LastStageRemainingTurns
        {
            set => SetParameter("lastStageRemainingTurns", value);
        }
        
        /// <summary>마지막 스테이지 사용한 액티브 아이템 사용 횟수</summary>
        public int LastStageAbilityUseCount
        {
            set => SetParameter("lastStageAbilityUseCount", value);
        }
        
        /// <summary>마지막 스테이지 획득 코인</summary>
        public int LastStageCoinsObtained
        {
            set => SetParameter("lastStageCoinsObtained", value);
        }
        
        /// <summary>현재 보유 아이템 (JSON 문자열)</summary>
        public string CurrentItems
        {
            set => SetParameter("currentItems", value);
        }
        
        /// <summary>현재 보유 코인</summary>
        public int CurrentCoins
        {
            set => SetParameter("currentCoins", value);
        }
        
        /// <summary>현재 남은 액티브 아이템 사용 횟수</summary>
        public int RemainingActiveItemCount
        {
            set => SetParameter("remainingActiveItemCount", value);
        }
        
        /// <summary>최고 득점 배치</summary>
        public int BestScorePlacement
        {
            set => SetParameter("bestScorePlacement", value);
        }
        
        /// <summary>최고 점수</summary>
        public int BestStageScore
        {
            set => SetParameter("bestStageScore", value);
        }
        
        /// <summary>총 획득 점수</summary>
        public int TotalScore
        {
            set => SetParameter("totalScore", value);
        }
        
        /// <summary>총 지운 줄 수</summary>
        public int TotalClearedLines
        {
            set => SetParameter("totalClearedLines", value);
        }
        
        /// <summary>총 능력 사용 횟수</summary>
        public int TotalAbilityUseCount
        {
            set => SetParameter("totalAbilityUseCount", value);
        }
        
        /// <summary>최대 획득 코인 (단일 스테이지 기준)</summary>
        public int BestStageCoinsObtained
        {
            set => SetParameter("bestStageCoinsObtained", value);
        }
        
        /// <summary>총 획득 코인</summary>
        public int TotalObtainedCoins
        {
            set => SetParameter("totalObtainedCoins", value);
        }
        
        /// <summary>광고 보고 이어하기 사용 횟수</summary>
        public int TotalReviveCount
        {
            set => SetParameter("totalReviveCount", value);
        }
    }
}