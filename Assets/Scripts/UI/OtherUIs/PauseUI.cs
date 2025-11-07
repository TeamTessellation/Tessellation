using System;
using System.Collections.Generic;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Machamy.Utils;
using UnityEngine;
using UnityEngine.InputSystem.XR;
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
        
        [Header("On Enable Transitions")] 
        [SerializeField]private float onEnableTransitionDuration = 0.5f;
        [SerializeField]private Ease onEnableTransitionEase = Ease.OutBack;
        [SerializeField]private float onDisableTransitionDuration = 0.5f;
        [SerializeField]private Ease onDisableTransitionEase = Ease.InBack;
        
        [SerializeField]private List<TransitionDataList> onEnableTransitionMap;
        
        [Serializable]
        public class TransitionData
        {
            public Transform startTransform;
            public Transform target;
        }
        [Serializable]
        public class TransitionDataList
        {
            public List<TransitionData> entries;
        }


        private Dictionary<Transform, Vector3> _originalPositions;
        
        
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
        
        CancellationTokenSource cancellationTokenSource;

        protected override void Awake()
        {
            base.Awake();
            cancellationTokenSource = new CancellationTokenSource();
            
            _originalPositions = new Dictionary<Transform, Vector3>();
            if (onEnableTransitionMap == null) return;
            
            foreach (var transitionList in onEnableTransitionMap)
            {
                foreach (var entry in transitionList.entries)
                {
                    if (entry.target != null && !_originalPositions.ContainsKey(entry.target))
                    {
                        _originalPositions[entry.target] = entry.target.localPosition;
                    }
                }
            }
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
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            
            gameObject.SetActive(true); 
            
            PlayOnEnableTransitionsAsync(cancellationTokenSource.Token).Forget();
        }
        
        public override void Hide()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            
            HideAndDisableAsync(cancellationTokenSource.Token).Forget();
        }

        /// <summary>
        /// Hide 애니메이션을 재생하고, 완료되면 스스로 비활성화하는 헬퍼
        /// </summary>
        private async UniTask HideAndDisableAsync(CancellationToken cancellationToken)
        {
            try
            {
                await PlayOnDisableTransitionsAsync(cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    gameObject.SetActive(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Hide 도중 Show가 호출되어 취소됨
            }
        }
        
        public async UniTask PlayOnEnableTransitionsAsync(CancellationToken cancellationToken)
        {
            List<UniTask> transitionTasks = new List<UniTask>();
            foreach (var transition in onEnableTransitionMap)
            {
                foreach (var entry in transition.entries)
                {
                    entry.target.gameObject.SetActive(true);
                    entry.target.position = entry.startTransform.position;
                    LogEx.Log($"Moving {entry.target.name} to start position {entry.startTransform.localPosition} (name: {entry.startTransform.name})");
                    
                    // 원래 위치로 이동하는 애니메이션
                    var task = entry.target.DOLocalMove(_originalPositions[entry.target], onEnableTransitionDuration)
                        .SetEase(onEnableTransitionEase)
                        .SetUpdate(true)
                        .ToUniTask(cancellationToken: cancellationToken);
                    
                    transitionTasks.Add(task);
                }
                await UniTask.WhenAll(transitionTasks);
                transitionTasks.Clear();
            }
            
        }
        
        public async UniTask PlayOnDisableTransitionsAsync(CancellationToken cancellationToken)
        {
            List<UniTask> transitionTasks = new List<UniTask>();
            for(int i = onEnableTransitionMap.Count - 1; i >= 0; i--)
            {
                var transition = onEnableTransitionMap[i];
                foreach (var entry in transition.entries)
                {
                    // 시작 위치로 이동하는 애니메이션
                    var task = entry.target.DOLocalMove(entry.startTransform.localPosition, onDisableTransitionDuration)
                        .SetEase(onDisableTransitionEase)
                        .OnComplete(() =>
                        {
                            entry.target.gameObject.SetActive(false);
                        })
                        .SetUpdate(true)
                        .ToUniTask(cancellationToken: cancellationToken);
                    
                    
                    transitionTasks.Add(task);
                }
                await UniTask.WhenAll(transitionTasks);
                transitionTasks.Clear();
            }
            await UniTask.WaitForSeconds(0.1f, cancellationToken: cancellationToken);
            
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
            Hide(); // Hide()는 이제 애니메이션 후 비활성화를 올바르게 처리합니다.
            LogEx.Log("재개 버튼 클릭됨");
            GameManager.Instance.ResumeGame();
        }
        
        public void OnHomeButtonClicked()
        {
            Hide(); // Hide()는 이제 애니메이션 후 비활성화를 올바르게 처리합니다.
            LogEx.Log("홈 버튼 클릭됨");
            GameManager.Instance.ResetGameAndReturnToMainMenu();
        }
    }
}