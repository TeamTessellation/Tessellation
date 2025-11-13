using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ExecEvents;
using Interaction;
using Machamy.Utils;
using Player;
using SaveLoad;
using Stage;
using UI;
using Unity.VisualScripting;
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
    

    public class GameManager : Singleton<GameManager>, ISaveTarget
    {
        public override bool IsDontDestroyOnLoad => true;

        /// <summary>
        /// 게임 전체를 취소할 수 있는 토큰 소스입니다.
        /// 게임 전체로직과 상관 없는 로직에서는 이 토큰을 사용하지 마세요.
        /// </summary>
        private CancellationTokenSource _gameCancellationTokenSource = new CancellationTokenSource();
        [SerializeField]private GlobalGameState _currentGameState = GlobalGameState.Initializing;

        [field:SerializeField] public bool DisableContinueInMainMenu { get; private set; } = false;
        public CancellationToken GameCancellationToken => _gameCancellationTokenSource.Token;
        [SerializeField] private PlayerStatus _playerStatus = new PlayerStatus();
        public GlobalGameState CurrentGameState
        {
            get => _currentGameState;
            set
            {
                if (value != _currentGameState)
                {
                    LogEx.Log($"게임 상태 변경: {_currentGameState} -> {value}");
                    _currentGameState = value;
                    OnGameStateChanged?.Invoke(_currentGameState);
                }
            }
        }

        StageManager StageManager => StageManager.Instance;
        TurnManager TurnManager => TurnManager.Instance;
        UIManager UIManager => UIManager.Instance;
        InteractionManager InteractionManager => InteractionManager.Instance;
        
        
        public PlayerStatus PlayerStatus => _playerStatus;
        
        public event Action<GlobalGameState> OnGameStateChanged;
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        
        
        public bool AutoPauseInBackground = false;

        protected override void AfterAwake()
        {
            base.AfterAwake();
            
            OnGameStateChanged += (newState) =>
            {
                switch (newState)
                {
                    case GlobalGameState.InGame:
                        OnGameResumed?.Invoke();
                        break;
                    case GlobalGameState.PausedInGame:
                        OnGamePaused?.Invoke();
                        break;
                }
            };
        }
        public async UniTaskVoid Start()
        {
            LogEx.Log("GameManager 시작");
            CurrentGameState = GlobalGameState.Initializing;
            await UniTask.Yield();
            if (InitialLoader.Initialized)
            {
                await UniTask.Yield();
                CurrentGameState = GlobalGameState.MainMenu;
            }
            else
            {
                CurrentGameState = GlobalGameState.Initializing;
                await InitialLoader.WaitUntilInitialized();
                await UniTask.Yield();
                CurrentGameState = GlobalGameState.MainMenu;
            }
            Initialize();
        }
        
        private void Initialize()
        {
            InteractionManager.CancelEvent += OnInputCancel;
            
            SaveLoadManager.RegisterPendingSavable(this);
        }
        
        private void OnDestroy()
        {
            if (InteractionManager.HasInstance)
            {
                InteractionManager.CancelEvent -= OnInputCancel;
            }
        }
        /// <summary>
        /// 게임을 시작합니다.
        /// 스테이지는 1-1부터 시작됩니다.
        /// </summary>
        public void StartGame()
        {
            ResetGame();
            _gameCancellationTokenSource = new CancellationTokenSource();
            CurrentGameState = GlobalGameState.InGame;
            StageManager.CurrentStage = StageModel.FirstStageModel;
            StageManager.StartStage(_gameCancellationTokenSource.Token);
        }
        
        public void ContinueStage()
        {
            LogEx.Log("스테이지 계속하기");
            ResetGame();
            // 이전 토큰 디스포즈
            
            _gameCancellationTokenSource = new CancellationTokenSource();
            SaveLoadManager svM = SaveLoadManager.Instance;
            
            if(svM == null || !svM.HasSimpleSave())
            {
                LogEx.LogWarning("No saved game to continue.");
                return;
            }
            GameData data = svM.GetSimpleSaveData();
            if(data == null || data.SaveHistory.Count == 0)
            {
                LogEx.LogWarning("No stage data in saved game.");
                return;
            }
            svM.LoadSaveData(data);
            StageManager sm = StageManager.Instance;
            sm.StartStage(_gameCancellationTokenSource.Token);
        }

        public void ContinueTurn()
        {
            LogEx.Log("턴 계속하기");
            ResetGame();
            // 이전 토큰 디스포즈
            
            _gameCancellationTokenSource = new CancellationTokenSource();
            SaveLoadManager svM = SaveLoadManager.Instance;
            
            if(svM == null || !svM.HasSimpleSave())
            {
                LogEx.LogWarning("No saved game to continue.");
                return;
            }
            svM.SimpleLoad(onComplete: () =>
            {
                StageManager sm = StageManager.Instance;
                if (sm.CurrentStage != null)
                {
                    CurrentGameState = GlobalGameState.InGame;
                    sm.StartStage(_gameCancellationTokenSource.Token);
                }
                else
                {
                    LogEx.LogError("Failed to continue game: Current stage is null after loading save.");
                }
            });
        }
        
        public void RestartCurrentStage()
        {
            LogEx.Log("현재 스테이지 재시작");
            _gameCancellationTokenSource.Cancel();
            if (CurrentGameState == GlobalGameState.PausedInGame)
            {
                Time.timeScale = 1f; // 일시정지 상태라면 시간 흐름을 복원
            }
            /*
             * 1. 상태 인게임으로 변경
             * 2. 스테이지 매니저 현재 스테이지 재시작
             */
            CurrentGameState = GlobalGameState.InGame;
            _gameCancellationTokenSource = new CancellationTokenSource();
            StageManager.RestartCurrentStage(_gameCancellationTokenSource.Token);
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
            /*
             * 1. 상태 메인메뉴로 변경
             * 2. 플레이어 상태 초기화
             * 3. 스테이지 매니저 초기화(플레이어에 종속적)
             * 3-n. 스테이지 매니저가 스테이지 초기화시 필요한 로직 수행
             * 4. 일시정지 UI 숨기기
             * 5. 메인메뉴 UI로 전환
             */
            
            CurrentGameState = GlobalGameState.MainMenu;
            PlayerStatus.Reset();
            StageManager.ResetStage();
            UIManager.HidePauseUI();
            UIManager.SwitchToMainMenu();
            
            _gameCancellationTokenSource = new CancellationTokenSource();
        }
        
        
        /// <summary>
        /// 게임을 중지하고 초기화합니다.
        /// </summary>
        public void ResetGame()
        {
            // 일단 내용 복사 해둠
            LogEx.Log("게임 중지 및 초기화");
            _gameCancellationTokenSource.Cancel();
            if (CurrentGameState == GlobalGameState.PausedInGame)
            {
                Time.timeScale = 1f; // 일시정지 상태라면 시간 흐름을 복원
            }
            /*
             * 1. 상태 메인메뉴로 변경
             * 2. 플레이어 상태 초기화
             * 3. 스테이지 매니저 초기화(플레이어에 종속적)
             * 3-n. 스테이지 매니저가 스테이지 초기화시 필요한 로직 수행
             * 4. 일시정지 UI 숨기기
             * 5. 메인메뉴 UI로 전환
             */
            CurrentGameState = GlobalGameState.MainMenu;
            PlayerStatus.Reset();
            StageManager.ResetStage();
            UIManager.HidePauseUI();
            UIManager.SwitchToMainMenu();
            
            _gameCancellationTokenSource = new CancellationTokenSource();
        }
        
        public void OnInputCancel()
        {
            LogEx.Log("입력 취소 이벤트 수신");
            if (CurrentGameState == GlobalGameState.InGame)
            {
            }
            else if (CurrentGameState == GlobalGameState.PausedInGame)
            {
                
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
                    if (AutoPauseInBackground)
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
        
        public Guid Guid { get; init; } = Guid.NewGuid();
        public void LoadData(GameData data)
        {
            _playerStatus.LoadData(data);
        }

        public void SaveData(ref GameData data)
        {
            _playerStatus.SaveData(ref data);
        }



        public static async UniTask WaitForInit()
        {
            await InitialLoader.WaitUntilInitialized();
            await UniTask.Yield();
            await UniTask.WaitUntil(() => GameManager.HasInstance);
            await UniTask.WaitUntil(() => GameManager.Instance.CurrentGameState != GlobalGameState.Initializing);
        }
    }
}