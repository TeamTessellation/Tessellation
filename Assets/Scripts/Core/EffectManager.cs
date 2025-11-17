using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 게임 내 실행되는 여러 이펙트들을 실행시키는 클래스
/// 이펙트들은 오브젝트 풀링을 사용한다
/// </summary>
public class EffectManager : Singleton<EffectManager>
{
    // === Property ===
    public override bool IsDontDestroyOnLoad => true;

    [SerializeField] private Transform effectContainer; // Pool 담을 것
    
    private Dictionary<eEffectType, EffectDataSO> _effectDatabase;
    private Dictionary<eEffectType, IObjectPool<GameObject>> _effectPool;

    private Dictionary<GameObject, eEffectType> _activeEffects;

    protected override void AfterAwake()
    {
        base.AfterAwake();
        InitializeDatabase();
        InitializePool();
    }

    private void InitializeDatabase()
    {
        // a. SO 담기
        EffectDataSO[] effects = Resources.LoadAll<EffectDataSO>("Effects");
        
        // b. Dictionary에 추가
        _effectDatabase = new Dictionary<eEffectType, EffectDataSO>();
        _activeEffects = new Dictionary<GameObject, eEffectType>();
        foreach (var effectData in effects)
        {
            _effectDatabase.Add(effectData.effectType, effectData);
        }
    }

    private void InitializePool()
    {
        _effectPool = new Dictionary<eEffectType, IObjectPool<GameObject>>();
        
        // a. 기본 개수만큼 풀 초기화
        foreach (var effectDataPair in _effectDatabase)
        {
            eEffectType effectType = effectDataPair.Key;
            EffectDataSO effectData = effectDataPair.Value;
            
            // ObjectPool 초기화
            ObjectPool<GameObject> tmpEffectPool = 
                new ObjectPool<GameObject>(createFunc:()=>Instantiate(effectData.effectPrefab, effectContainer),
                    ActionOnGet, ActionOnRelease, null, true, effectData.basePoolSize,
                    effectData.maxPoolSize);

            // Pool 기본 개수만큼 생성 & 해제 
            List<GameObject> tmp = new List<GameObject>();
            for (int i = 0; i < effectData.basePoolSize; i++)
            {
                tmp.Add(tmpEffectPool.Get());
            }
            for (int i = 0; i < effectData.basePoolSize; i++)
            {
                tmpEffectPool.Release(tmp[i]);
            }
            
            _effectPool.Add(effectType, tmpEffectPool);
        }
    }

    private void ActionOnGet(GameObject newPoolObject)
    {
        newPoolObject.SetActive(true);
    }

    private void ActionOnRelease(GameObject releasePoolObject)
    {
        releasePoolObject.SetActive(false);
    }

    public void ShowScoreEffect(int score, Vector2 pos)
    {
        Debug.Log("Kuxi");
        
        GameObject effectObj = _effectPool[eEffectType.ScorePopup].Get();
        _activeEffects.Add(effectObj, eEffectType.ScorePopup);

        ScoreEffect scoreEffect = effectObj.GetComponent<ScoreEffect>();
        scoreEffect.Initialize(score, pos, OnScoreEffectComplete);
    }

    private void OnScoreEffectComplete(ScoreEffect effect)
    {
        GameObject effectObj = effect.gameObject;

        if (_activeEffects.ContainsKey(effectObj))
        {
            eEffectType effectType = _activeEffects[effectObj];
            _activeEffects.Remove(effectObj);
            
            effect.ResetState();
            _effectPool[effectType].Release(effectObj);
        }
    }
}
