using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Events.Core;
using Machamy.Utils;
using UnityEngine.Pool;

namespace ExecEvents
{
    /// <summary>
    /// 우선순위 실행 큐 클래스<br/>
    /// 각 액션은 우선순위를 지정할 수 있으며, 우선순위에 따라 실행됩니다.
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    public class ExecQueue<TEventArgs> :IReadOnlyList<ExecQueue<TEventArgs>.ActionWrapper> where TEventArgs : ExecEventArgs<TEventArgs>, new()
    {
        /// <summary>
        /// 액션 래퍼 클래스
        /// </summary>
        public class ActionWrapper : IComparable<ActionWrapper>, IComparable, IDisposable
        {
            private static ObjectPool<ActionWrapper> _pool 
                = new ObjectPool<ActionWrapper>
                (() => new ActionWrapper(),
                    actionOnRelease: wrapper => wrapper.Clear());
            
            internal int _primaryPriority = (int) ExecPriority.Normal;
            internal List<int> _extraPriorities = new List<int>();
            internal int _enqueuedOrder = int.MaxValue;
            
            internal ExecAction<TEventArgs> action;
            
            public int PrimaryPriority => _primaryPriority;
            public IReadOnlyList<int> ExtraPriorities => _extraPriorities;

            public int EnqueuedOrder
            {
                get => _enqueuedOrder;
                set => _enqueuedOrder = value;
            }
            
            
            private ActionWrapper()
            {
            }
            
            /// <summary>
            /// 액션 래퍼를 가져옵니다.
            /// 해당 함수는 ExecQueue외부에서 사용을 상정하지 않습니다.
            /// </summary>
            /// <remarks>
            /// EnqueuedOrder는 자동으로 설정되지 않으므로, 큐에 등록할 때 반드시 설정해야 합니다.
            /// 풀링을 사용하므로, 사용 후에는 반드시 <see cref="Dispose"/>를 호출하여 반환해야 합니다.
            /// 또한, 동일한 액션 래퍼를 여러 큐에 등록하지 마십시오.
            /// </remarks>
            /// <param name="action"></param>
            /// <param name="primaryPriority"></param>
            /// <param name="extraPriorities"></param>
            /// <returns></returns>
            public static ActionWrapper Get(ExecAction<TEventArgs> action, int primaryPriority, params int[] extraPriorities)
            {
                var wrapper = _pool.Get();
                wrapper.action = action;
                wrapper._primaryPriority = primaryPriority;
                wrapper._extraPriorities.Clear();
                if (extraPriorities != null && extraPriorities.Length > 0)
                {
                    wrapper._extraPriorities.AddRange(extraPriorities);
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
                var primaryPriorityComparison = _primaryPriority.CompareTo(other._primaryPriority);
                if (primaryPriorityComparison != 0) return primaryPriorityComparison;
                
                // Extra Priorities 비교 (순서대로)
                int minExtraCount = Math.Min(_extraPriorities.Count, other._extraPriorities.Count);
                for (int i = 0; i < minExtraCount; i++)
                {
                    var extraComparison = _extraPriorities[i].CompareTo(other._extraPriorities[i]);
                    if (extraComparison != 0) return extraComparison;
                }
                
                // Extra Priorities 개수가 다르면 더 많은 쪽이 뒤로 (더 낮은 우선순위)
                var extraCountComparison = _extraPriorities.Count.CompareTo(other._extraPriorities.Count);
                if (extraCountComparison != 0) return extraCountComparison;
                
                // Enqueued Order 비교
                return _enqueuedOrder.CompareTo(other._enqueuedOrder);
            }


            public void Clear()
            {
                action = null;
                _primaryPriority = (int) ExecPriority.Normal;
                _extraPriorities.Clear();
                _enqueuedOrder = int.MaxValue;
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
            wrapper._enqueuedOrder = _actionWrappers.Count;
            _actionWrappers.Add(wrapper);
            _dirty = true;
        }
        
        /// <summary>
        /// 이진탐색을 통해 우선순위에 따라 액션을 큐에 추가합니다.
        /// dirty플래그를 사용하지 않는 대신, 등록 시간이 오래걸릴 수 있습니다.
        /// 이미 정렬된 상태에서 소수의 핸들러를 등록할 때 유용합니다.
        /// </summary>
        public void EnqueueBinarySearch(int priority, ExecAction<TEventArgs> action, params int[] extraPriorities)
        {
            if (_dirty)
            {
                throw new InvalidOperationException("Cannot use EnqueueBinarySearch when the queue is dirty. Please sort the queue first.");
            }
            var wrapper = ActionWrapper.Get(action, priority, extraPriorities);
            wrapper._enqueuedOrder = _actionWrappers.Count;
            int index = _actionWrappers.BinarySearch(wrapper);
            // 음수라면 못찾은 경우.
            if (index < 0)
            {
                index = ~index; 
            }
            _actionWrappers.Insert(index, wrapper);
            // _dirty = false; // 이미 정렬된 상태이므로 dirty 플래그를 변경하지 않음
        }
        
        /// <summary>
        /// 이진탐색을 통해 우선순위에 따라 액션을 큐에 추가합니다.
        /// </summary>
        public void EnqueueSafeBinarySearch(int priority, ExecAction<TEventArgs> action, params int[] extraPriorities)
        {
            if (_dirty)
            {
                SortByPriority();
            }
            EnqueueBinarySearch(priority, action, extraPriorities);
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
        /// <param name="cancellationToken"></param>
        public async UniTask ExecuteAll(TEventArgs eventArgs, CancellationToken cancellationToken = default)
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
                    LogEx.Log($"({wrapper._primaryPriority})Executing action {wrapper.action}");
                    await wrapper.action.Invoke(eventArgs);
                    
                    if (eventArgs.BreakChain)
                    {
                        LogEx.Log($"[ExecQueue<{typeof(TEventArgs).Name}>] Chain broken at action {wrapper.action}");
                        break;
                    }
                    if (cancellationToken.IsCancellationRequested)
                    {
                        LogEx.Log($"[ExecQueue<{typeof(TEventArgs).Name}>] Execution cancelled.");
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