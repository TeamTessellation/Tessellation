using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// TileSetµÈ¿« Group
/// </summary>
[CreateAssetMenu(fileName = "TileEffect", menuName = "HexaSystem/TileEffect", order = 1)]
public class TileEffectSO : ScriptableObject
{
    [Header("ETC Settings")]
    public bool WaitForEnd = false;

    [Header("Light Settings")]
    public Ease LightEase;
    public float LightDuration;
    public Color LightColor;
    public float LightStartPower;
    public float LightMaxPower;
    public float MaxRemainTime;

    [Header("FadeOut Settings")]
    public Ease FadeOutEase;
    public Color FadeOutColor;
    public float FadeOutDuration;
}