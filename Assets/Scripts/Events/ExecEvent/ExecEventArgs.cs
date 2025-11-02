using Events.Core;

namespace PriortyExecEvent
{
    /// <summary>
    /// Exec 이벤트 인자.
    /// 이벤트 중단 기능이 추가되어있습니다.
    /// </summary>
    /// <remarks>
    /// 해당 이벤트 클래스는 정의되면 자동으로 해당하는 EventBus가 생성됩니다.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class ExecEventArgs<T> : EventArgs<T> where T : ExecEventArgs<T>, new()
    {
        /// <summary>
        /// 이벤트 중단 여부
        /// </summary>
        public bool BreakChain { get; set; } = false;
        
        protected ExecEventArgs() : base()
        {
        }
        
        public override void Clear()
        {
            BreakChain = false;
        }
    }
}