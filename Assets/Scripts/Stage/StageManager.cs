using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using ExecEvents;
using Machamy.DeveloperConsole.Attributes;
using Machamy.Utils;
using Player;
using SaveLoad;
using UI;
using UI.Components;
using UI.OtherUIs;
using UnityEngine;

namespace Stage
{
    
    public class StageManager : Singleton<StageManager>, ISaveTarget
    {
        public override bool IsDontDestroyOnLoad => false;
        
        private TurnManager TurnManager => TurnManager.Instance;
        
        private StageModel _currentStage;


        
        [SerializeField] private bool _isStageCleared = false;

        // [SerializeField] private int _currentStageIndex = 0;

        [Obsolete("아마 안쓸듯")]
        public bool IsStageCleared => _isStageCleared;
        
        private CancellationToken token;
        
        
        public StageModel CurrentStage
        {
            get => _currentStage;
            set => _currentStage = value;
        }

        public StageModel NextStage => CurrentStage.GetNextStageModel();


        private void Start()
        {
             
        }

        /// <summary>
        /// 스테이지를 시작합니다.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void StartStage(CancellationToken cancellationToken, bool isContinue = false)
        {
            token = cancellationToken;
            
            StartStageAsync(cancellationToken, isContinue).Forget();
        }
        
        /// <summary>
        /// 스테이지를 정상적으로 종료합니다.
        /// </summary>
        public void EndStage(CancellationToken cancellationToken)
        {
            EndStageAsync(cancellationToken).Forget();
        }

        /// <summary>
        /// 스테이지를 실패로 처리합니다.
        /// </summary>
        public void FailStage(CancellationToken cancellationToken)
        {
            LogEx.Log("Stage Failed.");
            FailStageAsync(cancellationToken).Forget();
        }
        
        /// <summary>
        /// 다음 스테이지로 진행합니다.
        /// </summary>
        private void GoToNextStage()
        {
            _currentStage = NextStage;
            LogEx.Log($"Proceeding to next stage: {_currentStage.StageName}");
        }
            
        
        private async UniTask StartStageAsync(CancellationToken cancellationToken, bool isContinue = false)
        {
            if (token == default)
            {
                LogEx.LogError("CancellationToken is not set. Cannot start stage.");
                return;
            }
            var UM = UIManager.Instance;
            _isStageCleared = false;
            LogEx.Log("Stage Starting...");
             /*
              * 스테이지 시작 화면 표시
              */
            
             // UM.SwitchMainToGameUI(); 타이밍 문제로 인해 주석처리
             UIManager.Instance.GameUI.Show();
             
             await UM.StageInfoUI.ShowInfoRoutine(CurrentStage, token);
             
            /*
             * 화면 완전히 가려짐
             * 스테이지 초기화
             */
            LogEx.Log("Stage Initializing...");
             
            PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;
            
            
            // 6각형 타일 맵 초기화
            Field.Instance.gameObject.SetActive(true);
            Field.Instance.ResetField(playerStatus.FieldSize);
            
            // 핸드 초기화
            HandCanvas handCanvas = HandCanvas.Instance;
            handCanvas.Show();
            HandManager.Instance.ResetHand(playerStatus.HandSize);
            // 점수 초기화
            ScoreManager.Instance.Reset();
            
            // 목표 점수 설정
            // # 자동으로 할당됨
            // # TargetScore => StageManager.Instance.CurrentStage.StageTarget
            
            // # 자동으로 할당됨
            // TurnManager.Instance.MaxTurnCount => StageManager.Instance.CurrentStage.StageMaxTurn;
            // TurnManager::StartTurnLoop(){ ... _currentTurn = 1;  ... }
            
            // 제약 적용
            // # 스테이지 모델에 정의된 제약 조건들을 필드에 적용
            // # TODO : 현재 없음
            
            // UI 숨기기
            UM.MainTitleUI.Hide();
            UM.FailResultUI.Hide();
            UM.InGameUI.Show();

            
            using var initStageArgs = StageStartEventArgs.Get();
            initStageArgs.StageModel = _currentStage;
            await ExecEventBus<StageStartEventArgs>.InvokeMerged(initStageArgs);
            
            
            
            /*
             *  스테이지 시작화면 제거
             */
            await UM.StageInfoUI.HideInfoRoutine(token);
             
            await UniTask.Delay(150, cancellationToken: token);
             
            LogEx.Log("Stage Initialized.");
             
            /*
            * 스테이지 파트 시작
            */
            TurnManager.StartTurnLoop();
        }
        
