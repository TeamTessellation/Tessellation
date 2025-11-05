using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using ExecEvents;
using Machamy.DeveloperConsole.Attributes;
using Machamy.Utils;
using Player;
using UnityEngine;

namespace Stage
{
    public enum TurnState
    {
        Field,
        Etc,
        Player,
        Item,
    }

    /// <summary>
    /// 턴의 기본적인 시작과 종료를 처리하는 인터페이스입니다.
    /// </summary>
    public interface IBasicTurnLogic
    {
        UniTask OnTurnStart(int turnCount, CancellationToken token);
        UniTask OnTurnEnd(int turnCount, CancellationToken token);
    }
    
    /// <summary>
    /// 필드 턴 로직을 처리하는 인터페이스입니다.
    /// </summary>
    public interface IFieldTurnLogic
    {
        public UniTask TileSetDraw(CancellationToken token);
    }
    
    /// <summary>
    /// 플레이어 턴 로직을 처리하는 인터페이스입니다.
    /// </summary>
    public interface IPlayerTurnLogic
    {
        public bool IsPlayerInputEnabled { get; }
        public void SetPlayerInputEnabled(bool enabled);
        public UniTask<PlayerInputData> WaitForPlayerReady(CancellationToken token);

        public bool IsPlayerCanDoAction();
        
    }
    
    /// <summary>
    /// 플레이어 입력 처리를 담당하는 인터페이스입니다.
    /// </summary>
    public interface IPlayerInputHandler
    {
        public UniTask HandlePlayerInput(PlayerInputData inputData, CancellationToken token);
    }
    //
    // public interface IClearChecker
    // {
    //     public bool IsClear();
    // }

    /// <summary>
    /// 게임의 턴을 관리하는 매니저 클래스입니다.
    /// </summary>
    public class TurnManager : Singleton<TurnManager>
    {
        public override bool IsDontDestroyOnLoad => false;
        
        [SerializeField]private int _turnCount = 0;
        public int TurnCount => _turnCount;
        public TurnState State { get; private set; }
        
        [Header("Logics/Handlers")]
        private IFieldTurnLogic fieldTurnLogic;
        private IPlayerTurnLogic playerTurnLogic;
        private IPlayerInputHandler playerInputHandler;
        private IBasicTurnLogic[] basicTurnLogics;

        private StageManager StageManager => StageManager.Instance;

        private CancellationTokenSource _cancellationTokenSource;

        private void Reset()
        {
            FindLogics();
        }

        private void Start()
        {
            FindLogics();
        }

        [ContextMenu("Find Logics/Handlers")]
        public void FindLogics()
        {
            if (fieldTurnLogic == null)
            {
                fieldTurnLogic = GetComponent<IFieldTurnLogic>();
                if (fieldTurnLogic == null)
                {
                    LogEx.LogWarning("FieldTurnLogic not found on TurnManager GameObject.");
                }
            }
            if (playerTurnLogic == null)
            {
                playerTurnLogic = GetComponent<IPlayerTurnLogic>();
                if (playerTurnLogic == null)
                {
                    LogEx.LogWarning("PlayerTurnLogic not found on TurnManager GameObject.");
                }
            }
            if (playerInputHandler == null)
            {
                playerInputHandler = GetComponent<IPlayerInputHandler>();
                if (playerInputHandler == null)
                {
                    LogEx.LogWarning("PlayerInputHandler not found on TurnManager GameObject.");
                }
            }
            if (basicTurnLogics == null || basicTurnLogics.Length == 0)
            {
                basicTurnLogics = GetComponents<IBasicTurnLogic>();
                if (basicTurnLogics == null || basicTurnLogics.Length == 0)
                {
                    LogEx.LogWarning("No BasicTurnLogics found on TurnManager GameObject.");
                }
            }
        }
        

        protected override void AfterAwake()
        {
            _cancellationTokenSource = null;
        }
        
