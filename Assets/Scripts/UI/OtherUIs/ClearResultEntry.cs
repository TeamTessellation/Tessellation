using TMPro;
using UI.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.OtherUIs
{
    public class ClearResultEntry : UIBehaviour
    {
        [field: SerializeField] public TMP_Text Label { get; set; }
        [field: SerializeField] public CounterText ValueText { get; set; }
        
        protected override void Reset()
        {
            base.Reset();
            
            Label = transform.GetChild(0).GetComponent<TMP_Text>();
            ValueText = transform.GetChild(1).GetComponent<CounterText>();
        }
        
        
    }
}