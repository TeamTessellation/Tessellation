using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Interaction;
using Machamy.Utils;
using Stage;
using UI;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 글로벌 게임 상태를 나타내는 열거형입니다.
    /// </summary>
    public enum GlobalGameState
    {
        Initializing, // 게임 초기화 중
        MainMenu, // 게임 메인 메뉴
        InGame, // 게임 플레이 중
        PausedInGame, // 게임 플레이 중 일시정지
    }
    
    public class GameManager : Singleton<GameManager>
    {
        public override bool IsDontDestroyOnLoad => true;

        private CancellationTokenSource _gameCancellationTokenSource = new CancellationTokenSource();
        private CancellationToken _gameCancellationToken;
        [SerializeField]private GlobalGameState _currentGameState = GlobalGameState.Initializing;

        public GlobalGameState CurrentGameState
        {
            get => _currentGameState;
            set
            {
                if (value != _currentGameState)
                {
                    _currentGameState = value;
                    OnGameStateChanged?.Invoke(_currentGameState);
                }
            }
        }

        StageManager StageManager => StageManager.Instance;
        TurnManager TurnManager => TurnManager.Instance;
        UIManager UIManager => UIManager.Instance;
        InteractionManager InteractionManager => InteractionManager.Instance;
        
        public event Action<GlobalGameState> OnGameStateChanged;

        
        public async UniTaskVoid Start()
        {
            LogEx.Log("GameManager 시작");
            if (InitialLoader.Initialized)
            {
                CurrentGameState = GlobalGameState.MainMenu;
            }
            else
            {
                CurrentGameState = GlobalGameState.Initializing;
                await InitialLoader.WaitUntilInitialized();
                CurrentGameState = GlobalGameState.MainMenu;
            }
            Initialize();
        }
        
        private void Initialize()
        {
            InteractionManager.CancelEvent += OnInputCancel;
        }
        
        private void OnDestroy()
        {
            if (InteractionManager != null)
            {
                InteractionManager.CancelEvent -= OnInputCancel;
            }
        }
        
        
        /// <summary>
        /// 게임을 일시정지합니다.
        /// </summary>
        public void PauseGame()
        {
            if (CurrentGameState != GlobalGameState.InGame)
            {
                LogEx.LogWarning("게임이 진행 중이 아닙니다. 일시정지할 수 없습니다.");
                return;
            }
            LogEx.Log("게임 일시정지");
            Time.timeScale = 0f;
            CurrentGameState = GlobalGameState.PausedInGame;
        }

        /// <summary>
        /// 게임을 일시정지하고 정지 UI를 표시합니다.
        /// </summary>
        public void PauseGameWithUI()
        {
            PauseGame();
            UIManager.ShowPauseUI();
        }

        public void ResumeGame()
        {
            if (CurrentGameState != GlobalGameState.PausedInGame)
            {
                LogEx.LogWarning("게임이 일시정지 상태가 아닙니다. 재개할 수 없습니다.");
                return;
            }
            LogEx.Log("게임 재개");
            Time.timeScale = 1f;
            CurrentGameState = GlobalGameState.InGame;
            UIManager.HidePauseUI();
        }
        
        public void QuitGame()
        {
            LogEx.Log("게임 종료");
            Application.Quit();
        }
        
        public void StartStage()
        {
            _gameCancellationTokenSource = new CancellationTokenSource();
            _gameCancellationToken = _gameCancellationTokenSource.Token;
            
            CurrentGameState = GlobalGameState.InGame;
            StageManager.StartStage(_gameCancellationToken);
            UIManager.SwitchMainToGameUI().Forget();
        }
        
        /// <summary>
        /// 게임을 중지하고 메인 메뉴로 복귀합니다.
        /// </summary>
        public void ResetGameAndReturnToMainMenu()
        {
            LogEx.Log("게임 중지 및 메인 메뉴로 복귀");
            _gameCancellationTokenSource.Cancel();
            if (CurrentGameState == GlobalGameState.PausedInGame)
            {
                Time.timeScale = 1f; // 일시정지 상태라면 시간 흐름을 복원
            }
            CurrentGameState = GlobalGameState.MainMenu;
            StageManager.ResetStage();
            UIManager.HidePauseUI();
            UIManager.SwitchToMainMenu().Forget();
        }
        
        
        public void OnInputCancel()
        {
            LogEx.Log("입력 취소 이벤트 수신");
            if (CurrentGameState == GlobalGameState.InGame)
            {
                PauseGameWithUI();
            }
            else if (CurrentGameState == GlobalGameState.PausedInGame)
            {
                UIManager.HidePauseUI();
                ResumeGame();
            }else if (CurrentGameState == GlobalGameState.MainMenu)
            {
                
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                // 앱으로 돌아왔을때는 따로 처리하지 않는다
            }
            else
            {
                if (CurrentGameState == GlobalGameState.InGame)
                {
                    // 게임 플레이 중에 앱을 벗어났을때는 정지UI를 보여준다
                    PauseGameWithUI();
                }else if (CurrentGameState == GlobalGameState.PausedInGame)
                {
                    // 일시정지 상태에서 앱을 벗어났을때는 그대로
                }else if (CurrentGameState == GlobalGameState.MainMenu)
                {
                    // 메인메뉴에서 앱을 벗어났을때는 그대로
                }
            }
        }

    }
}