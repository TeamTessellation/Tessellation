using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Components
{
    
    
    public class RaycastDelegator : UIBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler, IMoveHandler, ISelectHandler, IDeselectHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IScrollHandler
    {
        [field: SerializeField] public MonoBehaviour Target { get; private set; }

        private EventSystem EventSystem => EventSystem.current;

        #if UNITY_EDITOR
        public enum UIType
        {
            Button,
            Toggle,
            Slider,
            InputField,
            ScrollRect,
            Image,
        }

        public UIType TargetUIType = UIType.Button;
        protected override void Reset()
        {
            switch (TargetUIType)
            {
                case UIType.Button:
                    Target = GetComponentInParent<UnityEngine.UI.Button>();
                    break;
                case UIType.Toggle:
                    Target = GetComponentInParent<UnityEngine.UI.Toggle>();
                    break;
                case UIType.Slider:
                    Target = GetComponentInParent<UnityEngine.UI.Slider>();
                    break;
                case UIType.InputField:
                    Target = GetComponentInParent<UnityEngine.UI.InputField>();
                    break;
                case UIType.ScrollRect:
                    Target = GetComponentInParent<UnityEngine.UI.ScrollRect>();
                    break;
                case UIType.Image:
                    Target = GetComponentInParent<UnityEngine.UI.Image>();
                    break;
            }
        }
        
        #endif

        public void OnPointerUp(PointerEventData eventData)
        {
            if(Target is IPointerUpHandler pointerUpHandler)
            {
                pointerUpHandler.OnPointerUp(eventData);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if(Target is IPointerDownHandler pointerDownHandler)
            {
                pointerDownHandler.OnPointerDown(eventData);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(Target is IPointerClickHandler pointerClickHandler)
            {
                pointerClickHandler.OnPointerClick(eventData);
            }
        }

        public void OnMove(AxisEventData eventData)
        {
            if(Target is IMoveHandler moveHandler)
            {
                moveHandler.OnMove(eventData);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if(Target is ISelectHandler selectHandler)
            {
                selectHandler.OnSelect(eventData);
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if(Target is IDeselectHandler deselectHandler)
            {
                deselectHandler.OnDeselect(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(Target is IDragHandler dragHandler)
            {
                dragHandler.OnDrag(eventData);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if(Target is IBeginDragHandler beginDragHandler)
            {
                beginDragHandler.OnBeginDrag(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if(Target is IEndDragHandler endDragHandler)
            {
                endDragHandler.OnEndDrag(eventData);
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            if(Target is IScrollHandler scrollHandler)
            {
                scrollHandler.OnScroll(eventData);
            }
        }
    }
}