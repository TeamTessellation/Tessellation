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
    /// 기존 <see cref="ExecEventBus{TEvent}"/>와 달리, 핸들러 등록 시 우선순위를 함께 지정하며, 좀더 최적화되어 있습니다.
    /// </remarks>
    /// <typeparam name="TEvent"><see cref="ExecEventArgs{T}"/>의 파생 클래스</typeparam>
    public static class ExecStaticEventBus<TEvent> where TEvent : ExecEventArgs<TEvent>, new()
    {
        
        private static readonly ExecQueue<TEvent> _execQueue = new ExecQueue<TEvent>();
        public static IReadOnlyList<ExecAction<TEvent>> Handlers => _execQueue;
        
        public static bool IsExecuting => _execQueue.IsExecuting;
        
        
        public static void Register(int priority, ExecAction<TEvent> handler)
        {
            _execQueue.Enqueue(priority, handler);
        }
        public static void Unregister(ExecAction<TEvent> handler)
        {
            _execQueue.Remove(handler);
        }
        public static void ClearHandlers()
        {
            _execQueue.Clear();
        }
        
        public static async UniTask Invoke(TEvent eventArgs)
        {
            await _execQueue.ExecuteAll(eventArgs);
        }
    }
}