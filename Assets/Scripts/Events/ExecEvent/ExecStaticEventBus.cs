using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Events.Core;

namespace PriortyExecEvent
{
    /// <summary>
    /// 우선순위 실행 이벤트 버스 클래스.
    /// 우선순위는 정적이며, 명시적으로 우선순위를 미리 설정해야 합니다.<br/>
    /// </summary>
    /// <remarks>
    /// 기존 <see cref="ExecDynamicEventBus{TEvent}"/>와 달리, 핸들러 등록 시 우선순위를 함께 지정하며, 좀더 최적화되어 있습니다.
    /// </remarks>
    /// <typeparam name="TEvent"><see cref="ExecEventArgs{T}"/>의 파생 클래스</typeparam>
    public static class ExecStaticEventBus<TEvent> where TEvent : ExecEventArgs<TEvent>, new()
    {
        
        private static readonly ExecQueue<TEvent> _execQueue = new ExecQueue<TEvent>();
        public static IReadOnlyList<ExecQueue<TEvent>.ActionWrapper> ExecQueue => _execQueue;
        
        public static bool IsExecuting => _execQueue.IsExecuting;
        
        
        /// <summary>
        /// 핸들러를 우선순위와 함께 등록합니다.
        /// </summary>
        /// <code>
        /// void RegisterHandlers(){
        ///   ExecStaticEventBus&lt;MyEventArgs&gt;.Register(10, OnMyEvent);
        ///   ExecStaticEventBus&lt;MyEventArgs&gt;.Register(10, OnMyEvent2, 5, 3); // Primary가 같으면 5, 3으로 추가 비교
        /// }
        /// 
        /// UniTask OnMyEvent(MyEventArgs args){
        ///   // ...
        /// }
        /// </code>
        /// <param name="priority">우선순위</param>
        /// <param name="handler">실행할 액션</param>
        /// <param name="extraPriorities">추가 우선순위 (Primary Priority가 같을 때 순서대로 비교됩니다)</param>
        public static void Register(int priority, ExecAction<TEvent> handler, params int[] extraPriorities)
        {
            _execQueue.Enqueue(priority, handler, extraPriorities);
        }
        
        /// <summary>
        /// 핸들러 등록을 해제합니다.
        /// </summary>
        /// <param name="handler"></param>
        public static void Unregister(ExecAction<TEvent> handler)
        {
            _execQueue.Remove(handler);
        }
        /// <summary>
        /// 모든 핸들러를 제거합니다.
        /// </summary>
        public static void ClearHandlers()
        {
            _execQueue.Clear();
        }
        
        /// <summary>
        /// 이벤트를 호출합니다.
        /// </summary>
        /// <code>
        /// using var args = new MyEventArgs { ... };
        /// await ExecStaticEventBus&lt;MyEventArgs&gt;.Invoke(args);
        /// </code>
        /// <param name="eventArgs"></param>
        public static async UniTask Invoke(TEvent eventArgs)
        {
            await _execQueue.ExecuteAll(eventArgs);
        }
        
        public static ExecQueue<TEvent> GetExecQueue()
        {
            return _execQueue;
        }
        
    }
}