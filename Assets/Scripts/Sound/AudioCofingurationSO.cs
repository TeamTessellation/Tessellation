﻿using UnityEngine;
using UnityEngine.Audio;
 
namespace Sound
{
	 
	/// <summary>
	/// 오디오 설정 저장 클래스
	/// https://github.com/UnityTechnologies/open-project-1/blob/release/UOP1_Project/Assets/Scripts/Audio/AudioData/AudioConfigurationSO.cs
	/// </summary>
	[CreateAssetMenu(fileName = "NewAudioConfig", menuName = "Audio/AudioConfig" )]
	public class AudioConfigurationSO : ScriptableObject
	{
	    public AudioMixerGroup MixerGroup = null;
	    
	    [SerializeField] private PriorityLevel _priorityLevel = PriorityLevel.Standard;
	    

	    [Header("Sound properties")]
	    public bool Mute = false;
	    [Range(0f, 1f)] public float Volume = 1f;
	    [Range(-3f, 3f)] public float Pitch = 1f;
	    [Range(-1f, 1f)] public float PanStereo = 0f;
	    [Range(0f, 1.1f)] public float ReverbZoneMix = 1f;

	    [Header("Spatialization")]
	    [Range(0f, 1f)] public float SpatialBlend = 1f;
	    public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;
	    [Range(0.1f, 5f)] public float MinDistance = 0.1f;
	    [Range(5f, 100f)] public float MaxDistance = 50f;
	    [Range(0, 360)] public int Spread = 0;
	    [Range(0f, 5f)] public float DopplerLevel = 1f;

	    [Header("Ignores")]
	    public bool BypassEffects = false;
	    public bool BypassListenerEffects = false;
	    public bool BypassReverbZones = false;
	    public bool IgnoreListenerVolume = false;
	    public bool IgnoreListenerPause = false;
	    
	    private enum PriorityLevel
	        {
        		Highest = 0,
        		High = 64,
        		Standard = 128,
        		Low = 194,
        		VeryLow = 256,
	        }
	    
	    public void ApplyTo(AudioSource audioSource)
	        {
        		audioSource.outputAudioMixerGroup = this.MixerGroup;
        		audioSource.mute = this.Mute;
        		audioSource.bypassEffects = this.BypassEffects;
        		audioSource.bypassListenerEffects = this.BypassListenerEffects;
        		audioSource.bypassReverbZones = this.BypassReverbZones;
	            audioSource.priority = (int)_priorityLevel;
        		audioSource.volume = this.Volume;
        		audioSource.pitch = this.Pitch;
        		audioSource.panStereo = this.PanStereo;
        		audioSource.spatialBlend = this.SpatialBlend;
        		audioSource.reverbZoneMix = this.ReverbZoneMix;
        		audioSource.dopplerLevel = this.DopplerLevel;
        		audioSource.spread = this.Spread;
        		audioSource.rolloffMode = this.RolloffMode;
        		audioSource.minDistance = this.MinDistance;
        		audioSource.maxDistance = this.MaxDistance;
        		audioSource.ignoreListenerVolume = this.IgnoreListenerVolume;
        		audioSource.ignoreListenerPause = this.IgnoreListenerPause;
	        }
	}

}
