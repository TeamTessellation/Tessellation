using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Events.Core;
using Machamy.Utils;
using UnityEngine.Pool;

namespace PriortyExecEvent
{
    /// <summary>
    /// 우선순위 실행 큐 클래스
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    public class ExecQueue<TEventArgs> :IReadOnlyList<ExecQueue<TEventArgs>.ActionWrapper> where TEventArgs : ExecEventArgs<TEventArgs>, new()
    {
        /// <summary>
        /// 우선순위 실행 액션 래퍼 클래스
        /// </summary>
        public class ActionWrapper : IComparable<ActionWrapper>, IComparable, IDisposable
        {
            private static ObjectPool<ActionWrapper> _pool 
                = new ObjectPool<ActionWrapper>
                (() => new ActionWrapper(),
                    actionOnRelease: wrapper => wrapper.Clear());
            
            internal int PrimaryPriority = (int) ExecPriority.Normal;
            internal List<int> ExtraPriorities = new List<int>();
            internal int EnqueuedOrder = int.MaxValue;
            
            internal ExecAction<TEventArgs> action;
            
            private ActionWrapper()
            {
            }
            
            public static ActionWrapper Get(ExecAction<TEventArgs> action, int primaryPriority, params int[] extraPriorities)
            {
                var wrapper = _pool.Get();
                wrapper.action = action;
                wrapper.PrimaryPriority = primaryPriority;
                if (extraPriorities != null && extraPriorities.Length > 0)
                {
                    wrapper.ExtraPriorities.AddRange(extraPriorities);
                }
                return wrapper;
            }
            
            public static implicit operator ExecAction<TEventArgs>(ActionWrapper wrapper)
            {
                return wrapper.action;
            }

            public int CompareTo(object obj)
            {
                if (obj is null) return 1;
                if (ReferenceEquals(this, obj)) return 0;
                return obj is ActionWrapper other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ExecQueue<TEventArgs>.ActionWrapper)}");
            }

            public int CompareTo(ActionWrapper other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (other is null) return 1;
                
                // Primary Priority 비교
                var primaryPriorityComparison = PrimaryPriority.CompareTo(other.PrimaryPriority);
                if (primaryPriorityComparison != 0) return primaryPriorityComparison;
                
                // Extra Priorities 비교 (순서대로)
                int minExtraCount = Math.Min(ExtraPriorities.Count, other.ExtraPriorities.Count);
                for (int i = 0; i < minExtraCount; i++)
                {
                    var extraComparison = ExtraPriorities[i].CompareTo(other.ExtraPriorities[i]);
                    if (extraComparison != 0) return extraComparison;
                }
                
                // Extra Priorities 개수가 다르면 더 많은 쪽이 뒤로 (더 낮은 우선순위)
                var extraCountComparison = ExtraPriorities.Count.CompareTo(other.ExtraPriorities.Count);
                if (extraCountComparison != 0) return extraCountComparison;
                
                // Enqueued Order 비교
                return EnqueuedOrder.CompareTo(other.EnqueuedOrder);
            }


            public void Clear()
            {
                action = null;
                PrimaryPriority = (int) ExecPriority.Normal;
                ExtraPriorities.Clear();
                EnqueuedOrder = int.MaxValue;
            }
            
