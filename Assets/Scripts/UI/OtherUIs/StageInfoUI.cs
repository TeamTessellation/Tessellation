using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Interaction;
using Stage;
using TMPro;
using UI.Components;
using UI.OtherUIs.Transitions;
using UI.UISettings;
using UnityEngine;
using UnityEngine.UI;

namespace UI.OtherUIs
{
    [RequireComponent(typeof(CanvasGroup))]
    public class StageInfoUI : UIBase
    {
        [SerializeField] private Image BackgroundImage;
        [SerializeField] private TMP_Text TitleText;
        [SerializeField] private TMP_Text StageLevelText;
        [SerializeField] private CounterText StageTargetScoreCounterText;
        [SerializeField] private HexTransition hexTransition;
        [SerializeField] private StageInfoUISettingSO stageInfoUISettingSO;
        private int _currentStageLevelView;

        private CanvasGroup _canvasGroup;
        
        private TMP_Text[] _allTMPTexts;
        
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        
        public int CurrentStageLevelView
        {
            get => _currentStageLevelView;
            set
            {
                _currentStageLevelView = value;
                if (StageLevelText != null)
                {
                    StageLevelText.text = _currentStageLevelView.ToString($"D{stageInfoUISettingSO.numLengthForLevel}");
                }
            }
        }
        
        public string CurrentStageNameView
        {
            get => StageLevelText != null ? StageLevelText.text : string.Empty;
            set
            {
                if (StageLevelText != null)
                {
                    StageLevelText.text = value;
                }
            }
        }
        


        protected override void Awake()
        {
            // _allTMPTexts = GetComponentsInChildren<TMP_Text>(true); 
            gameObject.SetActive(false);
            _canvasGroup = GetComponent<CanvasGroup>();
            hexTransition = GetComponentInChildren<HexTransition>(true);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        
        public async UniTask ShowInfoRoutine(StageModel stageModel, CancellationToken cancellationToken)
        {
            bool isConfirmed = false;
            Sequence currentSequence = null;
            gameObject.SetActive(true);
            SetFade(0f);
            void OnConfirmed()
            {
                // ReSharper disable AccessToModifiedClosure
                if (currentSequence != null && currentSequence.IsActive())
                {
                    currentSequence.Complete();
                }
                else
                {
                    isConfirmed = true;
                }
            }
            InteractionManager.Instance.ConfirmEvent += OnConfirmed;
            // 보이기
            Show();
            CurrentStageNameView = stageModel.StageName;

            await hexTransition.PlayHexagonTransition(
                stageInfoUISettingSO.showFadeInDuration,
                stageInfoUISettingSO.showTransitionFadeType,
                stageInfoUISettingSO.showTransitionCurve,
                stageInfoUISettingSO.showTransitionDirectionType, cancellationToken: cancellationToken);

            
            await DOTween.To(() => 0f, SetFade, 1f, 0.2f)
                .SetEase(stageInfoUISettingSO.showFadeInEase)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: cancellationToken);
            

            // 타이밍

            await UniTask.WhenAny(
                UniTask.Delay(500, cancellationToken: cancellationToken),
                UniTask.WaitUntil(() => isConfirmed, cancellationToken: cancellationToken)
            );
            // StageTargetScoreCounterText.PaddingChar = ' ';
            var sequence = DOTween.Sequence();
            sequence.Append(
                StageTargetScoreCounterText.DoCount(stageModel.StageTargetScore,
                        stageInfoUISettingSO.targetScoreCountUpDuration)
                    .SetEase(stageInfoUISettingSO.targetScoreCountUpEase)
            );
            currentSequence = sequence;
            await sequence.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: cancellationToken);
            
            await UniTask.WhenAny(
                UniTask.Delay(500, cancellationToken: cancellationToken),
                UniTask.WaitUntil(() => isConfirmed, cancellationToken: cancellationToken)
            );
            

            float elapsedTime = 0f;
            while (!isConfirmed && elapsedTime < stageInfoUISettingSO.autoHideDelay)
            {
                elapsedTime += Time.deltaTime;
                await UniTask.Yield(cancellationToken);
            }
            InteractionManager.Instance.ConfirmEvent -= OnConfirmed;
            
        }

        public async UniTask HideInfoRoutine(CancellationToken cancellationToken)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            await DOTween.To(() => 1f, SetFade, 0f, 0.2f)
                .SetEase(stageInfoUISettingSO.hideFadeOutEase)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: cancellationToken);
            await hexTransition.PlayHexagonTransition(
                stageInfoUISettingSO.hideFadeOutDuration,
                stageInfoUISettingSO.hideTransitionFadeType,
                stageInfoUISettingSO.hideTransitionCurve,
                stageInfoUISettingSO.hideTransitionDirectionType, cancellationToken: cancellationToken);
            Hide();
        }

        public void SetFade(float alpha)
        {
            if (_allTMPTexts == null)
            {
                _allTMPTexts = GetComponentsInChildren<TMP_Text>(true);
            }
            if (BackgroundImage != null)
            {
                Color color = BackgroundImage.color;
                color.a = alpha;
                BackgroundImage.color = color;
            }
            foreach (var tmpText in _allTMPTexts)
            {
                Color color = tmpText.color;
                color.a = alpha;
                tmpText.color = color;
            }
            //_canvasGroup.alpha = alpha;
        }
        
    }
}