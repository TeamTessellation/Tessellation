using Core;
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
        
        [SerializeField] private MainTitleUI _mainTitleUI;
        [SerializeField] private GameUI _gameUI;
        
        
        public void ShowPauseUI()
        {
            // PauseUI.Show();
        }
        
        public void SwitchToMainMenu()
        {
            // TODO : Unitask이용해서 애니메이션 처리 가능
            _gameUI.gameObject.SetActive(false);
            _mainTitleUI.gameObject.SetActive(true);
        }
        
        public void SwitchToGameUI()
        {
            // TODO : Unitask이용해서 애니메이션 처리 가능
            _mainTitleUI.gameObject.SetActive(false);
            _gameUI.gameObject.SetActive(true);
        }
        
        
    }
}