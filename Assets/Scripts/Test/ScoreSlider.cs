using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ExecEvents;
using Stage;
using UnityEngine;
using UnityEngine.UI;

public class ScoreSlider : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image fillImage;
    
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private Ease animationEase = Ease.OutCubic;

    private float _currentTargetScore;
    private int _currentScore;
    private Sequence _sequence;
    private CancellationTokenSource _inFlightCts;
    private CancellationToken _destroyToken;
    
    private void Awake()
    {
        _destroyToken = this.GetCancellationTokenOnDestroy();
        fillImage.fillAmount = 0f;
    }
    
    private void OnEnable()
    {
        ScoreManager.Instance.OnCurrentScoreChangedAsync += ScoreChange;
        ExecEventBus<StageStartEventArgs>.RegisterDynamic(OnStageStart);
    }

    private void OnDisable()
    {
        ScoreManager.Instance.OnCurrentScoreChangedAsync -= ScoreChange;
        ExecEventBus<StageStartEventArgs>.UnregisterDynamic(OnStageStart);
        
        // 진행 중인 작업 정리
        _inFlightCts?.Cancel();
        _inFlightCts?.Dispose();
        _inFlightCts = null;
        
        _sequence?.Kill();
        _sequence = null;
    }

    private void OnStageStart(ExecQueue<StageStartEventArgs> queue, StageStartEventArgs args)
    {
        _currentTargetScore = args.StageTargetScore;
        
        _currentScore = 0;
        fillImage.fillAmount = 0f;
    }

    private async UniTask ScoreChange(int newScore)
    {
        // 연속 호출 대비: 이전 작업 취소
        _inFlightCts?.Cancel();
        _inFlightCts?.Dispose();
        _inFlightCts = new CancellationTokenSource();

        var linked = CancellationTokenSource.CreateLinkedTokenSource(_destroyToken, _inFlightCts.Token);
        var token = linked.Token;

        try
        {

            if (_currentTargetScore <= 0)
            {
                return;
            }
            Debug.Log($"Fill : {fillImage.fillAmount}");
            // 진행 중인 트윈 종료 (즉시 완료로 간주)
            _sequence?.Kill();
            _sequence = null;

            float currentFillAmount = fillImage.fillAmount;
            float targetFillAmount = Mathf.Clamp01(newScore / _currentTargetScore);

            // Fill Amount 애니메이션 트윈 생성
            _sequence = DOTween.Sequence();
            _sequence.Append(
                DOTween.To(
                        () => fillImage.fillAmount,
                        v => fillImage.fillAmount = v,
                        targetFillAmount,
                        animationDuration
                    )
                    .SetEase(animationEase)
            );

            // 트윈 완료까지 대기
            await _sequence.ToUniTask(cancellationToken: token);
            
            _currentScore = newScore;
        }
        catch (OperationCanceledException)
        {
            // 파괴/연속 호출로 인한 정상 취소
        }
        finally
        {
            linked.Dispose();
        }
    }
}