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
    
    
}