using DG.Tweening;
using UI.OtherUIs;
using UnityEngine;

namespace UI.UISettings
{
    [CreateAssetMenu(fileName = "StageInfoUISettingSO", menuName = "Settings/Stage Info UI Setting SO")]
    public class StageInfoUISettingSO : ScriptableObject
    {
        [Header("Show Info Animation Settings")]
        public float showFadeInDuration = 2f;
        public Ease showFadeInEase = Ease.OutCubic;
        public DirectionType showTransitionDirectionType = DirectionType.Up2Down;
        public FadeType showTransitionFadeType = FadeType.In;
        public AnimationCurve showTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [Space]
        
        public int numLengthForLevel = 1;
        public float levelCountUpDuration = 1.0f;
        public int numLengthForTargetScore = 7;
        public float targetScoreCountUpDuration = 1.0f;
        public Ease targetScoreCountUpEase = Ease.OutCubic;
        
        [Space]
        [Space]
        [Header("Hide Info Animation Settings")]
        public float autoHideDelay = 2.0f;
        public float hideFadeOutDuration = 2f;
        public Ease hideFadeOutEase = Ease.InCubic;
        public DirectionType hideTransitionDirectionType = DirectionType.Down2Up;
        public FadeType hideTransitionFadeType = FadeType.Out;
        public AnimationCurve hideTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
    }
}