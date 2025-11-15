using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ExecEvents;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScoreUI : UIBehaviour
{
    [Header("Refs")]
    [SerializeField] private TextMeshProUGUI currentScoreText;

    [Header("Tween Settings")] 
    [Header("Current Score")] 
    [SerializeField] private float currentScorePopUpSize = 1.2f;
    [SerializeField] private float currentScorePopUpDuration = 0.025f;
    [SerializeField] private Ease currentScorePopUpEase = Ease.OutCubic;
    [SerializeField] private float countUpDuration = 0.6f;
    [SerializeField] private Ease countUpEase = Ease.OutCubic;

    [Header("Confirm Wait")]
    [SerializeField] private int minWaitMilliseconds = 500;

    private Sequence _sequence;            // 진행 중인 트윈
    private CancellationToken _destroyToken;

    // (선택) 빠른 연속 호출 시 마지막 요청만 반영하기 위한 토큰
    private CancellationTokenSource _inFlightCts;

    private void Awake()
    {
        _destroyToken = this.GetCancellationTokenOnDestroy();
        SetText(0);
        ExecEventBus<ScoreManager.CurrentScoreChangedEventArgs>.RegisterStatic((int)ExecPriority.UIDefault, OnCurrentScoreChanged);
        ExecEventBus<ScoreManager.TotalScoreChangedEventArgs>.RegisterStatic((int)ExecPriority.UIDefault, OnTotalScoreChanged);
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        _sequence?.Kill();
        _sequence = null;

        _inFlightCts?.Cancel();
        _inFlightCts?.Dispose();
        _inFlightCts = null;
    }
    
    private void OnDestroy()
    {
        ExecEventBus<ScoreManager.CurrentScoreChangedEventArgs>.UnregisterStatic(OnCurrentScoreChanged);
        ExecEventBus<ScoreManager.TotalScoreChangedEventArgs>.UnregisterStatic(OnTotalScoreChanged);
    }
    private void SetText(int value)
    {
        if (currentScoreText != null)
            currentScoreText.text = $"+{value:N0}";
    }

    /// <summary>
    /// 점수가 갱신될 때 호출되는 핸들러.
    /// </summary>
    private async UniTask OnCurrentScoreChanged(ScoreManager.CurrentScoreChangedEventArgs args)
    {
        int currentScore = args.NewCurrentScore;
        
        _inFlightCts?.Cancel();
        _inFlightCts?.Dispose();
        _inFlightCts = new CancellationTokenSource();

        _sequence.Kill();
        _sequence = null;

        SetText(currentScore);
        currentScoreText.transform.localScale = Vector3.one * currentScorePopUpSize;
        
        _sequence = DOTween.Sequence();
        _sequence.Append(currentScoreText.transform.DOScale(Vector3.one, currentScorePopUpDuration))
            .SetEase(Ease.InCubic);

        try
        {
            await _sequence.ToUniTask(cancellationToken: _inFlightCts.Token);
        }
        catch (OperationCanceledException)
        {

        }
    }
    
    // Multiplier가 추가되었을 때 호출되는 함수
    private async UniTask OnMultiplierAdded(ScoreManager.MultiplierAddedEventArgs args)
    {
        float multiplier = args.NewMultiplier;
    }

    /// <summary>
    /// TotalScore가 갱신될 떄 호출되는 핸들러
    /// </summary>
    private async UniTask OnTotalScoreChanged(ScoreManager.TotalScoreChangedEventArgs args)
    {
        int totalScore = args.NewTotalScore;
    }
}
