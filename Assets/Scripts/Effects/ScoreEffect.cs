using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Sequence = Unity.VisualScripting.Sequence;

public class ScoreEffect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _numberText;
    [SerializeField] private CanvasGroup _canvasGroup;

    private RectTransform _rectTransform;
    private Action<ScoreEffect> _onComplete;
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Initialize(int score, Vector2 pos, Action<ScoreEffect> onComplete = null)
    {
        _onComplete = onComplete;
        ShowScoreAsync(score, pos).Forget();
    }

    private async UniTask ShowScoreAsync(int score, Vector2 pos)
    {
        _numberText.text = $"+{score}";

        Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
        _rectTransform.position = screenPos;

        transform.localScale = Vector3.one * 0.6f;
        _canvasGroup.alpha = 1f;
        gameObject.SetActive(true);

        DG.Tweening.Sequence sequence = DOTween.Sequence();
        
        // 0.2초동안 스케일 올리기
        sequence.Append(transform.DOScale(1f, 0.2f)).SetEase(Ease.OutBack);
        
        // 0.3초동안 페이드아웃 및 위로 이동
        sequence.Append(_canvasGroup.DOFade(0f, 0.3f)).SetEase(Ease.InQuad);
        sequence.Join(transform.DOMoveY(transform.position.y + 3f, 0.3f)).SetEase(Ease.OutQuad);

        await sequence.AsyncWaitForCompletion();
        
        _onComplete?.Invoke(this);
    }

    public void ResetState()
    {
        transform.localScale = Vector3.one * 0.6f;
        _canvasGroup.alpha = 1f;

        DOTween.Kill(transform);
        DOTween.Kill(_canvasGroup);
    }

    private void OnDisable()
    {
        DOTween.Kill(transform);
        DOTween.Kill(_canvasGroup);
    }
}