            public void Dispose()
            {
                _pool.Release(this);
            }
            
        }

        // 어차피 실시간으로 우선순위 변경이 필요 없으므로 리스트로 구현
        // private SortedSet<ActionWrapper> _actionWrappers = new SortedSet<ActionWrapper>();

        private List<ActionWrapper> _actionWrappers = new List<ActionWrapper>();
        private bool _dirty = true;
        private readonly List<ActionWrapper> _snapshot = new List<ActionWrapper>();
        private bool _isExecuting = false;
        
        
        public bool IsExecuting => _isExecuting;
        
        /// <summary>
        /// 우선순위에 따라 액션을 큐에 추가합니다.
        /// </summary>
        /// <param name="priority">우선순위</param>
        /// <param name="action">실행할 액션</param>
        /// <param name="extraPriorities">추가 우선순위 (Primary Priority가 같을 때 순서대로 비교됩니다)</param>
        public void Enqueue(ExecPriority priority, ExecAction<TEventArgs> action, params int[] extraPriorities)
        {
            Enqueue((int)priority, action, extraPriorities);
        }
        
        /// <summary>
        /// 우선순위에 따라 액션을 큐에 추가합니다.
        /// </summary>
        /// <param name="priority">우선순위</param>
        /// <param name="action">실행할 액션</param>
        /// <param name="extraPriorities">추가 우선순위 (Primary Priority가 같을 때 순서대로 비교됩니다)</param>
        public void Enqueue(int priority, ExecAction<TEventArgs> action, params int[] extraPriorities)
        {
            var wrapper = ActionWrapper.Get(action, priority, extraPriorities);
            wrapper.EnqueuedOrder = _actionWrappers.Count;
            _actionWrappers.Add(wrapper);
            _dirty = true;
        }
        
        public void Remove(ExecAction<TEventArgs> action)
        {
            for (int i = 0; i < _actionWrappers.Count; i++)
            {
                if (_actionWrappers[i].action == action)
                {
                    _actionWrappers[i].Dispose();
                    _actionWrappers.RemoveAt(i);
                    _dirty = true;
                    return;
                }
            }
        }
        
        /// <summary>
        /// 내부 리스트의 용량을 설정합니다.
        /// </summary>
        /// <param name="capacity"></param>
        public void SetCapacity(int capacity)
        {
            if (_actionWrappers.Capacity < capacity)
            {
                _actionWrappers.Capacity = capacity;
            }
        }
        
        /// <summary>
        /// 우선순위에 따라 정렬합니다.
        /// </summary>
        public void SortByPriority()
        {
            _actionWrappers.Sort();
            _snapshot.Clear();
            _snapshot.AddRange(_actionWrappers);
            _dirty = false;
        }
        
        public void SortByPriorityIfDirty()
        {
            if (_dirty)
            {
                SortByPriority();
            }
        }
        
        /// <summary>
        /// 큐를 초기화합니다.
        /// </summary>
        public void Clear()
        {
            foreach (var wrapper in _actionWrappers)
            {
                wrapper.Dispose();
            }
            _actionWrappers.Clear();
            _snapshot.Clear();
            _dirty = true;
        }
        

        /// <summary>
        /// 모든 액션을 우선순위에 따라 실행합니다.
        /// </summary>
        /// <param name="eventArgs"></param>
        public async UniTask ExecuteAll(TEventArgs eventArgs)
        {
            if (_dirty)
            {
                SortByPriority();
            }
            
            _isExecuting = true;
            try
            {
                if(eventArgs.BreakChain) 
                {
                    LogEx.Log($"[ExecQueue<{typeof(TEventArgs).Name}>] Chain already broken before execution.");
                    return;
                }
                foreach (var wrapper in _snapshot)
                {
                    LogEx.Log($"({wrapper.PrimaryPriority})Executing action {wrapper.action}");
                    await wrapper.action.Invoke(eventArgs);
                    
                    if (eventArgs.BreakChain)
                    {
                        LogEx.Log($"[ExecQueue<{typeof(TEventArgs).Name}>] Chain broken at action {wrapper.action}");
                        break;
                    }
                }
            }
            finally
            {
                _isExecuting = false;
            }
            
            LogEx.Log($"[ExecQueue<{typeof(TEventArgs).Name}>] Executed {_snapshot.Count} actions.");
        }

        public IEnumerator<ActionWrapper> GetEnumerator()
        {
            return (_dirty ? _actionWrappers : _snapshot).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _dirty ? _actionWrappers.Count : _snapshot.Count;

        public ExecQueue<TEventArgs>.ActionWrapper this[int index]
        {
            get => _dirty ? _actionWrappers[index] : _snapshot[index];
        }
    }
}