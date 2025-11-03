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
    public class ExecQueue<TEventArgs> :IReadOnlyList<ExecAction<TEventArgs>> where TEventArgs : ExecEventArgs<TEventArgs>, new()
    {
        /// <summary>
        /// 우선순위 실행 액션 래퍼 클래스
        /// </summary>
        protected class ActionWrapper<TWrapperEventArgs> : IComparable<ActionWrapper<TWrapperEventArgs>>, IComparable, IDisposable where TWrapperEventArgs : ExecEventArgs<TWrapperEventArgs>, new()
        {
            private static ObjectPool<ActionWrapper<TWrapperEventArgs>> _pool 
                = new ObjectPool<ActionWrapper<TWrapperEventArgs>>
                (() => new ActionWrapper<TWrapperEventArgs>(),
                    actionOnRelease: wrapper => wrapper.Clear());
            
            public int PrimaryPriority;
            public int EnqueudOrder = 0;
            public ExecAction<TWrapperEventArgs> action;
            
            private ActionWrapper()
            {
            }
            
            private ActionWrapper(ExecAction<TWrapperEventArgs> action, int primaryPriority)
            {
                this.action = action;
                PrimaryPriority = primaryPriority;
            }
            
            public static ActionWrapper<TWrapperEventArgs> Get(ExecAction<TWrapperEventArgs> action, int primaryPriority)
            {
                var wrapper = _pool.Get();
                wrapper.action = action;
                wrapper.PrimaryPriority = primaryPriority;
                return wrapper;
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
                if (PrimaryPriority != other.PrimaryPriority)
                {
                    return PrimaryPriority.CompareTo(other.PrimaryPriority);
                }
                return EnqueudOrder.CompareTo(other.EnqueudOrder);
            }


            public void Clear()
            {
                action = null;
                PrimaryPriority = 0;
                EnqueudOrder = 0;
            }
            
            public void Dispose()
            {
                _pool?.Dispose();
            }
        }

        // 어차피 실시간으로 우선순위 변경이 필요 없으므로 리스트로 구현
        // private SortedSet<ActionWrapper> _actionWrappers = new SortedSet<ActionWrapper>();

        private List<ActionWrapper<TEventArgs>> _actionWrappers = new List<ActionWrapper<TEventArgs>>();
        private bool _dirty = true;
        private readonly List<ActionWrapper<TEventArgs>> _snapshot = new List<ActionWrapper<TEventArgs>>();
        private bool _isExecuting = false;
        
        
        public bool IsExecuting => _isExecuting;
        
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
            var wrapper = ActionWrapper<TEventArgs>.Get(action, priority);
            wrapper.EnqueudOrder = _actionWrappers.Count;
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
        

        public async UniTask ExecuteAll(TEventArgs eventArgs)
        {
            if (_dirty)
            {
                SortByPriority();
            }
            
            foreach (var wrapper in _snapshot)
            {
                LogEx.Log($"({wrapper.PrimaryPriority})Executing action {wrapper.action}");
                await wrapper.action.Invoke(eventArgs);
            }
            LogEx.Log($"[ExecQueue<{typeof(TEventArgs).Name}>] Executed {_snapshot.Count} actions.");
            
        }

        public IEnumerator<ExecAction<TEventArgs>> GetEnumerator()
        {
            foreach (var wrapper in _actionWrappers)
            {
                yield return wrapper.action;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _actionWrappers.Count;

        public ExecAction<TEventArgs> this[int index]
        {
            get
            {
                if (index < 0 || index >= _actionWrappers.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }

                return _actionWrappers[index].action;
            }
        }
    }
}