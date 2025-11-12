using System;
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
    [SerializeField] private float countUpDuration = 0.6f;
    [SerializeField] private Ease countUpEase = Ease.OutCubic;

    [Header("Confirm Wait")]
    [SerializeField] private int minWaitMilliseconds = 500;

    private int _displayedScore;           // 현재 UI에 표시 중인 정수 값
    private Sequence _sequence;            // 진행 중인 트윈
    private bool _isConfirmed;             // 외부에서 Confirm() 호출로 true
    private CancellationToken _destroyToken;

    // (선택) 빠른 연속 호출 시 마지막 요청만 반영하기 위한 토큰
    private CancellationTokenSource _inFlightCts;

    private void Awake()
    {
        _destroyToken = this.GetCancellationTokenOnDestroy();
        SetText(_displayedScore);
        ExecEventBus<ScoreManager.CurrentScoreChangedEventArgs>.RegisterStatic((int)ExecPriority.UIDefault, ScoreChange);
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        

        // 정리
        _sequence?.Kill();
        _sequence = null;

        _inFlightCts?.Cancel();
        _inFlightCts?.Dispose();
        _inFlightCts = null;
    }
    
    private void OnDestroy()
    {
        ExecEventBus<ScoreManager.CurrentScoreChangedEventArgs>.UnregisterStatic(ScoreChange);
    }

    /// <summary>
    /// 외부(버튼 등)에서 호출해 “확인됨” 상태로 만들어 대기 해제.
    /// </summary>
    public void Confirm()
    {
        _isConfirmed = true;
    }

    private void SetText(int value)
    {
        if (currentScoreText != null)
            currentScoreText.text = value.ToString("N0");
    }

    /// <summary>
    /// 점수가 갱신될 때 호출되는 핸들러.
    /// </summary>
    private async UniTask ScoreChange(ScoreManager.CurrentScoreChangedEventArgs args)
    {
        int newScore = args.NewCurrentScore;
        // 연속 호출 대비: 이전 작업 취소
        _inFlightCts?.Cancel();
        _inFlightCts?.Dispose();
        _inFlightCts = new CancellationTokenSource();

        var linked = CancellationTokenSource.CreateLinkedTokenSource(_destroyToken, _inFlightCts.Token);
        var token = linked.Token;

        try
        {
            // 진행 중인 트윈 종료 (즉시 완료로 간주)
            _sequence?.Kill();
            _sequence = null;

            int start = _displayedScore;
            _isConfirmed = false; // 새 사이클 시작 시 초기화

            // 카운트업 트윈 생성
            _sequence = DOTween.Sequence();
            _sequence.Append(
                DOTween.To(
                        () => _displayedScore,
                        v => { _displayedScore = v; SetText(v); },
                        newScore,
                        countUpDuration
                    )
                    .SetEase(countUpEase)
            );

            // 트윈 완료까지 대기 (Kill 시에도 Complete로 간주하여 예외 없이 종료)
            await _sequence
                .ToUniTask(cancellationToken: token);

            // 500ms 또는 Confirm 중 더 먼저 오는 것 대기
            await UniTask.WhenAny(
                UniTask.Delay(minWaitMilliseconds, cancellationToken: token),
                UniTask.WaitUntil(() => _isConfirmed, cancellationToken: token)
            );
        }
        catch (OperationCanceledException)
        {
            // 파괴/연속 호출로 인한 정상 취소
        }
        finally
        {
            linked.Cancel();
            linked.Dispose();
        }
    }
}
