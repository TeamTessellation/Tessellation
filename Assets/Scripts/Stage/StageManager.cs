using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using ExecEvents;
using Machamy.Utils;
using UnityEngine;

namespace Stage
{
    public class StageManager : Singleton<StageManager>
    {
        public override bool IsDontDestroyOnLoad => false;
        
        private TurnManager TurnManager => TurnManager.Instance;
        
        
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
        /// 스테이지를 종료합니다.
        /// </summary>
        public void EndStage()
        {
            EndStageAsync().Forget();
        }
        
        private async UniTask StartStageAsync()
        {
            if (token == default)
            {
                LogEx.LogError("StageManager: CancellationToken is not set. Cannot start stage.");
                return;
            }
            _isStageCleared = false;
            LogEx.Log("Stage Starting...");
             /*
              * 스테이지 시작 화면 표시
              */
             
             
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
        
        
        
        
        
        
        
        
        
        
        
        
        
        
    }
}