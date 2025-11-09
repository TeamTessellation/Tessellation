using System;
using Core;
using Cysharp.Threading.Tasks;
using Machamy.Utils;
using UI.MainUIs;
using UI.OtherUIs;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class UIManager : Singleton<UIManager>
    {
        public override bool IsDontDestroyOnLoad => true;
        
        private Canvas _globalCanvas;
        public Canvas GlobalCanvas
        {
            get
            {
                if (_globalCanvas == null)
                {
                    _globalCanvas = FindAnyObjectByType<GlobalCanvas>()?.GetComponent<Canvas>();
                    if (_globalCanvas == null)
                    {
                        var canvasGO = new GameObject("#GlobalCanvas");
                        _globalCanvas = canvasGO.AddComponent<Canvas>();
                        canvasGO.AddComponent<GlobalCanvas>();
                        canvasGO.AddComponent<CanvasScaler>();
                        canvasGO.AddComponent<GraphicRaycaster>();
                        _globalCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    }
                }
                return _globalCanvas;
            }
        }
        

        [Header("Main UIs")]
        [field:SerializeField] public MainTitleUI MainTitleUI{ get; private set; }
        [field:SerializeField] public GameUI GameUI{ get; private set; }
        [field:SerializeField] public InGameUI InGameUI{ get; private set; }
        [field: FormerlySerializedAs("<SettingUI>k__BackingField")] [field:SerializeField] public SettingParentUI SettingParentUI{ get; private set; }
        [Header("GameUI Child UIs")]
        [field:SerializeField] public StageInfoUI StageInfoUI{ get; private set; }
        [Header("Setting Child UIs")]
        [field:SerializeField] public PauseUI PauseUI { get; private set; }
        [field:SerializeField] public SoundSettingUI SoundSettingUI { get; private set; }  
        [field:Space(20)]
        [Header("Other UIs")]
        [field:SerializeField] public TransitionUI TransitionUI{ get; private set; }

        //[field: SerializeField] public UI TransitionUI { get; private set; }

        // ReSharper disable once Unity.IncorrectMethodSignature
        private async UniTaskVoid Start()
        {
            await InitialLoader.WaitUntilInitialized();
        }

        private void OnEnable()
        {
            if (GameManager.Instance.CurrentGameState == GlobalGameState.Initializing)
            {
                void OnInitialized(GlobalGameState newState)
                {
                    if (newState != GlobalGameState.Initializing)
                    {
                        GameManager.Instance.OnGameStateChanged -= OnInitialized;
                        RegisterUIs();
                        BindEventsToUIs();
                        SetMainMenu();
                    }
                }
                GameManager.Instance.OnGameStateChanged += OnInitialized;
            }
            else
            {
                RegisterUIs();
                BindEventsToUIs();
                SetMainMenu();
            }
        }
        
        private void OnDisable()
        {
            UnbindEventsFromUIs();
        }

        private void BindEventsToUIs()
        {
            InGameUI.RegisterEvents();
        }
        private void UnbindEventsFromUIs()
        {
            if (InGameUI == null)
            {
                InGameUI = GlobalCanvas.GetComponentInChildren<InGameUI>(true);
            }
            InGameUI.UnregisterEvents();
        }

        private void RegisterUIs()
        {
            T FindUI<T>() where T : Component
            {
                var ui = GlobalCanvas.GetComponentInChildren<T>(true);
                if (ui == null)
                {
                    LogEx.LogError($"UIManager: {typeof(T).Name} not found in the scene.");
                }
                return ui;
            }
            
            SettingParentUI = FindUI<SettingParentUI>();
            PauseUI = FindUI<PauseUI>();
            MainTitleUI = FindUI<MainTitleUI>();
            GameUI = FindUI<GameUI>();
            InGameUI = FindUI<InGameUI>();
            SoundSettingUI = FindUI<SoundSettingUI>();
            StageInfoUI = FindUI<StageInfoUI>();
            TransitionUI = FindUI<TransitionUI>();
        }

        public void ShowPauseUI()
        {
            SettingParentUI.Show();
            PauseUI.Show();
        }
        
        public void HidePauseUI()
        {
            PauseUI.Hide();
        }

        /// <summary>
        /// 애니메이션 없이 메인 메뉴 UI로 전환합니다.
        /// </summary>
        public void SetMainMenu()
        {
            LogEx.Log("SetMainMenu called.");
            GameUI.Hide();
            MainTitleUI.Show();
            PauseUI.Hide();
        }
        
        /// <summary>
        /// 메인 메뉴 UI로 즉시 전환합니다.
        /// </summary>
        /// <remarks>
        /// 시작/종료 애니메이션 없이 즉시 전환됩니다.
        /// </remarks>
        public async void SwitchToMainMenu()
        {
            // TODO : UniTask이용해서 애니메이션 처리 가능
            GameUI.Hide();
            MainTitleUI.Show();
        }
        
        /// <summary>
        /// MainUI에서 게임 UI로 전환합니다.
        /// </summary>
        /// <remarks>
        /// 시작/종료 애니메이션 없이 즉시 전환됩니다.
        /// </remarks>
        public async void SwitchMainToGameUI()
        {
            // TODO : UniTask이용해서 애니메이션 처리 가능
            MainTitleUI.Hide();
            GameUI.Show(); 
            InGameUI.Show();
        }
        
        
    }
}