        /// <summary>
        /// 스테이지를 정상적으로 종료합니다.
        /// 계산도 포함됩니다.
        /// </summary>
        /// <param name="cancellationToken"></param>
        private async UniTask EndStageAsync(CancellationToken cancellationToken)
        {
            if (token == CancellationToken.None)
            {
                LogEx.LogError("CancellationToken is not set. Cannot end stage.");
                return;
            }
            LogEx.Log("Stage Ending...");
            /*
             * 스테이지 종료 처리
             */
            using var endStageArgs = StageEndEventArgs.Get();
            await ExecEventBus<StageEndEventArgs>.InvokeMerged(endStageArgs);
            /*
             * PlayerStatus에서 처리해야할 것들 처리
             * Best/Total Score 갱신, 이자 등
             */
            CoinCounter coinCounter = UIManager.Instance.InGameUI.CoinCounter;
            coinCounter.autoUpdate = false;
            
            // 점수 갱신
            PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;
            playerStatus.BestStageScore = Math.Max(playerStatus.BestStageScore, playerStatus.CurrentStageScore);
            playerStatus.TotalScore += playerStatus.CurrentStageScore;
            
            // 코인 이자 지급
            int interest = (int)(playerStatus.CurrentCoins * playerStatus.CoinInterestRate);
            interest = Math.Clamp(interest, 0, playerStatus.MaxInterestCoins);
            playerStatus.StageCoinsObtained += interest;
            playerStatus.StageInterestEarnedCoins = interest;
            playerStatus.StageCoinsObtained += playerStatus.StageClearedLines;
            playerStatus.StageCoinsObtained += playerStatus.RemainingTurns;
            
            playerStatus.CurrentCoins += playerStatus.StageCoinsObtained;
            LogEx.Log($"Stage Coins Obtained: {playerStatus.StageCoinsObtained} (Interest: {interest}, Cleared Lines: {playerStatus.StageClearedLines}, Remaining Turns: {playerStatus.RemainingTurns})");
            
            // Best/Total Stage Clear 갱신
            playerStatus.BestScorePlacement = Math.Max(playerStatus.BestScorePlacement, playerStatus.StageBestPlacement);
            playerStatus.BestStageClearedLines = Math.Max(playerStatus.BestStageClearedLines, playerStatus.StageClearedLines);
            playerStatus.BestStageAbilityUseCount = Math.Max(playerStatus.BestStageAbilityUseCount, playerStatus.StageAbilityUseCount);
            playerStatus.BestStageCoinsObtained = Math.Max(playerStatus.BestStageCoinsObtained, playerStatus.StageCoinsObtained);
            
            playerStatus.TotalObtainedCoins += playerStatus.StageCoinsObtained;
            playerStatus.TotalClearedLines += playerStatus.StageClearedLines;
            playerStatus.TotalAbilityUseCount += playerStatus.StageAbilityUseCount;
            playerStatus.TotalInterestEarnedCoins += playerStatus.StageInterestEarnedCoins;
            
            
            // await UniTask.Delay(1000);
            // 결과 팝업
            ClearResultUI clearResultUI = UIManager.Instance.ClearResultUI;

            
            await UniTask.WhenAny(clearResultUI.ShowClearResultsAsync(token), clearResultUI.WaitForSkipButtonAsync(token));
            await clearResultUI.WaitForSkipButtonAsync(token);
            
            // 만약 스킵이 눌렸다면 즉시 결과창 닫기
            clearResultUI.gameObject.SetActive(false);
            
            // 코인 카운터 스킵된 경우 처리
            coinCounter.autoUpdate = true;
            coinCounter.KillTween();
            coinCounter.DoCount(playerStatus.CurrentCoins, 0.2f, false);
            
            // 상점 파트
            ShopUI shopUI = UIManager.Instance.ShopUI;
            
            await shopUI.ShowShopItemAsync(token);
            await shopUI.WaitForSkipButtonAsync(token);
            
            shopUI.Hide();
            
            LogEx.Log("Stage Ended.");
            // 스테이지 시작으로 돌아가기
            
            /*
             * PlayerStatus의 Stage값 초기화, 
             */
            for(PlayerStatus.VariableKey key = PlayerStatus.StageStart;
                key <= PlayerStatus.StageEnd;
                key++)
            {
                playerStatus[key.ToString()].IntValue = 0;
            }
            
            
            GoToNextStage();
            StartStage(token);
            
        }
        
