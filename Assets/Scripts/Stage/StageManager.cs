using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using ExecEvents;
using Machamy.DeveloperConsole.Attributes;
using Machamy.Utils;
using Player;
using UI;
using UI.OtherUIs;
using UnityEngine;

namespace Stage
{
    public class StageManager : Singleton<StageManager>
    {
        public override bool IsDontDestroyOnLoad => false;
        
        private TurnManager TurnManager => TurnManager.Instance;
        
        private StageModel _currentStage;


        
        [SerializeField] private bool _isStageCleared = false;

        [SerializeField] private int _currentStageIndex = 0;

        [Obsolete("아마 안쓸듯")]
        public bool IsStageCleared => _isStageCleared;
        
        private CancellationToken token;
        
        
        public StageModel CurrentStage
        {
            get => _currentStage;
            set => _currentStage = value;
        }

        public StageModel NextStage => CurrentStage.GetNextStageModel();
        
        /// <summary>
        /// 스테이지를 시작합니다.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void StartStage(CancellationToken cancellationToken)
        {
            token = cancellationToken;
            
            StartStageAsync(cancellationToken).Forget();
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
            
        
        private async UniTask StartStageAsync(CancellationToken cancellationToken)
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
            HandCanvas.Instance.gameObject.SetActive(true);
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
            
            await UniTask.Delay(1000);
            // 결과 팝업
            // 상점 파트
            LogEx.Log("Stage Ended.");
            // 스테이지 시작으로 돌아가기
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
            await UM.FailResultUI.ShowFailResult();
            
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

        public bool CheckStageFail()
        {
            // 턴 수 초과 확인
            PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;
            if (playerStatus.RemainingTurns <= 0 && !CheckStageClear())
            {
                return true;
            }
            // 핸드에 놓을 수 있는 타일이 없는지 확인
            if (HandManager.Instance.HandCount > 0 && !HandManager.Instance.CanPlace())
            {
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

        [ConsoleCommand("StageFail", "강제 스테이지 실패 처리")]
        public static void SetFail()
        {
            Instance.FailStage(Instance.token);
        }
    }
}