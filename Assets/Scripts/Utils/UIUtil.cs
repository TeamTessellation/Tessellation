using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public static class UIUtil
    {
        
        public static Vector2 GetRectSize(this GameObject gameObject) 
        {
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                return new Vector2(rectTransform.rect.width, rectTransform.rect.height);
            }
            else
            {
                Debug.LogWarning("GameObject does not have a RectTransform component.");
                return Vector2.zero;
            }
        }
        public static Vector2 GetRectSize(this Transform transform) 
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform != null)
            {
                return new Vector2(rectTransform.rect.width, rectTransform.rect.height);
            }
            else
            {
                Debug.LogWarning("Transform is not a RectTransform.");
                return Vector2.zero;
            }
        }
        public static Vector2 GetRectSize(this RectTransform rectTransform)
        {
            return new Vector2(rectTransform.rect.width, rectTransform.rect.height);
        }

        public static Vector2 GetRectSizeByCanvas(this RectTransform rectTransform)
        {
            CanvasScaler canvasScaler = rectTransform.GetComponentInParent<CanvasScaler>();
            if (canvasScaler != null)
            {
                Vector2 referenceResolution = canvasScaler.referenceResolution;
                Vector2 parentSize = rectTransform.parent.GetRectSize();
                float scaleX = parentSize.x / referenceResolution.x;
                float scaleY = parentSize.y / referenceResolution.y;
                return new Vector2(rectTransform.rect.width * scaleX, rectTransform.rect.height * scaleY);
            }
            else
            {
                Debug.LogWarning("No CanvasScaler found in parent hierarchy.");
                return rectTransform.GetRectSize();
            }
        }
    }
}