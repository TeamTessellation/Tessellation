using Machamy.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

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
        [field:SerializeField] public EventTrigger HandEventTrigger { get; private set; }
        [field:SerializeField] public RectTransform EntryLayoutTransform { get; private set; }
        
        public void Show()
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.one;
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
    
    
}