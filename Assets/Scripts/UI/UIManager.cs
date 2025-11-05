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
        [field:SerializeField] private MainTitleUI MainTitleUI{ get; private set; }
        [field:SerializeField] private GameUI GameUI{ get; private set; }
        
        
        public void ShowPauseUI()
        {
            PauseUI.gameObject.SetActive(true);
        }
        
        public void SwitchToMainMenu()
        {
            // TODO : Unitask이용해서 애니메이션 처리 가능
            GameUI.gameObject.SetActive(false);
            MainTitleUI.gameObject.SetActive(true);
        }
        
        public void SwitchToGameUI()
        {
            // TODO : Unitask이용해서 애니메이션 처리 가능
            MainTitleUI.gameObject.SetActive(false);
            GameUI.gameObject.SetActive(true);
        }
        
        
    }
}