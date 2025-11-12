using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.OtherUIs
{
    public class ShopUI : UIBase
    {
        [Header("Shop UI Components")] 
        [SerializeField] private TMP_Text stageNameText;
        [SerializeField] private TMP_Text goldText;
        
        [Header("Shop Settings")]
        [SerializeField] private int itemCount = 4;
        
        [Header("Tween Settings")] 
        // TODO..

        [Space(20)] 
        
        [Header("Buttons")] 
        [SerializeField] private Button _rerollButton;
        [SerializeField] private Button _skipButton;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private List<Tween> currentTweenList = new List<Tween>();

        protected override void Awake()
        {
            _rerollButton.onClick.AddListener(OnRerollButtonClicked);
            _skipButton.onClick.AddListener(OnSkipButtonClicked);
            gameObject.SetActive(false);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }

        private void OnEnable()
        {
            InteractionManager.Instance.ConfirmEvent += OnConfirmed;
        }
        private void OnDisable()
        {
            InteractionManager.Instance.ConfirmEvent -= OnConfirmed;
        }
        
        private void OnRerollButtonClicked()
        {
            
        }

        private void OnSkipButtonClicked()
        {
            
        }

        /// <summary>
        /// 아무 키나 입력이 들어왔을 때 실행되는 로직
        /// </summary>
        private void OnConfirmed()
        {
            foreach (var tween in currentTweenList)
            {
                if(tween.IsActive() && tween.IsPlaying())
                    tween.Complete();
            }
            currentTweenList.Clear();
        }
    }
}
