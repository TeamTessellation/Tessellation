
using UnityEngine;
using UnityEngine.UIElements;
namespace Machamy.UIToolkit
{
    public enum ResizeEdge { East, South, SouthEast }

    public sealed class ResizeManipulator : PointerManipulator
    {
        private readonly VisualElement _panel;
        private readonly ResizeEdge _edge;
        private Vector2 _startMouse;
        private Vector2 _startSize;
        private bool _active;

        public Vector2 MinSize = new Vector2(300, 180);
        public Vector2 MaxSize = new Vector2(1920, 1080);
        public bool ClampToParentBounds = true;

        public ResizeManipulator(VisualElement handle, VisualElement panel, ResizeEdge edge)
        {
            target = handle;
            _panel = panel;
            _edge = edge;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return; // 좌클릭만
            _active = true;
            _startMouse = evt.position;
            _startSize = new Vector2(_panel.resolvedStyle.width, _panel.resolvedStyle.height);
            target.CapturePointer(evt.pointerId);
            evt.StopImmediatePropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_active || !target.HasPointerCapture(evt.pointerId)) return;

            Vector2 mousePos = evt.position;
            Vector2 delta = mousePos - _startMouse;
            float newW = _startSize.x;
            float newH = _startSize.y;

            switch (_edge)
            {
                case ResizeEdge.East:
                    newW = _startSize.x + delta.x;
                    break;
                case ResizeEdge.South:
                    newH = _startSize.y + delta.y;
                    break;
                case ResizeEdge.SouthEast:
                    newW = _startSize.x + delta.x;
                    newH = _startSize.y + delta.y;
                    break;
            }

            newW = Mathf.Clamp(newW, MinSize.x, MaxSize.x);
            newH = Mathf.Clamp(newH, MinSize.y, MaxSize.y);

            if (ClampToParentBounds && _panel.parent != null)
            {
                var parent = _panel.parent.contentRect.size;
                newW = Mathf.Min(newW, parent.x);
                newH = Mathf.Min(newH, parent.y);
            }

            _panel.style.width = newW;
            _panel.style.height = newH;

            evt.StopPropagation();
        }

        public void Clamp()
        {
            float newW = Mathf.Clamp(_panel.resolvedStyle.width, MinSize.x, MaxSize.x);
            float newH = Mathf.Clamp(_panel.resolvedStyle.height, MinSize.y, MaxSize.y);

            if (ClampToParentBounds && _panel.parent != null)
            {
                var parent = _panel.parent.contentRect.size;
                newW = Mathf.Min(newW, parent.x);
                newH = Mathf.Min(newH, parent.y);
            }

            _panel.style.width = newW;
            _panel.style.height = newH;
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_active) return;
            _active = false;
            if (target.HasPointerCapture(evt.pointerId))
                target.ReleasePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent _)
        {
            _active = false;
        }
    }
}