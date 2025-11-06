using Core;
using Machamy.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UI.OtherUIs
{
    public class PauseUI : UIBase
    {
        [SerializeField] private Image pauseBackgroundPanel;

        [Header("Popup Elements")]
        [SerializeField] private GameObject pausePopup;
        [SerializeField] private Button soundButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button homeButton;

        protected override void Reset()
        {
            base.Reset();
            pauseBackgroundPanel = transform.GetChild(0).GetComponent<Image>();
            pausePopup = transform.Find("PausePopup").gameObject;
            var buttons = pausePopup.GetComponentsInChildren<Button>();
            void BindButton(ref Button buttonField, int index)
            {
                if (index >= 0 && index < buttons.Length)
                {
                    buttonField = buttons[index];
                }
                else
                {
                    LogEx.LogError("버튼의 인덱스가 범위를 벗어났습니다: " + index);
                }
            }
            BindButton(ref soundButton, 0);
            BindButton(ref retryButton, 1);
            BindButton(ref resumeButton, 2);
            BindButton(ref homeButton, 3);
            
        }

        private void OnEnable()
        {
            resumeButton.onClick.AddListener(OnResumeButtonClicked);
            homeButton.onClick.AddListener(OnHomeButtonClicked);
        }
        
        private void OnDisable()
        {
            resumeButton.onClick.RemoveListener(OnResumeButtonClicked);
            homeButton.onClick.RemoveListener(OnHomeButtonClicked);
        }
        
        public override void Show()
        {
            gameObject.SetActive(true);
        }
        
        public override void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public void OnSoundButtonClicked()
        {
            LogEx.Log("사운드 버튼 클릭됨");
        }
        public void OnRetryButtonClicked()
        {
            Hide();
            LogEx.Log("재시작 버튼 클릭됨");
        }

        public void OnResumeButtonClicked()
        {
            LogEx.Log("재개 버튼 클릭됨");
            GameManager.Instance.ResumeGame();
        }
        
        public void OnHomeButtonClicked()
        {
            LogEx.Log("홈 버튼 클릭됨");
            GameManager.Instance.ResetGameAndReturnToMainMenu();
        }
    }
}