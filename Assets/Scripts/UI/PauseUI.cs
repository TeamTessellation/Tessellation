using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    
    public class PauseUI : UIBase
    {
        [SerializeField] private Image pauseBackgroundPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button homeButton;

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
        
        public void Show()
        {
            pauseBackgroundPanel.gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            pauseBackgroundPanel.gameObject.SetActive(false);
        }

        public void OnResumeButtonClicked()
        {
            
        }
        
        public void OnHomeButtonClicked()
        {
            
        }
    }
}