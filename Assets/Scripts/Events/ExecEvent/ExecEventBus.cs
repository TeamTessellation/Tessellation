using Cysharp.Threading.Tasks;
using Machamy.Utils;

namespace ExecEvents
{
    /// <summary>
    /// ExecDynamicEventBus와 ExecStaticEventBus의 기능을 모두 포함하는 이벤트 버스 클래스입니다.
    /// </summary>
    public static class ExecEventBus<TEvent> where TEvent : ExecEventArgs<TEvent>, new()
    {
        
        private static bool _isMergedExecuting = false;
        public static bool IsExecuting => ExecDynamicEventBus<TEvent>.IsExecuting || ExecStaticEventBus<TEvent>.IsExecuting || _isMergedExecuting;
        /// <summary>
        /// 동적 핸들러를 등록합니다.
        /// </summary>
        /// <inheritdoc cref="ExecDynamicEventBus{TEvent}.Register(ExecEventHandler{TEvent})"/>
        public static void RegisterDynamic(ExecEventHandler<TEvent> handler)
        {
            ExecDynamicEventBus<TEvent>.Register(handler);
        }
        /// <summary>
        /// 동적 핸들러 등록을 해제합니다.
        /// </summary>
        /// <inheritdoc cref="ExecDynamicEventBus{TEvent}.Unregister(ExecEventHandler{TEvent})"/>
        public static void UnregisterDynamic(ExecEventHandler<TEvent> handler)
        {
            ExecDynamicEventBus<TEvent>.Unregister(handler);
        }
        /// <summary>
        /// 정적 핸들러를 우선순위와 함께 등록합니다.
        /// </summary>
        /// <inheritdoc cref="ExecStaticEventBus{TEvent}.Register(int, ExecAction{TEvent}, int[])"/>
        public static void RegisterStatic(int priority, ExecAction<TEvent> handler, params int[] extraPriorities)
        {
            ExecStaticEventBus<TEvent>.Register(priority, handler, extraPriorities);
        }
        
        /// <summary>
        /// 정적 핸들러를 우선순위와 함께 이진탐색으로 등록합니다.
        /// </summary>
        /// <inheritdoc cref="ExecStaticEventBus{TEvent}.RegisterSafeBinarySearch(int, ExecAction{TEvent}, int[])"/>
        public static void RegisterStaticBinarySearch(int priority, ExecAction<TEvent> handler, params int[] extraPriorities)
        {
            ExecStaticEventBus<TEvent>.RegisterSafeBinarySearch(priority, handler, extraPriorities);
        }
        

        /// <summary>
        /// 정적 핸들러 등록을 해제합니다.
        /// </summary>
        /// <inheritdoc cref="ExecStaticEventBus{TEvent}.Unregister(ExecAction{TEvent})"/>
        public static void UnregisterStatic(ExecAction<TEvent> handler)
        {
            ExecStaticEventBus<TEvent>.Unregister(handler);
        }
        /// <summary>
        /// 모든 동적 핸들러를 제거합니다.
        /// </summary>
        /// <inheritdoc cref="ExecDynamicEventBus{TEvent}.ClearHandlers()"/>
        public static void ClearDynamicHandlers()
        {
            ExecDynamicEventBus<TEvent>.ClearHandlers();
        }
        /// <summary>
        /// 모든 정적 핸들러를 제거합니다.
        /// </summary>
        /// <inheritdoc cref="ExecStaticEventBus{TEvent}.ClearHandlers()"/>
        public static void ClearStaticHandlers()
        {
            ExecStaticEventBus<TEvent>.ClearHandlers();
        }
        /// <summary>
        /// 모든 핸들러를 제거합니다.
        /// </summary>
        public static void ClearAllHandlers()
        {
            ExecDynamicEventBus<TEvent>.ClearHandlers();
            ExecStaticEventBus<TEvent>.ClearHandlers();
        }
        
        /// <summary>
        /// 동적 이벤트를 호출한 뒤, 정적 이벤트를 호출합니다.
        /// </summary>
        /// <code>
        /// using var args = new MyEventArgs { ... };
        /// await ExecEventBus&lt;MyEventArgs&gt;.InvokeSequentially(args);
        /// </code>
        /// <param name="args"></param>
        public static async UniTask InvokeSequentially(TEvent args)
        {
            LogEx.Log("Invoking Dynamic Event Bus");
            await ExecDynamicEventBus<TEvent>.Invoke(args);
            LogEx.Log("Invoking Static Event Bus");
            await ExecStaticEventBus<TEvent>.Invoke(args);
        }

        /// <summary>
        /// 동적 이벤트와 정적 이벤트를 병합하여 우선순위에 따라 호출합니다.
        /// 두 큐를 병합 정렬(merge sort) 방식으로 실행합니다.
        /// </summary>
        /// <remarks>
        /// 같은 우선순위일 경우, 동적 이벤트가 먼저 실행됩니다.
        /// </remarks>
        /// <code>
        /// using var args = new MyEventArgs { ... };
        /// await ExecEventBus&lt;MyEventArgs&gt;.InvokeMerged(args);
        /// </code>
        /// <param name="args"></param>
        public static async UniTask InvokeMerged(TEvent args)
        {
            LogEx.Log("Invoking Merged Event Bus");
            _isMergedExecuting = true;
            
            try
            {
                var dynamicQueue = ExecDynamicEventBus<TEvent>.InvocationQueue(args);
                var staticQueue = ExecStaticEventBus<TEvent>.GetExecQueue();
                staticQueue.SortByPriorityIfDirty();

                int dynamicIndex = 0;
                int staticIndex = 0;

                // 두 큐를 병합 정렬 방식으로 실행
                while (dynamicIndex < dynamicQueue.Count && staticIndex < staticQueue.Count)
                {
                    var dynamicAction = dynamicQueue[dynamicIndex];
                    var staticAction = staticQueue[staticIndex];
                    
                    if (dynamicAction.CompareTo(staticAction) <= 0)
                    {
                        LogEx.Log($"({dynamicAction.PrimaryPriority})Executing Dynamic action {dynamicAction.action}");
                        await dynamicAction.action.Invoke(args);
                        dynamicIndex++;
                    }
                    else
                    {
                        LogEx.Log($"({staticAction.PrimaryPriority})Executing Static action {staticAction.action}");
                        await staticAction.action.Invoke(args);
                        staticIndex++;
                    }
                    
                    if (args.BreakChain)
                    {
                        LogEx.Log("Merged Event Bus chain broken");
                        break;
                    }
                }
                
                // 남은 동적 작업 실행
                while (dynamicIndex < dynamicQueue.Count && !args.BreakChain)
                {
                    var dynamicAction = dynamicQueue[dynamicIndex];
                    LogEx.Log($"({dynamicAction.PrimaryPriority})Executing Dynamic action {dynamicAction.action}");
                    await dynamicAction.action.Invoke(args);
                    dynamicIndex++;
                }
                // 남은 정적 작업 실행
                while (staticIndex < staticQueue.Count && !args.BreakChain)
                {
                    var staticAction = staticQueue[staticIndex];
                    LogEx.Log($"({staticAction.PrimaryPriority})Executing Static action {staticAction.action}");
                    await staticAction.action.Invoke(args);
                    staticIndex++;
                }
            }
            finally
            {
                _isMergedExecuting = false;
            }
            
            LogEx.Log("Merged Event Bus Invocation Complete");
        }
    }
}