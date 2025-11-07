using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Test
{
    [Serializable]
    public class TestRangeModel
    {
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue;
        [SerializeField] private float value;

        public TestRangeModel(float minValue, float maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.value = minValue;
        }

        public float NormalizedValue
        {
            get
            {
                if (Math.Abs(maxValue - minValue) < Mathf.Epsilon)
                {
                    return 0f;
                }

                return (value - minValue) / (maxValue - minValue);
            }
        }

        public void SetValue(float newValue)
        {
            value = Mathf.Clamp(newValue, minValue, maxValue);
        }
        
        public void AddValue(float delta)
        {
            SetValue(value + delta);
        }
    }
    
    public class TestSlider : MonoBehaviour
    {
        public Slider slider;

        public Ease updateEase = Ease.Linear;

        public AnimationCurve durationCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float baseDuration = 0.5f;

        public TestRangeModel rangeModel;

        public int increaseAmount = 100;
        
        private void Reset()
        {
            slider = GetComponent<Slider>();
        }

        private void Awake()
        {
            rangeModel = new TestRangeModel(0,1000);
            if (slider == null)
            {
                slider = GetComponent<Slider>();
            }
            UpdateSliderToRangeModel();
        }


        public void UpdateSlider(float value)
        {
            float currentValue = slider.value;
            float targetValue = Mathf.Clamp01(value);
            float distance = Mathf.Abs(targetValue - currentValue);
            float duration = baseDuration * durationCurve.Evaluate(distance);
            slider.DOKill();
            slider.DOValue(targetValue, duration).SetEase(updateEase);
        }
        
        public void UpdateSliderToRangeModel()
        {
            UpdateSlider(rangeModel.NormalizedValue);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(TestSlider))]
    public class TestSliderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            TestSlider myScript = (TestSlider)target;
            if (GUILayout.Button($"Update Slider to {myScript.increaseAmount}"))
            {
                myScript.rangeModel.SetValue(myScript.rangeModel.NormalizedValue * 1000 + myScript.increaseAmount);
                myScript.UpdateSliderToRangeModel();
                myScript.increaseAmount = Random.Range(50, 200);
            }
        }
    }
    #endif
}