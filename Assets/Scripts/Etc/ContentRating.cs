using System;
using System.Collections;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 콘텐츠 등급 표시를 위한 컴포넌트입니다.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class ContentRating : MonoBehaviour
{ 
    [SerializeField] Canvas ratingCanvas;
    [SerializeField] Image ratingImage;

    [SerializeField] private float showing = 2f;
    [SerializeField] private float fading = 1f;
    private void Reset()
    {
        ratingCanvas = GetComponent<Canvas>();
        ratingImage = GetComponentInChildren<Image>();
        if (ratingImage == null)
        {
            var go = new GameObject("RatingImage", typeof(Image));
            go.transform.SetParent(transform, false);
            ratingImage = go.GetComponent<Image>();
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        ShowRatingAsync().Forget();
    }

    private async UniTask ShowRatingAsync()
    {
        ratingCanvas.enabled = true;
        Color color = ratingImage.color;
        color.a = 1f;
        ratingImage.color = color;

        var wait = UniTask.WaitForSeconds(showing, ignoreTimeScale: true);
        var isInitialized = UniTask.WaitUntil(() => InitialLoader.Initialized).ContinueWith(() => UniTask.WaitForSeconds(0.5f, ignoreTimeScale: true));
         
        await UniTask.WhenAll(wait, isInitialized);
        
        float elapsed = 0f;
        while (elapsed < fading)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / fading);
            ratingImage.color = color;
            await UniTask.Yield();
        }
        ratingCanvas.enabled = false;
        
        await UniTask.DelayFrame(1);
        Destroy(gameObject);
    }
}
