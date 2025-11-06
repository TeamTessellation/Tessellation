using System;
using System.Collections.Generic;
using Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UI.OtherUIs.Transitions;
using UnityEngine;
using UnityEngine.UI;

namespace UI.OtherUIs
{
    public enum TransitionType
    {
        Hexagon,
    }
    
    public enum DirectionType
    {
        Down2Up,
        Up2Down
    }
    
    public enum FadeType
    {
        In,
        Out
    }
    public class TransitionUI : UIBase
    {
        [SerializeField,HideInInspector] private Canvas _canvas;
        [SerializeField] private HexTransition hexTransition;

        

        
        protected override void Reset()
        {
            base.Reset();
            _canvas = GetComponent<Canvas>();
            hexTransition = GetComponentInChildren<HexTransition>(true);
        }

        private void Awake()
        {
            if (_canvas == null)
            {
                _canvas = GetComponent<Canvas>();
            }
        }
        
        private void Start()
        {
            hexTransition.Progress = 1f;
            hexTransition.DirectionType = DirectionType.Down2Up;
        }
        



        public UniTask PlayTransition(TransitionType type, float duration)
        {
            switch (type)
            {
                case TransitionType.Hexagon:
                    return hexTransition.PlayHexagonTransition(duration, FadeType.Out, DirectionType.Down2Up, 0);
                default:
                    return UniTask.CompletedTask;
            }
        }


    }
}