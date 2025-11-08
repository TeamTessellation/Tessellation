namespace UI.MainUIs
{
    public class SettingUI : UIBase
    {      
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