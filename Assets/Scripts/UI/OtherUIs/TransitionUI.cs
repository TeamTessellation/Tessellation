using System;
using Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        private static readonly int ProgressHash = Shader.PropertyToID("_Progress");
        private static readonly int DirectoinHash = Shader.PropertyToID("_Direction");
        private static readonly int AngleHash = Shader.PropertyToID("_Angle");
        private static readonly int IntervalHash = Shader.PropertyToID("_FadeType");
        
        [SerializeField] Image _hexTransitionImage;
        [SerializeField] Material _hexTransitionMaterial;

        private void Awake()
        {
            _hexTransitionMaterial = _hexTransitionImage.material;
        }

        private void Start()
        {
            _hexTransitionMaterial.SetFloat(ProgressHash, 1f);
            _hexTransitionMaterial.SetFloat(DirectoinHash, 0f);
        }


        public UniTask PlayTransition(TransitionType type, float duration)
        {
            switch (type)
            {
                case TransitionType.Hexagon:
                    return PlayHexagonTransition(duration, FadeType.Out, DirectionType.Down2Up, 0);
                default:
                    return UniTask.CompletedTask;
            }
        }

        public async UniTask PlayHexagonTransition(float duration, FadeType fade,
            DirectionType direction = DirectionType.Down2Up, Ease easeType = Ease.Linear, float angle = 0f)
        {
            _hexTransitionImage.gameObject.SetActive(true);
            _hexTransitionMaterial.SetFloat(DirectoinHash, direction == DirectionType.Down2Up ? 0f : 1f);
            _hexTransitionMaterial.SetFloat(AngleHash, angle);
            
            float elapsed = 0f;
            float from = fade == FadeType.In ? 1f : 0f;
            float to = fade == FadeType.In ? 0f : 1f;
            
            // while (elapsed < duration)
            // {
            //     elapsed += Time.unscaledDeltaTime;
            //     float progress = Mathf.Lerp(from, to, elapsed / duration);
            //     _hexTransitionMaterial.SetFloat(ProgressHash, progress);
            //     await UniTask.Yield();
            // }
            await DOTween.To(() => from, x =>
            {
                from = x;
                _hexTransitionMaterial.SetFloat(ProgressHash, from);
            }, to, duration).SetEase(easeType).ToUniTask();
            
            _hexTransitionMaterial.SetFloat(ProgressHash, to);
        }
        
        public async UniTask PlayHexagonTransition(float duration, FadeType fade, AnimationCurve curve,
            DirectionType direction = DirectionType.Down2Up, float angle = 0f)
        {
            _hexTransitionImage.gameObject.SetActive(true);
            _hexTransitionMaterial.SetFloat(DirectoinHash, direction == DirectionType.Down2Up ? 0f : 1f);
            _hexTransitionMaterial.SetFloat(AngleHash, angle);
            
            float elapsed = 0f;
            float from = fade == FadeType.In ? 1f : 0f;
            float to = fade == FadeType.In ? 0f : 1f;
            
            // while (elapsed < duration)
            // {
            //     elapsed += Time.unscaledDeltaTime;
            //     float progress = Mathf.Lerp(from, to, elapsed / duration);
            //     _hexTransitionMaterial.SetFloat(ProgressHash, progress);
            //     await UniTask.Yield();
            // }
            await DOTween.To(() => from, x =>
            {
                from = x;
                _hexTransitionMaterial.SetFloat(ProgressHash, from);
            }, to, duration).SetEase(curve).ToUniTask();
            
            _hexTransitionMaterial.SetFloat(ProgressHash, to);
        }
    }
}