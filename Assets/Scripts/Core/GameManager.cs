using System;
using System.Threading;
using Stage;
using UnityEngine;

namespace Core
{
    public class GameManager : Singleton<GameManager>
    {
        public override bool IsDontDestroyOnLoad => true;

        private CancellationTokenSource _gameCancellationTokenSource = new CancellationTokenSource();
        private CancellationToken _gameCancellationToken;
        
        StageManager StageManager => StageManager.Instance;
        TurnManager TurnManager => TurnManager.Instance;
        
    

        public void PauseGame()
        {
            Time.timeScale = 0f;
        }

        public void PauseGameWithUI()
        {
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;
        }
        
        public void QuitGame()
        {
            Application.Quit();
        }
        
        public void StartStage()
        {
            _gameCancellationTokenSource = new CancellationTokenSource();
            _gameCancellationToken = _gameCancellationTokenSource.Token;
            
            StageManager.StartStage(_gameCancellationToken);
        }
        
        /// <summary>
        /// 게임을 정지하고 메인 메뉴로 돌아갑니다.
        /// </summary>
        public void StopGameAndReturnToMainMenu()
        {
            _gameCancellationTokenSource.Cancel();
            ResumeGame();
            
            // TODO : 메인 메뉴로 돌아가기
            
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                // 앱으로 돌아왔을때
            }
            else
            {
                PauseGameWithUI();
            }
        }

    }
}