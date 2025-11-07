using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Components
{
    public class TransitionText : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text mainText;
        [SerializeField] private TMPro.TMP_Text effectText;
        [Header("Motion Settings")]
        public float moveDuration = 1.0f;
        public Ease moveEase = Ease.Linear;
        public float moveGlobalOffsetY = 0.0f;
        [Tooltip("UI 크기 비례 거리")] public float moveLocalOffsetY = 1.5f;
        [Header("Fade Settings")]
        public float fadeStartDelay = 0.0f;
        public float fadeDuration = 1.0f;
        public Ease fadeEase = Ease.Linear;

        private void Awake()
        {
            effectText.gameObject.SetActive(false);
        }

        public void SetText(string text)
        {
            if (mainText != null)
            {
                mainText.text = text;
            }
            mainText.gameObject.SetActive(true);
            effectText.gameObject.SetActive(false);
        }
        
        public float GetRectSizeY()
        {
            RectTransform rectTransform = mainText.GetComponent<RectTransform>();
            
            return rectTransform.rect.height;
        }
        

        public void AnimateText(string text)
        {
            
            Vector3 originalPosition = Vector3.zero;
            float calculatedOffsetY = moveLocalOffsetY * GetRectSizeY() + moveGlobalOffsetY;
            
            effectText.text = text;
            effectText.gameObject.SetActive(true);
            effectText.transform.localPosition = new Vector3(
                mainText.transform.localPosition.x,
                originalPosition.y + calculatedOffsetY,
                mainText.transform.localPosition.z 
            );
            Sequence seq = DOTween.Sequence();
            seq.Append(mainText.transform.DOLocalMoveY(mainText.transform.localPosition.y - calculatedOffsetY, moveDuration).SetEase(moveEase));
            seq.Join(effectText.transform.DOLocalMoveY(originalPosition.y, moveDuration).SetEase(moveEase));
            seq.Join(effectText.DOFade(1.0f, fadeDuration).SetEase(fadeEase)).SetDelay(fadeStartDelay);
            seq.Join(mainText.DOFade(0.0f, fadeDuration).SetEase(fadeEase));
            seq.OnComplete(() =>
            {
                // 스왑
                (mainText, effectText) = (effectText, mainText);
                mainText.name = "MainText";
                effectText.name = "EffectText";
                mainText.transform.SetAsFirstSibling();
                mainText.gameObject.SetActive(true);
                effectText.gameObject.SetActive(false);
                mainText.transform.localPosition = originalPosition;
            });
            seq.Play();
        }
        
        
        public int testNewText = 1;

        [ContextMenu("Test Animate Text")]
        private void TestAnimateText()
        {
            AnimateText($"{testNewText++}");
        }
    }
}