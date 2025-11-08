using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

namespace UI.Components
{
    public class CounterText : MonoBehaviour
    {
        [SerializeField] private TMP_Text counterTMPText;
        [SerializeField] private int numberPaddingLength = 0;
        
        private int _counterValue;
        public int CounterValue
        {
            get => _counterValue;
            set
            {
                _counterValue = value;
                SetCounterValue(_counterValue);
            }
        }

        public string Text
        {
            get => counterTMPText != null ? counterTMPText.text : string.Empty;
            set
            {
                if (counterTMPText != null)
                {
                    counterTMPText.text = value;
                }
            }
        }
        
        public void SetCounterValue(int value)
        {
            if (counterTMPText != null)
            {
                counterTMPText.text = value.ToString(numberPaddingLength > 0 ? $"D{numberPaddingLength}" : "D");
            }
        }
        

        public TweenerCore<int, int, NoOptions> DoCount(int from, int to, float duration)
        {
            _counterValue = from;
            var tween = DOTween.To(() => _counterValue, x =>
            {
                CounterValue = x;
            }, to, duration);
            return tween;
        }
        
        public TweenerCore<int, int, NoOptions> DoCount(int to, float duration)
        {
            return DoCount(_counterValue, to, duration);
        }


        private void OnValidate()
        {
            SetCounterValue(_counterValue);
        }
    }
}