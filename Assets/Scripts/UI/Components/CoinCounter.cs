using System;
using Cysharp.Threading.Tasks;
using ExecEvents;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    public class CoinCounter : CounterText
    {
        public Image goldIconImage;
        public override void Reset()
        {
            base.Reset();
            SetCounterValue(CounterValue);
        }
        [field:SerializeField] public bool autoUpdate { get; set; } = true;
        
        public Image GoldIconImage
        {
            get => goldIconImage;
        }
        

        private void OnEnable()
        {
            ExecEventBus<CurrentCoinChangedEventArgs>.RegisterStatic((int)ExecPriority.UIDefault, SetCoinAsync);
        }
        
        private void OnDisable()
        {
            
        }
        
        public void HideIcon()
        {
            if (goldIconImage != null)
            {
                goldIconImage.enabled = false;
            }
        }
        
        public void UpdateCoin(int newGoldAmount)
        {
            CounterValue = newGoldAmount;
        }
        
        public void OnCoinChanged(int newGoldAmount)
        {
            if (autoUpdate)
            {
                CounterValue = newGoldAmount;
            }
        }
        public UniTask SetCoinAsync(CurrentCoinChangedEventArgs evt)
        {
            if (autoUpdate)
            {
                CounterValue = evt.NewCurrentCoin;
            }
            return UniTask.CompletedTask;
        }
    }
}