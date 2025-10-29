using System;
using System.Collections.Generic;

namespace Events.Core
{
    public abstract class EventArgs : System.EventArgs
    {
        /// <summary>
        /// 이벤트 초기화
        /// </summary>
        public abstract void Clear();
    }
    public abstract class EventArgs<T> : EventArgs, IDisposable where T : EventArgs<T>, new()
    {
        private static Queue<T> pool = new Queue<T>();
        protected bool isUsed = false;
        
        /// <summary>
        /// 이벤트가 이미 사용 되었는지 여부
        /// </summary>
        public bool IsUsed => isUsed;
        
        protected EventArgs()
        {
        }

        /// <summary>
        /// 이벤트를 풀에서 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public static T Get()
        {
            var res = pool.Count > 0 ? pool.Dequeue() : new T();
            res.isUsed = false;
            return res;
        }

        /// <summary>
        /// 이벤트를 초기화하고 풀로 되돌림
        /// </summary>
        public void Release()
        {
            Clear();
            pool.Enqueue((T)this);
            isUsed = true;
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