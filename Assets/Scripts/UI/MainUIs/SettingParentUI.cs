namespace UI.MainUIs
{
    public class SettingParentUI : UIBase
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