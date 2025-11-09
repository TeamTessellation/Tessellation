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
        [SerializeField] private string formatString = "{0} 점";
        [SerializeField] private char paddingChar = '0';
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
        
        public int PaddingLength
        {
            get => numberPaddingLength;
            set
            {
                numberPaddingLength = value;
                SetCounterValue(_counterValue);
            }
        }
        
        public char PaddingChar
        {
            get => paddingChar;
            set
            {
                paddingChar = value;
                SetCounterValue(_counterValue);
            }
        }
        
        public void SetCounterValue(int value)
        {
            if (counterTMPText != null)
            {
                counterTMPText.text = string.Format(formatString, numberPaddingLength > 0 ? value.ToString().PadLeft(numberPaddingLength, paddingChar) : value.ToString());
            }
        }
        

        public TweenerCore<int, int, NoOptions> DoCount(int from, int to, float duration, bool setPaddingToToValue = true)
        {
            if (setPaddingToToValue)
            {
                int toValueLength = to.ToString().Length;
                if (toValueLength > numberPaddingLength)
                {
                    numberPaddingLength = toValueLength;
                }
            }
            _counterValue = from;
            var tween = DOTween.To(() => _counterValue, x =>
            {
                CounterValue = x;
            }, to, duration);
            return tween;
        }
        
        public TweenerCore<int, int, NoOptions> DoCount(int to, float duration, bool setPaddingToToValue = true)
        {
            return DoCount(_counterValue, to, duration, setPaddingToToValue);
        }


        private void OnValidate()
        {
            SetCounterValue(_counterValue);
        }
    }
}