        private async UniTask FailStageAsync(CancellationToken cancellationToken)
        {
            
            LogEx.Log("Stage Failing...");
            /*
             * 스테이지 실패 처리
             */
            using var failStageArgs = StageFailEventArgs.Get();
            await ExecEventBus<StageFailEventArgs>.InvokeMerged(failStageArgs);
            var UM = UIManager.Instance;
            // UM.InGameUI.Hide();
            
            // 남아 있는 Stage 관련 값 집계
            // Fail에서는 실패한 스테이지의  BestStageAbilityUseCount 같은건 갱신 안하므로 별도로 작성함
            
            PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;
            playerStatus.BestStageScore = Math.Max(playerStatus.BestStageScore, playerStatus.CurrentStageScore);
            // Score는 ScoreManager에서 이미 TotalScore에 반영됨
            // TODO : 개선 필요
            
            playerStatus.TotalObtainedCoins += playerStatus.StageCoinsObtained;
            playerStatus.TotalClearedLines += playerStatus.StageClearedLines;
            playerStatus.TotalAbilityUseCount += playerStatus.StageAbilityUseCount;
            playerStatus.TotalInterestEarnedCoins += playerStatus.StageInterestEarnedCoins;
            
            playerStatus.BestScorePlacement = Math.Max(playerStatus.BestScorePlacement, playerStatus.StageBestPlacement);
            
            
            
            await UM.FailResultUI.ShowFailResult();
            
        }
        public void RestartCurrentStage(CancellationToken cancellationToken)
        {
            LogEx.Log("Restarting Current Stage...");
            ResetStage();
            StartStage(cancellationToken);
        }
        
        public bool CheckStageClear()
        {
            // 목표 점수 도달 확인
            if (_currentStage.StageTargetScore <= ScoreManager.Instance.CurrentScore)
            {
                _isStageCleared = true;
            }
            else
            {
                _isStageCleared = false;
            }
            
            return _isStageCleared;
        }

        public bool CheckStageFail(bool includeTurnLimit)
        {
            // 턴 수 초과 확인
            PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;
            if (includeTurnLimit)
            {
                if (playerStatus.RemainingTurns <= 0 && !CheckStageClear())
                {
                    LogEx.Log("Stage Failed: No remaining turns.");
                    return true;
                }
            }
            // 핸드에 놓을 수 있는 타일이 없는지 확인
            if (HandManager.Instance.HandCount > 0 && !HandManager.Instance.CanPlace())
            {
                LogEx.Log("Stage Failed: No placeable tiles in hand.");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 스테이지를 초기 상태로 재설정합니다.
        /// </summary>
        public void ResetStage()
        {
            /*
             *  1. 스테이지 초기화
             *  2. 필드, 핸드, 점수 초기화
             */
            _isStageCleared = false;
            Field.Instance.ResetField(GameManager.Instance.PlayerStatus.FieldSize);
            HandManager.Instance.ResetHand(GameManager.Instance.PlayerStatus.HandSize);
            ScoreManager.Instance.Reset();
        }

        public Guid Guid { get; init; } = Guid.NewGuid();
        public void LoadData(GameData data)
        {
            if (data.CurrentStage == null)
            {
                LogEx.LogWarning("No stage data found in save data.");
                return;
            }
            // 같은 스테이지인 경우
            if (_currentStage != null && _currentStage.StageIdentifiers == data.CurrentStage)
            {
                LogEx.Log("Same stage detected. No need to load stage data.");
                return;
            }
            CurrentStage = StageModel.CreateModel(data.CurrentStage[0], data.CurrentStage[1]);
            LogEx.Log($"Loaded stage data: {CurrentStage.StageName}");
        }

        public void SaveData(ref GameData data)
        {
            data.CurrentStage = _currentStage.StageIdentifiers;
        }

        [ConsoleCommand("StageFail", "강제 스테이지 실패 처리")]
        public static void SetFail()
        {
            Instance.FailStage(Instance.token);
        }

        [ConsoleCommand("StageClear", "강제 스테이지 클리어 처리")]
        public static void SetClear()
        {
            Instance.EndStage(Instance.token);
        }
    }
}