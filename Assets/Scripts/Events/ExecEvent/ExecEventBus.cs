using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Events.Core;

namespace PriortyExecEvent
{
    /// <summary>
    /// 우선순위 실행 이벤트 버스 클래스
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public static class ExecEventBus<TEvent> where TEvent : ExecEventArgs<TEvent>, new()
    {
        private static List<ExecEventHandler<TEvent>> _handlers = new List<ExecEventHandler<TEvent>>();
        private static readonly ExecQueue<TEvent> _execQueue = new ExecQueue<TEvent>();
        private static bool _isExecuting = false;
        
        public static bool IsExecuting => _isExecuting;
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
    }
}