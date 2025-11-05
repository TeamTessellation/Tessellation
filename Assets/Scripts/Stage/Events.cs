using ExecEvents;

namespace Stage
{
    public class TurnStartEventArgs : ExecEventArgs<TurnStartEventArgs>
    {
        public TurnStartEventArgs()
        {

        }
    }
    
    public class TurnEndEventArgs : ExecEventArgs<TurnEndEventArgs>
    {
        public TurnEndEventArgs()
        {

        }
    }
    
    public class PlayerActionLoopStartEventArgs : ExecEventArgs<PlayerActionLoopStartEventArgs>
    {
        public PlayerActionLoopStartEventArgs()
        {

        }
    }
    
    public class PlayerActionLoopEndEventArgs : ExecEventArgs<PlayerActionLoopEndEventArgs>
    {
        public PlayerActionLoopEndEventArgs()
        {

        }
    }
    
    public class BeforePlayerActionEventArgs : ExecEventArgs<BeforePlayerActionEventArgs>
    {
        public BeforePlayerActionEventArgs()
        {

        }
    }
    
    public class StageEndEventArgs : ExecEventArgs<StageEndEventArgs>
    {
        public bool IsCleared;
        
        public StageEndEventArgs()
        {

        }
    }

}