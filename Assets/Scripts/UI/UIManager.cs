using System;
using Core;
using Cysharp.Threading.Tasks;
using Interaction;
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
                }
                return _globalCanvas;
            }
        }
        
        private GameObject _worldScene;

        public GameObject WorldScene
        {
            get
            {
                if (_worldScene == null)
                {
                    _worldScene = GameObject.Find("#WorldScene");
                }
                return _worldScene;
            }
        }
        

        [Header("Main UIs")]
        [field:SerializeField] public MainTitleUI MainTitleUI{ get; private set; }
        [field:SerializeField] public GameUI GameUI{ get; private set; }
        [field:SerializeField] public InGameUI InGameUI{ get; private set; }
        [field: FormerlySerializedAs("<SettingUI>k__BackingField")] [field:SerializeField] public SettingParentUI SettingParentUI{ get; private set; }
        [Header("GameUI Child UIs")]
        [field:SerializeField] public StageInfoUI StageInfoUI{ get; private set; }
        [field:SerializeField] public FailResultUI FailResultUI{ get; private set; }
        [field:SerializeField] public ClearResultUI ClearResultUI{ get; private set; }
        [Header("Setting Child UIs")]
        [field:SerializeField] public PauseUI PauseUI { get; private set; }
        [field:SerializeField] public SoundSettingUI SoundSettingUI { get; private set; }  
        [field:Space(20)]
        [Header("World UIs")]
        [field:SerializeField] public HandCanvas HandCanvas{ get; private set; }
        [Header("Other UIs")]
        [field:SerializeField] public TransitionUI TransitionUI{ get; private set; }

        //[field: SerializeField] public UI TransitionUI { get; private set; }

        // ReSharper disable once Unity.IncorrectMethodSignature
        private async UniTaskVoid Start()
        {
            cnt++;
            LogEx.Log($"UIUIStart {GetInstanceID()}, {GetEntityId()}, {name}, Scene: {gameObject.scene.name}");
            if (cnt > 1)
            {
                LogEx.LogWarning($"UIManager Start called multiple times! Count: {cnt}");
                Debug.Assert(false, "UIManager Start called multiple times!");
            }
            await GameManager.WaitForInit();
            Init();
            InteractionManager.Instance.CancelEvent += OnCancelInput;
        }
        
        private void OnDestroy()
        {
            UnbindEventsFromUIs();
        }

        private static int cnt = 0;
        void Init()
        {
            
            RegisterUIs();
            BindEventsToUIs();
            SetMainMenu();
        }
        
        private void BindEventsToUIs()
        {
            InGameUI.RegisterEvents();
        }
        private void UnbindEventsFromUIs()
        {
            if (InGameUI == null)
            {
                InGameUI = GlobalCanvas?.GetComponentInChildren<InGameUI>(true);
            }
            if (InGameUI != null)
            {
                InGameUI.UnregisterEvents();
            }
        }

        private void RegisterUIs()
        {
            LogEx.Log("Registering UIs...");
            T FindUI<T>() where T : Component
            {
                var ui = GlobalCanvas.GetComponentInChildren<T>(true);
                if (ui == null)
                {
                    LogEx.LogError($"UIManager: {typeof(T).Name} not found in the scene.");
                }
                return ui;
            }
            
            T FindWorldUI<T>() where T : Component
            {
                var ui = WorldScene.GetComponentInChildren<T>(true);
                if (ui == null)
                {
                    LogEx.LogError($"UIManager: {typeof(T).Name} not found in the WorldScene.");
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
            // TransitionUI = FindUI<TransitionUI>();
            FailResultUI = FindUI<FailResultUI>();
            ClearResultUI = FindUI<ClearResultUI>();
            
            HandCanvas = FindWorldUI<HandCanvas>();
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
            InGameUI.Hide();
            FailResultUI.Hide();
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
            FailResultUI.Hide();
        }


        public void OnCancelInput()
        {
            if (SoundSettingUI.isActiveAndEnabled)
            {
                SoundSettingUI.OnClickBackButton();
            }
            else if (PauseUI.isActiveAndEnabled)
            {
                HidePauseUI();
                GameManager.Instance.ResumeGame();
            }
            else
            {
                if (GameManager.Instance.CurrentGameState == GlobalGameState.InGame)
                {
                    GameManager.Instance.PauseGameWithUI();
                }
                else
                {
                    SoundSettingUI.ShowDefaultAsync().Forget();
                }
            }
        }
        
    }
}