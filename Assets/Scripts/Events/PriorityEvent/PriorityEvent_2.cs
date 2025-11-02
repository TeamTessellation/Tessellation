using System;
using System.Collections.Generic;

namespace Events.PriorityEvent
{
    /// <inheritdoc cref="PriorityEventBase"/>
    public class PriorityEvent<T0,T1> : PriorityEventBase
    {
        private SortedList<int,Action<T0,T1>> _events = new SortedList<int, Action<T0,T1>>();
    
        private List<(int,Action<T0,T1>)> _eventsToAdd = new ();
        private List<(int,Action<T0,T1>)> _eventsToRemove = new ();
    
    
        public void AddListener(Action<T0,T1> listener, int priority= 0)
        {
            if (priority == MAGIC_NUMBER)
            {
                throw new ArgumentException($"Priority cannot be {MAGIC_NUMBER}");
            }
            if(_isInvoking)
            {
                _eventsToAdd.Add((priority, listener));
                return;
            }
            if (_events.ContainsKey(priority))
            {
                _events[priority] += listener;
            }
            else
            {
                _events.Add(priority, listener);
            }
        }
    
        public void RemoveListener(Action<T0,T1> listener, int priority)
        {
            if (_isInvoking)
            {
                _eventsToRemove.Add((priority, listener));
                return;
            }
            if (priority == MAGIC_NUMBER)
            {
                RemoveListener(listener);
            }
            if (_events.ContainsKey(priority))
            {
                _events[priority] -= listener;
            }
        }
    
        public void RemoveListener(Action<T0,T1> listener)
        {
            if (_isInvoking)
            {
                _eventsToRemove.Add((MAGIC_NUMBER, listener));
                return;
            }
            var keys = new List<int>(_events.Keys);
            foreach (var k in keys)
            {
                _events[k] -= listener;
            }
        }
    
        public void ClearListeners(int priority)
        {
            if (_isInvoking)
            {
                _keysToClear.Add(priority);
                return;
            }
            if (_events.ContainsKey(priority))
            {
                _events.Remove(priority);
            }
        }
    
        public void ClearListeners()
        {
            if (_isInvoking)
            {
                _clearAll = true;
                return;
            }
            _events.Clear();
        }
    
        public void Invoke(T0 arg,T1 arg1)
        {
            _isInvoking = true;
            foreach (var e in _events.Values)
            {
                e?.Invoke(arg,arg1);
            }
            _isInvoking = false;
        
            foreach (var (priority, listener) in _eventsToAdd)
            {
                AddListener(listener, priority);
            }
        
            foreach (var (priority, listener) in _eventsToRemove)
            {
                RemoveListener(listener, priority);
            }
        
            _eventsToAdd.Clear();
            _eventsToRemove.Clear();

            if (_clearAll)
            {
                _events.Clear();
                _clearAll = false;
            }

            foreach (var key in _keysToClear)
            {
                ClearListeners(key);
            }
            _keysToClear.Clear();
        }

        public override void Clear() => ClearListeners();
    }
}
