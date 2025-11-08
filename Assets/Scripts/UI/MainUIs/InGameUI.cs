using Cysharp.Threading.Tasks;
using ExecEvents;
using Stage;

namespace UI.MainUIs
{
    public class InGameUI : UIBase
    {
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
            return UniTask.CompletedTask;
        }
        
    }
}