using System;
using Core;
using Cysharp.Threading.Tasks;
using Machamy.Utils;
using UI.MainUIs;
using UnityEngine;
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
        
        [field:SerializeField] public PauseUI PauseUI { get; private set; }
        [field:SerializeField] public MainTitleUI MainTitleUI{ get; private set; }
        [field:SerializeField] public GameUI GameUI{ get; private set; }

        // ReSharper disable once Unity.IncorrectMethodSignature
        private async UniTaskVoid Start()
        {
            await InitialLoader.WaitUntilInitialized();
            RegisterUIs();
        }

        
        
        private void RegisterUIs()
        {
            T FindUI<T>() where T : Component
            {
                var ui = FindAnyObjectByType<T>();
                if (ui == null)
                {
                    LogEx.LogError($"UIManager: {typeof(T).Name} not found in the scene.");
                }
                return ui;
            }
            
            PauseUI = FindUI<PauseUI>();
            MainTitleUI = FindUI<MainTitleUI>();
            GameUI = FindUI<GameUI>();
        }

        public void ShowPauseUI()
        {
            PauseUI.Show();
        }
        
        public void HidePauseUI()
        {
            PauseUI.Hide();
        }
        
        /// <summary>
        /// 메인 메뉴 UI로 전환합니다.
        /// </summary>
        public void SwitchToMainMenu()
        {
            // TODO : UniTask이용해서 애니메이션 처리 가능
            GameUI.gameObject.SetActive(false);
            MainTitleUI.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// MainUI에서 게임 UI로 전환합니다.
        /// </summary>
        public void SwitchMainToGameUI()
        {
            // TODO : UniTask이용해서 애니메이션 처리 가능
            MainTitleUI.gameObject.SetActive(false);
            GameUI.gameObject.SetActive(true);
        }
        
        
    }
}