using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using ExecEvents;
using Machamy.Utils;
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
            
             UM.SwitchMainToGameUI();
             await UM.StageInfoUI.ShowInfoRoutine(CurrentStage, token);
             
             /*
              * 스테이지 초기화
              */
             LogEx.Log("Stage Initializing...");

            Field.Instance.ResetField(4);
            HandManager.Instance.ResetHand(3);
            // 6각형 타일 맵 초기화
            // 점수 초기화
            // 목표 점수 설정
            // 턴 초기화
            // 제약 적용
            using var initStageArgs = StageStartEventArgs.Get();
            initStageArgs.StageTargetScore = _currentStage.StageTargetScore;
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
            
            
        }
        
        private async UniTask FailStageAsync(CancellationToken cancellationToken)
        {
            
            LogEx.Log("Stage Failing...");
            /*
             * 스테이지 실패 처리
             */
            using var failStageArgs = StageFailEventArgs.Get();
            await ExecEventBus<StageFailEventArgs>.InvokeMerged(failStageArgs);
            
            await UniTask.Delay(1000);
            // 실패 팝업
            // 스테이지 시작으로 돌아가기
            
            LogEx.Log("Stage Failed.");
            GameManager.Instance.ResetGameAndReturnToMainMenu();
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

        /// <summary>
        /// 스테이지를 초기 상태로 재설정합니다.
        /// </summary>
        public void ResetStage()
        {
            
        }
    }
}