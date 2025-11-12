using Player;
using TMPro;
using UI.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.OtherUIs
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ClearResultEntry : UIBehaviour
    {
        [Header("Clear Result Entry Components")]
        [field: SerializeField] public TMP_Text Label { get; set; }
        [field: SerializeField] public CounterText ValueText { get; set; }
        [field: SerializeField] public CanvasGroup CanvasGroup { get; set; }
        [Header("Settings")]
        [field: SerializeField] public PlayerStatus.VariableKey VariableKey { get; set; }
        protected override void Reset()
        {
            base.Reset();
            
            Label = transform.GetChild(0).GetComponent<TMP_Text>();
            ValueText = transform.GetChild(1).GetComponent<CounterText>();
            CanvasGroup = GetComponent<CanvasGroup>();
        }
        
        
        
    }
}