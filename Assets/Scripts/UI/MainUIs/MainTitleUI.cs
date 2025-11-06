using System;
using System.Collections.Generic;
using Core;
using Machamy.Utils;
using SaveLoad;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainUIs
{
    public class MainTitleUI : UIBase
    {
        [Header("Main Title Buttons")]
        [SerializeField] private Transform mainTitleButtonsParent;
        [SerializeField] private Button startButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        
        protected override void Reset()
        {
            base.Reset();
            if (mainTitleButtonsParent == null)
            {
                mainTitleButtonsParent = transform.Find("MainTitleButtons");
                if (mainTitleButtonsParent == null)
                {
                    var go = new GameObject("MainTitleButtons");
                    go.transform.SetParent(transform);
                    mainTitleButtonsParent = go.transform;
                }
                var buttons = mainTitleButtonsParent.GetComponentsInChildren<Button>();

                
                void BindButton(ref Button variable, string name)
                {
                    foreach (var button in buttons)
                    {
                        if (button.name.Contains(name, StringComparison.OrdinalIgnoreCase))
                        {
                            variable = button;
                            break;
                        }
                    }
                }
                BindButton(ref startButton, "Start");
                BindButton(ref continueButton, "Continue");
                BindButton(ref settingsButton, "Settings");
                BindButton(ref exitButton, "Exit");
            }

        }
        public override void Show()
        {
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            exitButton.onClick.AddListener(OnExitButtonClicked);
            
            UpdateContinueButtonInteractable();
        }
        
        private void OnDisable()
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
            continueButton.onClick.RemoveListener(OnContinueButtonClicked);
            settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }

        public void SetContinueButtonInteractable(bool interactable)
        {
            if (continueButton != null)
            {
                continueButton.interactable = interactable;
            }
        }

        public void UpdateContinueButtonInteractable()
        {
            bool hasSavedGame = false;
            
            SaveLoadManager saveLoadManager = SaveLoadManager.Instance;
            if (saveLoadManager != null)
            {
                hasSavedGame = saveLoadManager.HasSimpleSave();
            }
            SetContinueButtonInteractable(hasSavedGame);
        }

        public void OnStartButtonClicked()
        {
            LogEx.Log("Start Button Clicked");
            GameManager.Instance.StartStage();
        }
        
        public void OnContinueButtonClicked()
        {
            LogEx.Log("Continue Button Clicked");
            //TODO : History 기반으로 저장 로드 해야함. 현재 SaveLoadManager에서 가장 최근 저장 불러오는 것으로 임시 구현
            
        }
        public void OnSettingsButtonClicked()
        {
            LogEx.Log("Settings Button Clicked");
            // TODO : 설정 UI 열기
            
        }
        public void OnExitButtonClicked()
        {
            LogEx.Log("Exit Button Clicked");
            Application.Quit();
        }



    }
}