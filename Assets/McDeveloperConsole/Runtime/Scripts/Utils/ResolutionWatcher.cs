using System;
using UnityEngine;

namespace Machamy.Utils
{
    public class ResolutionWatcher : MonoBehaviour
    {
        public Vector2Int CurrentResolution { get; private set; }
        public event System.Action<Vector2Int> OnResolutionChanged;
        
        private void Start()
        {
            CurrentResolution = new Vector2Int(Screen.width, Screen.height);
        }

        private void Update()
        {
            if (Screen.width != CurrentResolution.x || Screen.height != CurrentResolution.y)
            {
                CurrentResolution = new Vector2Int(Screen.width, Screen.height);
                OnResolutionChanged?.Invoke(CurrentResolution);
            }
        }
    }
}