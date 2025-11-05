using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using ExecEvents;
using Machamy.Utils;
using UI;
using UnityEngine;

namespace Stage
{
    public class StageManager : Singleton<StageManager>
    {
        public override bool IsDontDestroyOnLoad => false;
        
        private TurnManager TurnManager => TurnManager.Instance;
        
        
        
        private StageModel _currentStage;

        public StageModel CurrentStage
        {
            get => _currentStage;
            set => _currentStage = value;
        }
        
        [SerializeField] private bool _isStageCleared = false;
        
        public bool IsStageCleared => _isStageCleared;
        
        private CancellationToken token;
        
        /// <summary>
        /// 스테이지를 시작합니다.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void StartStage(CancellationToken cancellationToken)
        {
            token = cancellationToken;
            StartStageAsync().Forget();
        }
        
        /// <summary>
        /// 스테이지를 정상적으로 종료합니다.
        /// </summary>
        public void EndStage()
        {
            EndStageAsync().Forget();
        }

        public StageModel GetNextStage()
        {
            StageModel tmpStage = new StageModel()
            {
                StageLevel = CurrentStage == null ? 1 : CurrentStage.StageLevel + 1,
                StageTargetScore = CurrentStage == null ? 1000 : CurrentStage.StageTargetScore + Random.Range(500, 2500),
            };
            return tmpStage;
        }
        
        private async UniTask StartStageAsync()
        {
            if (token == default)
            {
                LogEx.LogError("StageManager: CancellationToken is not set. Cannot start stage.");
                return;
            }
            var UM = UIManager.Instance;
            _isStageCleared = false;
            LogEx.Log("Stage Starting...");
            _currentStage = GetNextStage();
             /*
              * 스테이지 시작 화면 표시
              */
             
             //TODO 임시 스테이지

             UM.SwitchMainToGameUI();
             await UM.StageInfoUI.ShowInfoRoutine(CurrentStage);
             
             /*
              * 스테이지 초기화
              */
             LogEx.Log("Stage Initializing...");
             
             // 6각형 타일 맵 초기화
             // 점수 초기화
             // 목표 점수 설정
             // 턴 초기화
             // 제약 적용
             using var initStageArgs = StageStartEventArgs.Get();
             await ExecEventBus<StageStartEventArgs>.InvokeMerged(initStageArgs);
             
             await UniTask.Delay(1000, cancellationToken: token);
             
             LogEx.Log("Stage Initialized.");
             
            /*
            * 스테이지 파트 시작
            */
            TurnManager.StartTurnLoop();
        }
        
        private async UniTask EndStageAsync()
        {
            if (token == CancellationToken.None)
            {
                LogEx.LogError("StageManager: CancellationToken is not set. Cannot end stage.");
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
        
        public bool CheckStageClear()
        {
            // 목표 점수 도달 확인
            // _isStageCleared = true;
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