using UnityEngine;

public enum eEffectType
{
    None,
    Max,
}

[CreateAssetMenu(fileName = "NewEffectData", menuName = "EffectData")]
public class EffectDataSO : ScriptableObject
{
    [Header("기본 정보")] 
    public eEffectType effectType;
    public GameObject effectPrefab;
    [Tooltip("-1으로 설정 시 영구지속")] public float effectDuration;

    [Space(20)] 
    
    [Header("오브젝트 풀링")] 
    public int basePoolSize = 5;

    [Space(20)]
    
    [Header("렌더링")]
    public int sortingOrder;
    
    [Space(20)]
    
    [Header("사운드")]
    public AudioClip soundEffect;
    [Range(0, 1)] public float soundVolume;

}
