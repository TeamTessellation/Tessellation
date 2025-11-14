using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
            ResetInventoryPositionY();
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

        public void ResetInventoryPositionY()
        {
            foreach (var itemPlaceEntry in ItemPlaceEntries)
            {
                var position = itemPlaceEntry.transform.position;
                position.y = IngameInventoryPosition.position.y;
                itemPlaceEntry.transform.position = position;
            }
        }
        public Tween MoveInventoryYToShopPosition(float duration, Ease ease)
        {
            var tweens = new List<Tween>();
            foreach (var itemPlaceEntry in ItemPlaceEntries)
            {
                var tween = itemPlaceEntry.transform.DOMoveY(ShopInventoryPosition.position.y, duration).SetEase(ease);
                tweens.Add(tween);
            }
            
            return DOTween.Sequence().AppendInterval(0).Join(tweens[0]);
        }
        public Tween MoveInventoryYToIngamePosition(float duration, Ease ease)
        {
            var tweens = new List<Tween>();
            foreach (var itemPlaceEntry in ItemPlaceEntries)
            {
                var tween = itemPlaceEntry.transform.DOMoveY(IngameInventoryPosition.position.y, duration).SetEase(ease);
                tweens.Add(tween);
            }
            return DOTween.Sequence().AppendInterval(0).Join(tweens[0]);
        }

    }
}