        /// <summary>
        /// 턴 루프를 시작합니다.
        /// </summary>
        public void StartTurnLoop()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                LogEx.LogWarning("Turn loop is already running.");
                return;
            }
            _cancellationTokenSource = new CancellationTokenSource();
            _turnCount = 0;
            LogEx.Log("Turn loop started.");
            var token = _cancellationTokenSource.Token;
            TurnLoop(token).Forget();
        }
        
        /// <summary>
        /// 턴 루프를 중지합니다.
        /// </summary>
        public void StopTurnLoop()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }
        
        /// <summary>
        /// 턴 루프를 진행하는 메서드입니다.
        /// </summary>
        private async UniTask TurnLoop(CancellationToken token)
        {
            LogEx.Log($"Turn {_turnCount} started.");
            
            if (fieldTurnLogic == null)
            {
                LogEx.LogError("FieldTurnLogic is not assigned.");
                return;
            }
            if (playerTurnLogic == null)
            {
                LogEx.LogError("PlayerTurnLogic is not assigned.");
                return;
            }

            while (true)
            {
                
                _turnCount++;
                using var turnStartArgs = TurnStartEventArgs.Get();
                await ExecEventBus<TurnStartEventArgs>.InvokeMerged(turnStartArgs);
                
                if (token.IsCancellationRequested)
                {
                    LogEx.Log("Turn loop cancelled.");
                    break;
                }
                /*
                 * 타일셋 뽑기
                 */
                LogEx.Log("Field phase...");
                State = TurnState.Field;
                await fieldTurnLogic.TileSetDraw(token);
                
                /*
                 * 기타 턴 시작 단계
                 */
                LogEx.Log("etc. phase...");
                State = TurnState.Etc;
                foreach (var basicTurnLogic in basicTurnLogics)
                {
                    if (basicTurnLogic == null) continue;
                    await basicTurnLogic.OnTurnStart(_turnCount, token);
                }
                /*
                 * 유저
                 */
                LogEx.Log("Player phase...");
                State = TurnState.Player;
                while (playerTurnLogic.IsPlayerCanDoAction())
                {
                    using var playerActionLoopStartArgs = PlayerActionLoopStartEventArgs.Get();
                    await ExecEventBus<PlayerActionLoopStartEventArgs>.InvokeMerged(playerActionLoopStartArgs);
                    // 플레이어가 행동할 수 있는 동안 반복
                    
                    if (token.IsCancellationRequested)
                    {
                        LogEx.Log("Turn loop cancelled.");
                        return;
                    }
                    
                    playerTurnLogic.SetPlayerInputEnabled(true);
                    var playerInputData = await playerTurnLogic.WaitForPlayerReady(token);
                    playerTurnLogic.SetPlayerInputEnabled(false);
                    using var playerActionArgs = BeforePlayerActionEventArgs.Get();
                    await ExecEventBus<BeforePlayerActionEventArgs>.InvokeMerged(playerActionArgs);
                    await playerInputHandler.HandlePlayerInput(playerInputData, token);
                    
                    // 클리어 체크
                    if(StageManager.CheckStageClear())
                    {
                        LogEx.Log("Stage Cleared!");
                        StageManager.EndStage();
                        return;
                    }
                    using var playerActionLoopEndArgs = PlayerActionLoopEndEventArgs.Get();
                    await ExecEventBus<PlayerActionLoopEndEventArgs>.InvokeMerged(playerActionLoopEndArgs);
                    
                }
                 

            
                LogEx.Log($"Turn {_turnCount} ended.");
                State = TurnState.Item;
                /*
                 * 턴 마무리 단계
                 * ex) 아이템 같은거?
                 */
                foreach (var basicTurnLogic in basicTurnLogics)
                {
                    if (basicTurnLogic == null) continue;
                    await basicTurnLogic.OnTurnEnd(_turnCount, token);
                }
                using var turnEndArgs = TurnEndEventArgs.Get();
                await ExecEventBus<TurnEndEventArgs>.InvokeMerged(turnEndArgs);
                
                LogEx.Log($"Turn {_turnCount} fully ended.");
                
                
            }
        }

        [ConsoleCommand("StartTurnLoop", "Starts the turn loop.")]
        private static void StartTurnLoopCommand()
        {
            var turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager != null)
            {
                turnManager.StartTurnLoop();
            }
            else
            {
                LogEx.LogWarning("No TurnManager found in the scene.");
            }
        }
        
        [ConsoleCommand("StopTurnLoop", "Stops the turn loop.")]
        private static void StopTurnLoopCommand()
        {
            var turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager != null)
            {
                turnManager.StopTurnLoop();
            }
            else
            {
                LogEx.LogWarning("No TurnManager found in the scene.");
            }
        }
    }
}
