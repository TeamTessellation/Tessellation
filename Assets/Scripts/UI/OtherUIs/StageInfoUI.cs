using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Interaction;
using Stage;
using TMPro;
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
        [SerializeField] private TMP_Text StageTargetScoreText;
        [SerializeField] private HexTransition hexTransition;
        [SerializeField] private StageInfoUISettingSO stageInfoUISettingSO;
        private int _currentStageLevelView;
        private int _currentStageTargetScoreView;
        private CanvasGroup _canvasGroup;
        
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

        protected override void Awake()
        {
            // _allTMPTexts = GetComponentsInChildren<TMP_Text>(true); 
            gameObject.SetActive(false);
            _canvasGroup = GetComponent<CanvasGroup>();
            hexTransition = GetComponentInChildren<HexTransition>(true);
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
            // var levelSequence = DOTween.Sequence();
            // levelSequence.Append(DOTween
            //     .To(() => CurrentStageLevelView, x => CurrentStageLevelView = x, stageModel.StageLevel,
            //         stageInfoUISettingSO.levelCountUpDuration)
            //     .SetEase(stageInfoUISettingSO.targetScoreCountUpEase));
            // currentSequence = levelSequence;
            // await levelSequence.ToUniTask();

            CurrentStageNameView = stageModel.StageName;

            // 타이밍

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

            await hexTransition.PlayHexagonTransition(
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
            await hexTransition.PlayHexagonTransition(
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
            //_canvasGroup.alpha = alpha;
        }
        
    }
}