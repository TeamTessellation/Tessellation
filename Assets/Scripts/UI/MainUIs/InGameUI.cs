using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ExecEvents;
using Stage;
using TMPro;
using UI.Components;
using UI.OtherUIs;
using UnityEngine;

namespace UI.MainUIs
{
    public class InGameUI : UIBase
    {
        [SerializeField] private TMP_Text currentStageText;
        [field:SerializeField] public CoinCounter CoinCounter { get; private set; }

        [field:SerializeField] public List<ItemPlaceEntry> ItemPlaceEntries { get; private set; }

        [field:SerializeField] public Transform IngameInventoryPosition { get; private set; }
        [field:SerializeField] public Transform ShopInventoryPosition { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            
            T FindUIComponent<T>() where T : Component
            {
                T ui = GetComponentInChildren<T>();
                if (ui == null)
                {
                    Debug.LogError($"InGameUI: Could not find {typeof(T).Name} in children.");
                }
                return ui;
            }
            CoinCounter = FindUIComponent<CoinCounter>();
            
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void RegisterEvents()
        {
            ExecEventBus<StageStartEventArgs>.RegisterStatic(1, OnStageStarted);
        }
        
        public void UnregisterEvents()
        {
            ExecEventBus<StageStartEventArgs>.UnregisterStatic(OnStageStarted);
        }
        
        private UniTask OnStageStarted(StageStartEventArgs args)
        {
            Show();
            currentStageText.text = args.StageModel.StageName;
            return UniTask.CompletedTask;
        }
        
    }
}