
using System.Collections.Generic;
using Events.Core;

namespace Events.PriorityEvent
{
    /// <summary>
    /// 우선순위 이벤트
    /// priority의 값이 낮을수록 먼저 호출된다.
    /// </summary>
    /// <remarks> 0은 가장 기본적인 이벤트가 등록되어야함. ex : 입장이벤트의 경우, 입장 처리 부분</remarks>
    public abstract class PriorityEventBase : IClearable
    {
        protected const int MAGIC_NUMBER = -321322;
        
        protected bool _isInvoking = false;
        protected List<int> _keysToClear = new List<int>();
        protected bool _clearAll = false;
        public abstract void Clear();
    }
}
