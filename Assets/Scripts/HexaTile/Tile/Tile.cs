using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System;

[PoolSize(20)]
public class Tile : MonoBehaviour, IPoolAble<TileData>
{
    public SerializedDictionary<TileOption, TileEffectSO> TileEffectSO;
    public List<TileEffectSO> TileEffectSOList;

    /// <summary>
    /// 맵 절대 타일 좌표
    /// </summary>
    public Coordinate Coor;
    /// <summary>
    /// 원래 속했던 TileSet
    /// </summary>
    public TileSet Group;
    /// <summary>
    /// 속한 Cell
    /// </summary>
    public Cell Owner;
    public bool IsPlace => Owner != null;
    public Direction Direction;
    private Sprite _defaultSprite;
    private Light2D _light;
    [NonSerialized] private TileOptionBase _tileOptionBase;

    /// <summary>
    /// 해당 Tile Data
    /// </summary>
    public TileData Data { get; private set; }

    public SpriteRenderer Sr { get { return _sr; } private set { _sr = value; } }
    private SpriteRenderer _sr;

    public void ChangeSprite(Sprite sprite)
    {
        _sr.sprite = sprite;
    }

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _light = GetComponent<Light2D>();

        for (int i = 0; i < (int)TileOption.End; i++)
        {
            if (TileEffectSOList.Count > i)
                TileEffectSO[(TileOption)i] = TileEffectSOList[i];
            else
                TileEffectSO[(TileOption)i] = TileEffectSOList[0];
        }
    }

    public void Set(TileData data)
    {
        _defaultSprite = _sr.sprite;
        Data = data;
        gameObject.transform.localScale = Vector3.one * data.Scale;

        _light.intensity = 0;
        _light.color = TileEffectSO[data.Option].LightColor;

        _sr.color = new Color(1, 1, 1, 1);

        switch(data.Option)
        {
            case TileOption.Default:
                _tileOptionBase = new TileOptionDefault();
                break;
            // TO다산 여기다가 각 경우에 따라서 추가 사실 클래스 매번 생성할 필요는 없고 같은거 써도 됨 알아서 수정
        }
    }

    public void Reset()
    {
        _sr.sprite = _defaultSprite;
    }

    public async UniTask ActiveEffect(Action endAction, Action<Tile> remainAction)
    {
        _endAction = endAction;
        TileEffectSO effectData;
        if (TileEffectSO.ContainsKey(Data.Option))
            effectData = TileEffectSO[Data.Option];
        else
            effectData = TileEffectSO[TileOption.Default];

        DOTween.Kill(this);

        float progress = effectData.LightStartPower;
        await DOTween.To(() => progress, x => { SetLight(x); progress = x; }, effectData.LightMaxPower, effectData.LightDuration)
            .SetEase(effectData.LightEase)
            .ToUniTask();

        _light.intensity = effectData.LightMaxPower;
        await UniTask.WaitForSeconds(effectData.MaxRemainTime);

        if (effectData.WaitForEnd)
        {
            remainAction?.Invoke(this);
        }
        else
        {
            await RemoveEffect();
        }
    }

    private Action _endAction = null;

    public async UniTask RemoveEffect()
    {
        TileEffectSO effectData;
        if (TileEffectSO.ContainsKey(Data.Option))
            effectData = TileEffectSO[Data.Option];
        else
            effectData = TileEffectSO[TileOption.Default];

        DOTween.Kill(this);

        _light.intensity = 0;

        _sr.color = effectData.FadeOutColor;
        float progress = 1;
        await DOTween.To(() => progress, x => { _sr.color = new Color(_sr.color.r, _sr.color.g, _sr.color.b, x); progress = x; }, 0, effectData.FadeOutDuration)
            .SetEase(effectData.FadeOutEase)
            .ToUniTask();

        _sr.color = new Color(_sr.color.r, _sr.color.g, _sr.color.b, 0);
        _endAction?.Invoke();
    }

    private void SetLight(float light)
    {
        _light.intensity = light;
    }
}
