using UnityEngine;

namespace UI
{
    /// <summary>
    /// UI의 기본 클래스입니다.
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {

        public virtual void Show()
        {
        }

        public virtual void Hide()
        {
        }

        public void SetVisible(bool visible)
        {
            if (visible)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
    }
}