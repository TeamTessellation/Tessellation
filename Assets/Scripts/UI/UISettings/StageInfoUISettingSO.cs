using DG.Tweening;
using UnityEngine;

namespace UI.UISettings
{
    [CreateAssetMenu(fileName = "StageInfoUISettingSO", menuName = "Settings/Stage Info UI Setting SO")]
    public class StageInfoUISettingSO : ScriptableObject
    {
        [Header("Show Info Animation Settings")]
        public float showFadeInDuration = 0.5f;
        public Ease showFadeInEase = Ease.OutCubic;
        [Space]
        
        public int numLengthForLevel = 3;
        public float levelCountUpDuration = 1.0f;
        public Ease levelCountUpEase = Ease.OutCubic;
        public int numLengthForTargetScore = 7;
        public float targetScoreCountUpDuration = 1.0f;
        public Ease targetScoreCountUpEase = Ease.OutCubic;
        
        [Space]
        [Space]
        [Header("Hide Info Animation Settings")]
        public float autoHideDelay = 2.0f;
        public float hideFadeOutDuration = 0.5f;
        public Ease hideFadeOutEase = Ease.InCubic;
    }
}