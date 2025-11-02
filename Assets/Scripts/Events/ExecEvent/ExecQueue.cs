using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Events.Core;
using Machamy.Utils;

namespace PriortyExecEvent
{
    /// <summary>
    /// 우선순위 실행 큐 클래스
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    public class ExecQueue<TEventArgs> where TEventArgs : ExecEventArgs<TEventArgs> , new()
    {
        /// <summary>
        /// 우선순위 실행 액션 래퍼 클래스
        /// </summary>
        protected class ActionWrapper<TWrapperEventArgs> : IComparable<ActionWrapper<TWrapperEventArgs>>, IComparable where TWrapperEventArgs : ExecEventArgs<TWrapperEventArgs>, new()
        {
            public int PrimaryPriority;
            public ExecAction<TWrapperEventArgs> action;
            
            public ActionWrapper(ExecAction<TWrapperEventArgs> action, int primaryPriority)
            {
                this.action = action;
                PrimaryPriority = primaryPriority;
            }


            public int CompareTo(object obj)
            {
                if (obj is null) return 1;
                if (ReferenceEquals(this, obj)) return 0;
                return obj is ActionWrapper<TWrapperEventArgs> other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ExecQueue<TEventArgs>.ActionWrapper<TWrapperEventArgs>)}");
            }

            public int CompareTo(ActionWrapper<TWrapperEventArgs> other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (other is null) return 1;
                return PrimaryPriority.CompareTo(other.PrimaryPriority);
            }
        }

        // 어차피 실시간으로 우선순위 변경이 필요 없으므로 리스트로 구현
        // private SortedSet<ActionWrapper> _actionWrappers = new SortedSet<ActionWrapper>();

        private List<ActionWrapper<TEventArgs>> _actionWrappers = new List<ActionWrapper<TEventArgs>>();
        
        /// <summary>
        /// 우선순위에 따라 액션을 큐에 추가합니다.
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="action"></param>
        public void Enqueue(ExecPriority priority, ExecAction<TEventArgs> action)
        {
            Enqueue((int)priority, action);
        }
        
        /// <summary>
        /// 우선순위에 따라 액션을 큐에 추가합니다.
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="action"></param>
        public void Enqueue(int priority, ExecAction<TEventArgs> action)
        {
            var wrapper = new ActionWrapper<TEventArgs>(action, priority);
            _actionWrappers.Add(wrapper);
        }
        
        /// <summary>
        /// 우선순위에 따라 정렬합니다.
        /// </summary>
        public void SortByPriority()
        {
            _actionWrappers.Sort();
        }
        
        public void Clear()
        {
            _actionWrappers.Clear();
        }
        
        public async UniTask ExecuteAll(TEventArgs eventArgs)
        {
            SortedSet<ActionWrapper<TEventArgs>> snapshot = new SortedSet<ActionWrapper<TEventArgs>>(_actionWrappers);
            foreach (var wrapper in snapshot)
            {
                LogEx.Log($"({wrapper.PrimaryPriority})Executing action {wrapper.action}");
                await wrapper.action.Invoke(eventArgs);
            }
            LogEx.Log($"[ExecQueue<{typeof(TEventArgs).Name}>] Executed {snapshot.Count} actions.");
        }
    }
}