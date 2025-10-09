using UnityEngine;
using UnityEngine.UIElements;

namespace Machamy.UIToolkit
{
    public sealed class DragManipulator : PointerManipulator
    {
        private readonly VisualElement _panel;
        private Vector2 _startMouse;
        private Vector2 _startPos;
        private bool _active;

        
        public bool ClampToParentBounds = true;
        public RectOffset Padding = new RectOffset(0,0,0,0); // 부모 가장자리 여백

        public DragManipulator(VisualElement handle, VisualElement panel)
        {
            target = handle; // 드래그를 감지할 핸들 (TopBar)
            _panel = panel;  // 실제로 이동할 패널 (Root)
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
            // 현재 패널의 좌표(좌상단 기준) 확보
            _startPos = new Vector2(_panel.resolvedStyle.left, _panel.resolvedStyle.top);
            // 인라인 스타일에 없으면 0으로 들어올 수 있으므로 일단 style에 값을 고정
            _panel.style.left = _startPos.x;
            _panel.style.top  = _startPos.y;

            target.CapturePointer(evt.pointerId);
            evt.StopImmediatePropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_active || !target.HasPointerCapture(evt.pointerId)) return;

            Vector2 mousePos = evt.position;
            var delta = mousePos - _startMouse;
            float newLeft = _startPos.x + delta.x;
            float newTop  = _startPos.y + delta.y;

            if (ClampToParentBounds && _panel.parent != null)
            {
                var parentRect = _panel.parent.contentRect;
                var size = new Vector2(_panel.resolvedStyle.width, _panel.resolvedStyle.height);

                float minX = parentRect.xMin + Padding.left;
                float maxX = parentRect.xMax - size.x - Padding.right;
                float minY = parentRect.yMin + Padding.top;
                float maxY = parentRect.yMax - size.y - Padding.bottom;

                newLeft = Mathf.Clamp(newLeft, minX, maxX);
                newTop  = Mathf.Clamp(newTop,  minY, maxY);
            }

            _panel.style.left = newLeft;
            _panel.style.top  = newTop;

            evt.StopPropagation();
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