using Machamy.Utils;

namespace UI.OtherUIs
{
    public class HandCanvas : UIBase
    {
        private static HandCanvas _instance;
        public static HandCanvas Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<HandCanvas>();
                    if (_instance == null)
                    {
                        LogEx.LogError("HandCanvas instance not found in the scene.");
                    }
                }
                return _instance;
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
    }
    
    
}