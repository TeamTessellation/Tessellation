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
    
    public class AfterPlayerActionEventArgs : ExecEventArgs<AfterPlayerActionEventArgs>
    {
        public AfterPlayerActionEventArgs()
        {

        }
    }
    
    /// <summary>
    /// 스테이지 initializing의 마무리 단계에 호출됩니다.
    /// </summary>
    public class StageStartEventArgs : ExecEventArgs<StageStartEventArgs>
    {
        public StageStartEventArgs()
        {

        }
    }
    
    
    /// <summary>
    /// 스테이지가 정상적으로 종료될 때 호출됩니다.
    /// </summary>
    public class StageEndEventArgs : ExecEventArgs<StageEndEventArgs>
    {
        
        public StageEndEventArgs()
        {

        }
    }
    
    /// <summary>
    /// 스테이지가 실패로 종료될 때 호출됩니다.
    /// </summary>
    public class StageFailEventArgs : ExecEventArgs<StageFailEventArgs>
    {
        public StageFailEventArgs()
        {

        }
    }

}