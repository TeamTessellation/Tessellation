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
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI multiplierText;

    [Header("Tween Settings")] 
    [Header("Current Score")] 
    [SerializeField] private float currentScorePopUpSize = 1.2f;
    [SerializeField] private float currentScorePopUpDuration = 0.025f;
    [SerializeField] private Ease currentScorePopUpEase = Ease.OutCubic;
    [SerializeField] private float countUpDuration = 0.6f;
    [SerializeField] private Ease countUpEase = Ease.OutCubic;

    [Header("Wait Settings")]
    [SerializeField] private int minWaitMilliseconds = 500;
    [SerializeField] private int confirmWaitMilliseconds = 1000;

    // 시퀀스
    private Sequence _currentScoreSequence;
    private Sequence _multiplierSequence;
    private Sequence _totalScoreSequence;
    private CancellationToken _destroyToken;
    
    // 빠른 연속 호출 시 마지막 요청만 반영하기 위한 토큰
    private CancellationTokenSource _currentScoreCts;
    private CancellationTokenSource _multiplierCts;
    private CancellationTokenSource _totalScoreCts;

    private int _cachedCurrentScore;
    private float _cachedMultiplier = 1f;

    private void Awake()
    {
        _destroyToken = this.GetCancellationTokenOnDestroy();
        ExecEventBus<ScoreManager.TotalScoreChangedEventArgs>.RegisterStatic((int)ExecPriority.UIDefault, OnTotalScoreChanged);
        ExecEventBus<ScoreManager.CurrentScoreChangedEventArgs>.RegisterStatic((int)ExecPriority.UIDefault, OnCurrentScoreChanged);
        ExecEventBus<ScoreManager.MultiplierAddedEventArgs>.RegisterStatic((int)ExecPriority.UIDefault, OnMultiplierAdded);
        
        SetTotalScoreText(0);
        totalScoreText.gameObject.SetActive(true);
        currentScoreText.gameObject.SetActive(false);
        multiplierText.gameObject.SetActive(false);
    }
    
    private void OnDisable()
    {
        _currentScoreSequence?.Kill();
        _currentScoreSequence = null;
        _multiplierSequence?.Kill();
        _multiplierSequence = null;
        _totalScoreSequence?.Kill();
        _totalScoreSequence = null;

        _currentScoreCts?.Cancel();
        _currentScoreCts?.Dispose();
        _currentScoreCts = null;
        _multiplierCts?.Cancel();
        _multiplierCts?.Dispose();
        _multiplierCts = null;
        _totalScoreCts?.Cancel();
        _totalScoreCts?.Dispose();
        _totalScoreCts = null;
    }
    
    private void OnDestroy()
    {
        ExecEventBus<ScoreManager.CurrentScoreChangedEventArgs>.UnregisterStatic(OnCurrentScoreChanged);
        ExecEventBus<ScoreManager.TotalScoreChangedEventArgs>.UnregisterStatic(OnTotalScoreChanged);
        ExecEventBus<ScoreManager.MultiplierAddedEventArgs>.UnregisterStatic(OnMultiplierAdded);
    }
    private void SetCurrentScoreText(int value)
    {
        if (currentScoreText != null)
            currentScoreText.text = $"+{value:N0}";
    }

    private void SetTotalScoreText(int value)
    {
        if (totalScoreText != null)
            totalScoreText.text = $"{value:N0}";
    }

    private void SetMultiplierText(float value)
    {
        if (multiplierText != null)
            multiplierText.text = $"x{value:N0}";
    }

    /// <summary>
    /// 점수가 갱신될 때 호출되는 핸들러.
    /// </summary>
    private async UniTask OnCurrentScoreChanged(ScoreManager.CurrentScoreChangedEventArgs args)
    {
        int currentScore = args.NewCurrentScore;
        _cachedCurrentScore = currentScore;

        totalScoreText.gameObject.SetActive(false);
        currentScoreText.gameObject.SetActive(true);
        
        _currentScoreCts?.Cancel();
        _currentScoreCts?.Dispose();
        _currentScoreCts = new CancellationTokenSource();

        _currentScoreSequence?.Kill();
        _currentScoreSequence = null;

        SetCurrentScoreText(currentScore);
        currentScoreText.transform.localScale = Vector3.one * currentScorePopUpSize;
        
        _currentScoreSequence = DOTween.Sequence();
        _currentScoreSequence.Append(currentScoreText.transform.DOScale(Vector3.one, currentScorePopUpDuration))
            .SetEase(Ease.InCubic);

        try
        {
            await _currentScoreSequence.ToUniTask(cancellationToken: _currentScoreCts.Token);
        }
        catch (OperationCanceledException)
        {
            currentScoreText.transform.localScale = Vector3.one;
        }
    }
    
    /// <summary>
    /// Multiplier가 변경되었을 때 호출되는 핸들러
    /// </summary>
    private async UniTask OnMultiplierAdded(ScoreManager.MultiplierAddedEventArgs args)
    {
        float multiplier = args.NewMultiplier;
        _cachedMultiplier = multiplier;
        
        // Multiplier가 1 초과라면 activate해준다.
        if (multiplier > 1f)
        {
            multiplierText.gameObject.SetActive(true);
            
            _multiplierCts?.Cancel();
            _multiplierCts?.Dispose();
            _multiplierCts = new CancellationTokenSource();

            _multiplierSequence?.Kill();
            _multiplierSequence = null;

            SetMultiplierText(multiplier);
            multiplierText.transform.localScale = Vector3.one * currentScorePopUpSize;
            
            _multiplierSequence = DOTween.Sequence();
            _multiplierSequence.Append(multiplierText.transform.DOScale(Vector3.one, currentScorePopUpDuration))
                .SetEase(currentScorePopUpEase);

            try
            {
                await _multiplierSequence.ToUniTask(cancellationToken: _multiplierCts.Token);
            }
            catch (OperationCanceledException)
            {
                multiplierText.transform.localScale = Vector3.one;
            }
        }
        else
        {
            multiplierText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// TotalScore가 갱신될 떄 호출되는 핸들러
    /// </summary>
    private async UniTask OnTotalScoreChanged(ScoreManager.TotalScoreChangedEventArgs args)
    {
        int totalScore = args.NewTotalScore;
        int previousTotal = args.PrevTotalScore;
        
        _totalScoreCts?.Cancel();
        _totalScoreCts?.Dispose();
        _totalScoreCts = new CancellationTokenSource();
        
        var token = CancellationTokenSource.CreateLinkedTokenSource(_totalScoreCts.Token, _destroyToken).Token;

        try
        {
            // Multiplier가 활성화되어 있다면 제거
            if (multiplierText.gameObject.activeSelf)
            {
                multiplierText.gameObject.SetActive(false);
                _cachedMultiplier = 1f;
                
                // CurrentScore 점수 올리는 애니메이션
                if (currentScoreText.gameObject.activeSelf && _cachedCurrentScore > 0)
                {
                    
                    _currentScoreSequence?.Kill();
                    _currentScoreSequence = DOTween.Sequence();

                    int startScore = _cachedCurrentScore;
                    _currentScoreSequence.Append(DOTween.To(
                        () => startScore,
                        x =>
                        {
                            startScore = x;
                            SetCurrentScoreText(x);
                        },
                        totalScore - previousTotal,
                        countUpDuration
                    ).SetEase(countUpEase));

                    await _currentScoreSequence.ToUniTask(cancellationToken: token);
                }
            }

            // 대기
            await UniTask.Delay(confirmWaitMilliseconds, cancellationToken: token);

            // TotalScore activate하고 CurrentScore와 같이 보여주기
            totalScoreText.gameObject.SetActive(true);
            SetTotalScoreText(previousTotal);
            
            // 대기
            await UniTask.Delay(confirmWaitMilliseconds, cancellationToken: token);

            // CurrentScore 사라지고 TotalScore 갱신
            currentScoreText.gameObject.SetActive(false);
            
            _totalScoreSequence?.Kill();
            _totalScoreSequence = DOTween.Sequence();

            int displayScore = previousTotal;
            _totalScoreSequence.Append(DOTween.To(
                () => displayScore,
                x =>
                {
                    displayScore = x;
                    SetTotalScoreText(x);
                },
                totalScore,
                countUpDuration
            ).SetEase(countUpEase));

            await _totalScoreSequence.ToUniTask(cancellationToken: token);
            
            // 최종 값 갱신
            SetTotalScoreText(totalScore);
            _cachedCurrentScore = 0;
        }
        catch (OperationCanceledException)
        {
            // 취소되면 최종 상태로 즉시 설정
            totalScoreText.gameObject.SetActive(true);
            SetTotalScoreText(totalScore);
            currentScoreText.gameObject.SetActive(false);
            multiplierText.gameObject.SetActive(false);
            _cachedCurrentScore = 0;
            _cachedMultiplier = 1f;
        }
    }
}
