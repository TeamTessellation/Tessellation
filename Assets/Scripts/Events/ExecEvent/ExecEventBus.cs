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
            _execQueue.Clear();
            foreach (var handler in _handlers)
            {
                handler.Invoke(_execQueue, eventArgs);
            }
            _execQueue.SortByPriority();
            await _execQueue.ExecuteAll(eventArgs);
        }
    }
}