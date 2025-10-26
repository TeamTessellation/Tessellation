using System;
using Core;
using DataAnalysis.Events;
using Machamy.Utils;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine.Analytics;
using UnityEngine.UnityConsent;

namespace DataAnalysis
{
    public enum AnalyticsEvent
    {
        game_start,
        item_acquired,
        game_over,
        level_start,
        level_complete
    }
    public class AnalyticsManager : Singleton<AnalyticsManager>
    {
        public override bool IsDontDestroyOnLoad => true;
        private IAnalyticsService Service => AnalyticsService.Instance;
        private async void Start()
        {
            try
            {
                await UnityServices.InitializeAsync();
                // Deprecated 지만, 6.2부터 적용된거라 일단 유지
                Service.StartDataCollection();

            }
            catch (Exception e)
            {
                LogEx.LogError($"Failed to initialize Analytics: {e.Message}");
            }
            
        }
         
        /// <summary>
        /// 커스텀 이벤트를 전송합니다.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="parameters"></param>
        /// 
        public void TrackCustomEvent(string eventName, params (string key, object value)[] parameters)
        {
            CustomEvent customEvent = new CustomEvent(eventName);
            foreach (var (key, value) in parameters)
            {
                customEvent.Add(key, value);
            }
            Service.RecordEvent(customEvent);
        }
        
        /// <summary>
        /// 커스텀 이벤트를 전송합니다.
        /// </summary>
        public void TrackCustomEvent(AnalyticsEvent eventName, params (string key, object value)[] parameters)
        {
            TrackCustomEvent(eventName.ToString(), parameters);
        }
        
        public void TrackGameStart()
        {
            var evt = new GameStartEvent();
            
            Service.RecordEvent(evt);
        }
        
        public void TrackItemAcquired(string itemId)
        {
            var evt = new ItemAcquiredEvent
            {
                ItemId = itemId
            };
            Service.RecordEvent(evt);
        }
        
        public void TrackGameOver(bool isWin, int score, string reason)
        {
            var evt = new GameOverEvent
            {
                IsWin = isWin,
                PlayTime = score,
                PlayerName = reason
            };
            Service.RecordEvent(evt);
        }
        
        public void TrackLevelStart(int levelNumber)
        {
            var evt = new LevelStartEvent
            {
                LevelNumber = levelNumber
            };
            Service.RecordEvent(evt);
        }
        
        public void TrackLevelComplete(int levelNumber, int earnedScore)
        {
            var evt = new LevelCompleteEvent
            {
                LevelNumber = levelNumber,
                EarnedScore = earnedScore
            };
            Service.RecordEvent(evt);
        }
        
    }
}