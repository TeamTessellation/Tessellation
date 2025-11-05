using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Events.Core;

namespace ExecEvents
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
    public static class ExecDynamicEventBus<TEvent> where TEvent : ExecEventArgs<TEvent>, new()
    {
        private static List<ExecEventHandler<TEvent>> _handlers = new List<ExecEventHandler<TEvent>>();
        private static readonly ExecQueue<TEvent> _execQueue = new ExecQueue<TEvent>();
        
        public static bool IsExecuting => _execQueue.IsExecuting;
        public static IReadOnlyList<ExecEventHandler<TEvent>> Handlers => _handlers;
        
        
        /// <summary>
        /// 핸들러를 등록합니다.
        /// </summary>
        /// <code>
        ///
        /// void RegisterHandlers(){
        ///   ExecEventBus&lt;MyEventArgs&gt;.Register(OnMyEvent);
        /// }
        ///
        /// UniTask OnMyEvent(ExecQueue&lt;MyEventArgs&gt; queue, MyEventArgs args){
        ///   queue.Enqueue(10, async (a) =&gt; {
        ///       // ...
        ///   });
        ///   queue.Enqueue(ExecPriority.VeryHigh, highPriorityAction); // 가장 먼저 실행됨
        /// }
        ///
        /// ExecAction&lt;MyEventArgs&gt; highPriorityAction = async (args) =&gt; {
        ///   // ...
        /// };
        /// </code>
        /// <param name="handler"></param>
        public static void Register(ExecEventHandler<TEvent> handler)
        {
            _handlers.Add(handler);
        }

        /// <summary>
        /// 핸들러 등록을 해제합니다.
        /// </summary>
        /// <param name="handler"></param>
        public static void Unregister(ExecEventHandler<TEvent> handler)
        {
            _handlers.Remove(handler);
        }
        
        /// <summary>
        /// 모든 핸들러를 제거합니다.
        /// </summary>
        public static void ClearHandlers()
        {
            _handlers.Clear();
        }

        /// <summary>
        /// 등록된 모든 핸들러를 호출합니다.<br/>
        /// 각 핸들러는 <see cref="ExecQueue{TEventArgs}"/>에 작업을 등록하여 우선순위를 지정할 수 있습니다.
        /// </summary>
        /// <code>
        /// using var args = new MyEventArgs { ... };
        /// await ExecEventBus&lt;MyEventArgs&gt;.Invoke(args);
        /// </code>
        /// <param name="eventArgs"></param>
        /// <param name="cancellationToken"></param>
        public static async UniTask Invoke(TEvent eventArgs, CancellationToken cancellationToken = default)
        {
            _execQueue.Clear();
            _execQueue.SetCapacity(_handlers.Count);
            if(eventArgs.BreakChain) 
            {
                return;
            }
            foreach (var handler in _handlers)
            {
                handler.Invoke(_execQueue, eventArgs);
            }
            await _execQueue.ExecuteAll(eventArgs, cancellationToken);
        }

        /// <summary>
        /// 모든 핸들러를 Invoke한 후, 해당 큐를 반환합니다.
        /// </summary>
        /// <returns></returns> 
        public static ExecQueue<TEvent> InvocationQueue(TEvent eventArgs)
        {
            _execQueue.Clear();
            _execQueue.SetCapacity(_handlers.Count);
            foreach (var handler in _handlers)
            {
                handler.Invoke(_execQueue, eventArgs);
            }
            _execQueue.SortByPriority();
            return _execQueue;
        }



    }
}