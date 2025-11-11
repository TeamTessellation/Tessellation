using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI.OtherUIs
{
    public class ClearResultUI : UIBase
    {
        [Header("Clear Result UI Components")]
        [SerializeField] private CanvasGroup clearCanvasGroup;
        [SerializeField] private TMP_Text clearTitleText;
        
        
        [SerializeField] List<ClearResultEntry> entries = new List<ClearResultEntry>();
        [SerializeField] ClearResultEntry totalEntry;
        
        
        public async UniTask ShowClearResultsAsync(CancellationToken cancellationToken)
        {
            gameObject.SetActive(true);
            clearCanvasGroup.alpha = 0f;

            
        }
    }
}