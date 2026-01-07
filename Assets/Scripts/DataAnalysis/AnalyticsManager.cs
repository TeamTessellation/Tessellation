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
                var consent = new ConsentState()
                {
                    AnalyticsIntent = ConsentStatus.Granted,
                    AdsIntent = ConsentStatus.Granted
                };
                EndUserConsent.SetConsentState(consent);
                LogEx.Log("Analytics initialized successfully.");
            }
            catch (Exception e)
            {
                LogEx.LogError($"Failed to initialize Analytics: {e.Message}");
            }
            
        }

        private void OnDisable()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                AnalyticsService.Instance.Flush();
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
        
        /// <summary>
        /// 스테이지 클리어 시 분석 데이터를 전송합니다.
        /// </summary>
        public void TrackStageClear(
            string currentStage,
            uint gameRandomSeed,
            uint stageRandomSeed,
            int stageBestPlacement,
            int stageScore,
            int stageClearedLines,
            int remainingTurns,
            int stageAbilityUseCount,
            int stageCoinsObtained,
            string currentItems,
            int currentCoins,
            int remainingActiveItemCount)
        {
            var evt = new StageClearEvent
            {
                CurrentStage = currentStage,
                GameRandomSeed = gameRandomSeed,
                StageRandomSeed = stageRandomSeed,
                StageBestPlacement = stageBestPlacement,
                StageScore = stageScore,
                StageClearedLines = stageClearedLines,
                RemainingTurns = remainingTurns,
                StageAbilityUseCount = stageAbilityUseCount,
                StageCoinsObtained = stageCoinsObtained,
                CurrentItems = currentItems,
                CurrentCoins = currentCoins,
                RemainingActiveItemCount = remainingActiveItemCount
            };
            Service.RecordEvent(evt);
        }
        
        /// <summary>
        /// 게임 종료 시 분석 데이터를 전송합니다.
        /// </summary>
        public void TrackGameEnd(
            string endedStage,
            uint gameRandomSeed,
            uint stageRandomSeed,
            int lastStageBestPlacement,
            int lastStageScore,
            int lastStageClearedLines,
            int lastStageRemainingTurns,
            int lastStageAbilityUseCount,
            int lastStageCoinsObtained,
            string currentItems,
            int currentCoins,
            int remainingActiveItemCount,
            int bestScorePlacement,
            int bestStageScore,
            int totalScore,
            int totalClearedLines,
            int totalAbilityUseCount,
            int bestStageCoinsObtained,
            int totalObtainedCoins,
            int totalReviveCount)
        {
            var evt = new GameEndEvent
            {
                EndedStage = endedStage,
                GameRandomSeed = gameRandomSeed,
                StageRandomSeed = stageRandomSeed,
                LastStageBestPlacement = lastStageBestPlacement,
                LastStageScore = lastStageScore,
                LastStageClearedLines = lastStageClearedLines,
                LastStageRemainingTurns = lastStageRemainingTurns,
                LastStageAbilityUseCount = lastStageAbilityUseCount,
                LastStageCoinsObtained = lastStageCoinsObtained,
                CurrentItems = currentItems,
                CurrentCoins = currentCoins,
                RemainingActiveItemCount = remainingActiveItemCount,
                BestScorePlacement = bestScorePlacement,
                BestStageScore = bestStageScore,
                TotalScore = totalScore,
                TotalClearedLines = totalClearedLines,
                TotalAbilityUseCount = totalAbilityUseCount,
                BestStageCoinsObtained = bestStageCoinsObtained,
                TotalObtainedCoins = totalObtainedCoins,
                TotalReviveCount = totalReviveCount
            };
            Service.RecordEvent(evt);
        }
        
    }
}