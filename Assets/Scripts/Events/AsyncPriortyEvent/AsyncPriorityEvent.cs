using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Events.AsyncPriortyEvent;
using Events.Core;

namespace Events.AsyncPriorityEvent
{
    public class AsyncPriorityEvent : AsyncPriorityEventBase
    {
        private SortedList<int,AsyncEventHandler> _events = new ();
    
        private List<(int,AsyncEventHandler)> _eventsToAdd = new ();
        private List<(int,AsyncEventHandler)> _eventsToRemove = new ();
    
        public void AddListener(AsyncEventHandler listener, int priority = 0)
        {
            if (_isInvoking)
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
    
        public void RemoveListener(AsyncEventHandler listener, int priority)
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
    
        public void RemoveListener(AsyncEventHandler listener)
        {
            if (_isInvoking)
            {
                _eventsToRemove.Add((0, listener));
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
        
        public void Invoke()
        {
            InvokeAsync().Forget();
        }
    
        public async UniTask InvokeAsync()
        {
            _isInvoking = true;
            foreach (AsyncEventHandler e in _events.Values)
            {
                foreach (var handler in e.GetInvocationList())
                {
                    try
                    {
                        await ((AsyncEventHandler)handler)();
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Exception during event invocation: {ex}");
                    }
                }
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