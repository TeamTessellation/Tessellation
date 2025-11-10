using UnityEngine;

public enum eEffectType
{
    None,
    ScorePopup,
    Max,
}

[CreateAssetMenu(fileName = "NewEffectData", menuName = "GameData/EffectData")]
public class EffectDataSO : ScriptableObject
{
    [Header("기본 정보")] 
    public eEffectType effectType;
    [Tooltip("이펙트 객체")] public GameObject effectPrefab;
    [Tooltip("Trail 객체")] public GameObject effectTrailPrefab;
    [Tooltip("-1으로 설정 시 영구지속")] public float effectDuration;

    [Space(20)] 
    
    [Header("오브젝트 풀링")] 
    public int basePoolSize = 5;

    public int maxPoolSize = 50;

    [Space(20)]
    
    [Header("렌더링")]
    [Tooltip("숫자가 높을수록 위에 위치")]public int sortingOrder;
    [Tooltip("소팅 레이어")] public int sortingLayer;
    
    [Space(20)]
    
    [Header("사운드")]
    [Tooltip("함께 재상할 사운드")]public AudioClip soundEffect;
    [Range(0, 1)] public float soundVolume;
}
