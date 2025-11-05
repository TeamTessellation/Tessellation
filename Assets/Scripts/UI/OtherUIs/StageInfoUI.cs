using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Interaction;
using Stage;
using TMPro;
using UI.UISettings;
using UnityEngine;
using UnityEngine.UI;

namespace UI.OtherUIs
{
    public class StageInfoUI : UIBase
    {
        [SerializeField] private Image BackgroundImage;
        [SerializeField] private TMP_Text TitleText;
        [SerializeField] private TMP_Text StageLevelText;
        [SerializeField] private TMP_Text StageTargetScoreText;
        [SerializeField] private StageInfoUISettingSO stageInfoUISettingSO;
        private int _currentStageLevelView;
        private int _currentStageTargetScoreView;
        
        private TMP_Text[] _allTMPTexts;
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
        public int CurrentStageTargetScoreView
        {
            get => _currentStageTargetScoreView;
            set
            {
                _currentStageTargetScoreView = value;
                if (StageTargetScoreText != null)
                {
                    StageTargetScoreText.text = _currentStageTargetScoreView.ToString($"D{stageInfoUISettingSO.numLengthForTargetScore}");
                }
            }
        }

        private void Awake()
        {
            _allTMPTexts = GetComponentsInChildren<TMP_Text>(true); 
            gameObject.SetActive(false);
        }

        public override void Show()
        {
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public async UniTask ShowInfoRoutine(StageModel stageModel)
        {
            bool isConfirmed = false;
            Sequence currentSequence = null;
            void OnConfirmed()
            {
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
            await ShowRoutine();
            // 레벨
            var levelSequence = DOTween.Sequence();
            levelSequence.Append(DOTween
                .To(() => CurrentStageLevelView, x => CurrentStageLevelView = x, stageModel.StageLevel,
                    stageInfoUISettingSO.levelCountUpDuration)
                .SetEase(stageInfoUISettingSO.targetScoreCountUpEase));
            currentSequence = levelSequence;
            await levelSequence.ToUniTask();

            await UniTask.WhenAny(
                UniTask.Delay(500),
                UniTask.WaitUntil(() => isConfirmed)
            );
            // 목표 점수는 숫자가 올라가는 효과
            var sequence = DOTween.Sequence();
            sequence.Append(DOTween
                .To(() => CurrentStageTargetScoreView, x => CurrentStageTargetScoreView = x, stageModel.StageTargetScore,
                    stageInfoUISettingSO.targetScoreCountUpDuration)
                .SetEase(stageInfoUISettingSO.targetScoreCountUpEase));
            currentSequence = sequence;
            await sequence.ToUniTask();
            
            await UniTask.WhenAny(
                UniTask.Delay(500),
                UniTask.WaitUntil(() => isConfirmed)
            );
            

            float elapsedTime = 0f;
            while (!isConfirmed && elapsedTime < stageInfoUISettingSO.autoHideDelay)
            {
                elapsedTime += Time.deltaTime;
                await UniTask.Yield();
            }
            InteractionManager.Instance.ConfirmEvent -= OnConfirmed;
            await HideRoutine();
        }
        
        public async UniTask ShowRoutine()
        {
            Show();
            await UIManager.Instance.TransitionUI.PlayHexagonTransition(
                stageInfoUISettingSO.showFadeInDuration,
                stageInfoUISettingSO.showTransitionFadeType,
                stageInfoUISettingSO.showTransitionCurve,
                stageInfoUISettingSO.showTransitionDirectionType);
            await DOTween.To(() => 0f, SetFade, 1f, 0.2f)
                .SetEase(stageInfoUISettingSO.showFadeInEase)
                .ToUniTask();
        }
        
        public async UniTask HideRoutine()
        {
            await DOTween.To(() => 1f, SetFade, 0f, 0.2f)
                .SetEase(stageInfoUISettingSO.hideFadeOutEase)
                .ToUniTask();
            await UIManager.Instance.TransitionUI.PlayHexagonTransition(
                stageInfoUISettingSO.hideFadeOutDuration,
                stageInfoUISettingSO.hideTransitionFadeType,
                stageInfoUISettingSO.hideTransitionCurve,
                stageInfoUISettingSO.hideTransitionDirectionType);
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
            
        }


        [ContextMenu("Test Show Info Routine")]
        private async void CtxTestShowInfoRoutine()
        {
            UIManager.Instance.SwitchMainToGameUI();
            StageManager stageManager = StageManager.Instance;
            StageModel stageModel = stageManager.GetNextStage();
            stageManager.CurrentStage = stageModel;
            SetFade(0);
            await ShowInfoRoutine(stageModel);
        }
    }
}