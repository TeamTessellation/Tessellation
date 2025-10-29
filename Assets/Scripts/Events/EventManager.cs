
using Cardevil.Events;
using Core;
using Events.Core;
using Events.PriorityEvent;

namespace Cardevil.Events
{
    
    /// <summary>
    /// 이벤트 채널을 관리하는 클래스
    /// </summary>
    public class EventManager : Singleton<EventManager>, IClearable
    {
        public override bool IsDontDestroyOnLoad => false;

        protected override void AfterAwake()
        {
            Initialize();
        }
        
        PriorityEvent<ScoreChangedEventArgs> _scoreChangedEvent = new PriorityEvent<ScoreChangedEventArgs>();

        protected void Initialize()
        {
            _scoreChangedEvent = new PriorityEvent<ScoreChangedEventArgs>();
        }

        public void Clear()
        {
            _scoreChangedEvent.Clear();
        }

        
    }
}