using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Events.Core;

namespace PriortyExecEvent
{
    /// <summary>
    /// 우선순위 실행 이벤트 버스 클래스<br/>
    /// 각 핸들러는 Queue에 작업을 등록하여 우선순위를 지정할 수 있습니다.
    /// </summary>
    /// <remarks>
    /// 실행시마다 우선순위를 정렬하므로, 빈번한 이벤트 호출에는 적합하지 않을 수 있습니다.
    /// 해당 경우에는 <see cref="ExecStaticEventBus{TEvent}"/>를 고려하세요.
    /// </remarks>
    /// <typeparam name="TEvent"><see cref="ExecEventArgs{T}"/>의 파생 클래스</typeparam>
    public static class ExecEventBus<TEvent> where TEvent : ExecEventArgs<TEvent>, new()
    {
        private static List<ExecEventHandler<TEvent>> _handlers = new List<ExecEventHandler<TEvent>>();
        private static readonly ExecQueue<TEvent> _execQueue = new ExecQueue<TEvent>();
        private static bool _isExecuting = false;
        
        public static bool IsExecuting => _isExecuting || _execQueue.IsExecuting;
        public static IReadOnlyList<ExecEventHandler<TEvent>> Handlers => _handlers;
        
        
        public static void Register(ExecEventHandler<TEvent> handler)
        {
            _handlers.Add(handler);
        }

        public static void Unregister(ExecEventHandler<TEvent> handler)
        {
            _handlers.Remove(handler);
        }
        
        public static void ClearHandlers()
        {
            _handlers.Clear();
        }
        
        public static async UniTask Invoke(TEvent eventArgs)
        {
            _isExecuting = true;
            _execQueue.Clear();
            _execQueue.SetCapacity(_handlers.Count);
            foreach (var handler in _handlers)
            {
                handler.Invoke(_execQueue, eventArgs);
            }
            _execQueue.SortByPriority();
            await _execQueue.ExecuteAll(eventArgs);
            _isExecuting = false;
        }
        
        /// <summary>
        /// 핸들러 실행 후 정적 이벤트 버스도 함께 호출합니다.
        /// </summary>
        /// <param name="eventArgs"></param>
        public static async UniTask InvokeWithStatic(TEvent eventArgs)
        {
            _isExecuting = true;
            _execQueue.Clear();
            _execQueue.SetCapacity(_handlers.Count);
            foreach (var handler in _handlers)
            {
                handler.Invoke(_execQueue, eventArgs);
            }
            _execQueue.SortByPriority();
            await _execQueue.ExecuteAll(eventArgs);
            _isExecuting = false;
            await ExecStaticEventBus<TEvent>.Invoke(eventArgs);
        }
    }
}