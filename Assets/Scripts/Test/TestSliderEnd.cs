using System;
using System.Collections.Generic;
using Machamy.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Test
{
    public class TestSliderEnd : UIBehaviour
    {
        [SerializeField,HideInInspector] ParticleSystem _particleSystem;
        [SerializeField,HideInInspector] BoxCollider _boxCollider;

        List<ParticleSystem.Particle> enter = new ();
        List<Vector4> scores = new();
        
        public BoxCollider BoxCollider => _boxCollider;

        void Reset()
        {
            //base.Reset();
            if (_particleSystem == null)
            {
                _particleSystem = FindAnyObjectByType<ParticleSystem>();
            }
            if (_boxCollider == null)
            {
                _boxCollider = GetComponent<BoxCollider>();
            }

 
            
            OnRectTransformDimensionsChange();
        }

        protected override void Start()
        {
            base.Start();
            if (_particleSystem == null)
            {
                _particleSystem = FindAnyObjectByType<ParticleSystem>();
            }
            if (_boxCollider == null)
            {
                _boxCollider = GetComponent<BoxCollider>();
            }

        }
        
        private Vector2 _savedRectSize;

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;

            Vector2 newSize = rectTransform.rect.size;
            if (_savedRectSize != newSize)
            {
                _savedRectSize = newSize;
                if (_boxCollider != null)
                {
                    _boxCollider.size = new Vector3(newSize.x, newSize.y, 20f);
                    _boxCollider.center = Vector3.zero;
                }
            }
            
        }

        public void UpdateSize()
        {
            OnRectTransformDimensionsChange();
        }
        

        public void OnParticleTrigger()
        {
            LogEx.Log("OnParticleTrigger called");

        }
    }
}