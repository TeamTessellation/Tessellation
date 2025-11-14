using System;
using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;
using Machamy.Utils;
using SaveLoad;
using Sound;
using Stage;
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
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
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
            
            
            
            if (SaveLoadManager.HasInstance)
            {
                SaveLoadManager saveLoadManager = SaveLoadManager.Instance;
                hasSavedGame = saveLoadManager.HasSimpleSave();
                if(GameManager.Instance.DisableContinueInMainMenu)
                {
                    hasSavedGame = false;
                }
                SetContinueButtonInteractable(hasSavedGame);
            }
            else
            {
                async UniTask WaitForSaveLoadManager()
                {
                    await UniTask.WaitUntil(() => SaveLoadManager.HasInstance);
                    hasSavedGame = SaveLoadManager.Instance.HasSimpleSave();
                    if (GameManager.Instance.DisableContinueInMainMenu)
                    {
                        hasSavedGame = false;
                    }
                    SetContinueButtonInteractable(hasSavedGame);
                }
                WaitForSaveLoadManager().Forget();
            }
            

        }

        public void OnStartButtonClicked()
        {
            LogEx.Log("Start Button Clicked");
            GameManager.Instance.StartGame();
            SoundManager.Instance.PlaySfx(SoundReference.UIClick);
        }
        
        public void OnContinueButtonClicked()
        {
            LogEx.Log("Continue Button Clicked");
            GameManager.Instance.ContinueTurn();
            SoundManager.Instance.PlaySfx(SoundReference.UIClick);
        }
        public void OnSettingsButtonClicked()
        {
            LogEx.Log("Settings Button Clicked");
            UIManager.Instance.SettingParentUI.Show();
            UIManager.Instance.SoundSettingUI.ShowDefaultAsync().Forget();
            SoundManager.Instance.PlaySfx(SoundReference.UIClick);
        }
        public void OnExitButtonClicked()
        {
            LogEx.Log("Exit Button Clicked");
            SoundManager.Instance.PlaySfx(SoundReference.UIClick);
            Application.Quit();
        }



    }
}