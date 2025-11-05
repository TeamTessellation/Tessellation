using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Events.Core
{
    public abstract class EventArgs : System.EventArgs
    {
        /// <summary>
        /// 이벤트 초기화
        /// </summary>
        public abstract void Clear();
    }
    
    [MustDisposeResource]
    public abstract class EventArgs<T> : EventArgs, IDisposable where T : EventArgs<T>, new()
    {
        private static Queue<T> pool = new Queue<T>();
        protected bool isInPool = false;
        
        /// <summary>
        /// 이벤트가 풀에 있는지 여부
        /// </summary>
        public bool IsInPool => isInPool;
        /// <summary>
        /// 이벤트가 유효한지 여부
        /// </summary>
        public bool IsValid => !isInPool;
        
        protected EventArgs()
        {
        }

        /// <summary>
        /// 이벤트를 풀에서 가져옵니다.
        /// </summary>
        /// <returns></returns>
        // 반드시 Dispose
        public static T Get()
        {
            var res = pool.Count > 0 ? pool.Dequeue() : new T();
            res.isInPool = false;
            return res;
        }

        /// <summary>
        /// 이벤트를 초기화하고 풀로 되돌림
        /// </summary>
        public void Release()
        {
            Clear();
            pool.Enqueue((T)this);
            isInPool = true;
        }

        /// <summary>
        /// 이벤트를 초기화하고 풀로 되돌림
        /// </summary>
        public void Dispose()
        {
            Release();
        }
    }
    
}