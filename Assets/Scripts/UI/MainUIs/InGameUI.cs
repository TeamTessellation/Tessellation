using Cysharp.Threading.Tasks;
using ExecEvents;
using Stage;
using TMPro;
using UnityEngine;

namespace UI.MainUIs
{
    public class InGameUI : UIBase
    {
        [SerializeField] private TMP_Text currentStageText;
        
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void RegisterEvents()
        {
            ExecEventBus<StageStartEventArgs>.RegisterStatic(1, OnStageStarted);
        }
        
        public void UnregisterEvents()
        {
            ExecEventBus<StageStartEventArgs>.UnregisterStatic(OnStageStarted);
        }
        
        private UniTask OnStageStarted(StageStartEventArgs args)
        {
            Show();
            currentStageText.text = args.StageModel.StageName;
            return UniTask.CompletedTask;
        }
        
    }
}