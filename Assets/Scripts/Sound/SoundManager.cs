using System;
using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace Sound
{
    public class SoundManager : Singleton<SoundManager>
    {
        public const string MainTitleBgmPath = "bgm/Haunted";
        public override bool IsDontDestroyOnLoad => true;
        
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup masterGroup;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
        
        [SerializeField] private float defaultVolume = 0.75f;
        
        // #if UNITY_EDITOR
        // [Range(0.0001f, 1f)] [SerializeField] private float debugMasterVolume = 0.75f;
        // [Range(0.0001f, 1f)] [SerializeField] private float debugMusicVolume = 0.75f;
        // [Range(0.0001f, 1f)] [SerializeField] private float debugSfxVolume = 0.75f;
        // #endif
        
        private Dictionary<string, AudioResource> _cachedAudioClips = new Dictionary<string, AudioResource>();
        private Dictionary<string, SoundEmitter> _cachedBackgroundSoundEmitters = new Dictionary<string, SoundEmitter>();
        private string _currentPlayingBackgroundMusicPath = string.Empty;

        public float DefaultVolume => defaultVolume;
        public float MinVolume => 0.0001f;
        public float MaxVolume => 1f;
        
        public float MasterVolume => GetMasterVolume();
        public float MusicVolume => GetMusicVolume();
        public float SfxVolume => GetSfxVolume();

        private void Start()
        {
            audioMixer = Resources.Load<AudioMixer>("Audio/MainMixer");
            masterGroup = audioMixer.FindMatchingGroups("Master")[0];
            musicGroup = audioMixer.FindMatchingGroups("Background")[0];
            sfxGroup = audioMixer.FindMatchingGroups("SFX")[0];

            float savedMasterVolume = PlayerPrefs.GetFloat("MasterVolume", defaultVolume);
            float savedMusicVolume = PlayerPrefs.GetFloat("BackgroundVolume", defaultVolume);
            float savedSfxVolume = PlayerPrefs.GetFloat("SFXVolume", defaultVolume);
            
            print($"savedMasterVolume: {savedMasterVolume}, savedMusicVolume: {savedMusicVolume}, savedSfxVolume: {savedSfxVolume}");
            
            SetMasterVolume(savedMasterVolume);
            SetMusicVolume(savedMusicVolume);
            SetSfxVolume(savedSfxVolume);
        // #if UNITY_EDITOR
        //     debugMasterVolume = savedMasterVolume;
        //     debugMusicVolume = savedMusicVolume;
        //     debugSfxVolume = savedSfxVolume;
        // #endif
        }

        private void OnValidate()
        {
            if (audioMixer == null)
            {
                audioMixer = Resources.Load<AudioMixer>("Audio/MainMixer");
                masterGroup = audioMixer.FindMatchingGroups("Master")[0];
                musicGroup = audioMixer.FindMatchingGroups("Background")[0];
                sfxGroup = audioMixer.FindMatchingGroups("SFX")[0];
            }
            // #if UNITY_EDITOR
            // SetMasterVolume(debugMasterVolume);
            // SetMusicVolume(debugMusicVolume);
            // SetSfxVolume(debugSfxVolume); 
            // #endif
            PlayerPrefs.Save();
        }

        private void OnDisable()
        {
            PlayerPrefs.Save();
        }

        private void OnDestroy()
        {
            foreach (var clip in _cachedAudioClips)
            {
                Resources.UnloadAsset(clip.Value);
            }
            _cachedAudioClips.Clear();
            PlayerPrefs.Save();
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="percent"> 0.0001 ~ 1의 값.</param>
        public void SetMasterVolume(float percent)
        {
            var clampedPercent = Mathf.Clamp(percent, 0.0001f, 1f);
            PlayerPrefs.SetFloat("MasterVolume", clampedPercent);
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(clampedPercent) * 20);
// #if UNITY_EDITOR
//             print($"Setting Master Volume: {clampedPercent}");
//             debugMasterVolume = clampedPercent;
// #endif
            PlayerPrefs.Save();
        }
        /// <param name="percent"> 0.0001 ~ 1의 값.</param>
        public void SetMusicVolume(float percent)
        {
            var clampedPercent = Mathf.Clamp(percent, 0.0001f, 1f);
            PlayerPrefs.SetFloat("BackgroundVolume", clampedPercent);
            audioMixer.SetFloat("BackgroundVolume", Mathf.Log10(clampedPercent) * 20);
// #if UNITY_EDITOR
//             debugMusicVolume = clampedPercent;
// #endif
            PlayerPrefs.Save();
        }
        /// <param name="percent"> 0.0001 ~ 1의 값.</param>
        public void SetSfxVolume(float percent)
        {
            var clampedPercent = Mathf.Clamp(percent, 0.0001f, 1f);
            PlayerPrefs.SetFloat("SFXVolume", clampedPercent);
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(clampedPercent) * 20);
// #if UNITY_EDITOR
//             debugSfxVolume = clampedPercent;
// #endif
            PlayerPrefs.Save();
        }
        
        public float GetMasterVolume()
        {
            return PlayerPrefs.GetFloat("MasterVolume", defaultVolume);
        }
        public float GetMusicVolume()
        {
            return PlayerPrefs.GetFloat("BackgroundVolume", defaultVolume);
        }

        public float GetSfxVolume()
        {
            return PlayerPrefs.GetFloat("SFXVolume", defaultVolume);
        }

        private AudioResource GetAudioSource(string name)
        {
            AudioResource resource;
            if (_cachedAudioClips.TryGetValue(name, out  resource))
            {
                return resource;
            }
            resource = Resources.Load<AudioResource>($"Audio/{name}");
            if (resource != null)
            {
                _cachedAudioClips.Add(name, resource);
                return resource;
            }
            Debug.LogError($"Audio clip not found: {name}");
            return null;
        }
        
        public bool IsBackgroundMusicPlaying(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            if (_cachedBackgroundSoundEmitters.TryGetValue(path, out SoundEmitter soundEmitter) && soundEmitter)
            {
                AudioSource audioSource = soundEmitter.GetComponent<AudioSource>();
                return audioSource.isPlaying;
            }
            return false;
        }
        
        public SoundEmitter GetCurrentBackgroundMusicEmitter()
        {
            if (string.IsNullOrEmpty(_currentPlayingBackgroundMusicPath))
                return null;
            if (_cachedBackgroundSoundEmitters.TryGetValue(_currentPlayingBackgroundMusicPath, out SoundEmitter soundEmitter) && soundEmitter)
            {
                return soundEmitter;
            }
            return null;
        }
        
        /// <summary>
        /// 배경음악 재생.
        /// 이미 재생중인 배경음악이 있다면 일시정지한다.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ifPausedResume">이전에 일시정지 되었다면, 이어서 재생</param>
        public SoundEmitter PlayBackgroundMusic(string path, bool ifPausedResume = false)
        {
            AudioResource clip = GetAudioSource(path);
            if (clip == null)
                return null;
            SoundEmitter soundEmitter;
            
            // 이미 재생중인 배경음악이 있다면 일시정지한다.
            if(string.IsNullOrEmpty(_currentPlayingBackgroundMusicPath) == false)
            {
                if (_cachedBackgroundSoundEmitters.TryGetValue(_currentPlayingBackgroundMusicPath, out soundEmitter) && soundEmitter)
                {
                    AudioSource audioSource = soundEmitter.GetComponent<AudioSource>();
                    if (audioSource.isPlaying)
                    {
                        audioSource.Pause();
                    }
                }
            }
            _currentPlayingBackgroundMusicPath = path;
            
            // 배경음악이 이전에 일시정지 된 적 있는지 확인.
            if (_cachedBackgroundSoundEmitters.TryGetValue(path, out soundEmitter) && soundEmitter != null)
            {
                // if (soundEmitter.GetComponent<Poolable>().IsInPool)
                // {
                //     Debug.LogError("[SoundManager] SoundEmitter가 풀에 있음에도 불구하고, _cachedBackgroundSoundEmitters에 존재함. 뭔가 잘못됨");
                // }
                Debug.Log("Checking if sound emitter is playing");
                AudioSource audioSource = soundEmitter.GetComponent<AudioSource>();
                if (soundEmitter.IsPaused && ifPausedResume)
                {
                    audioSource.volume = 1f;
                    audioSource.UnPause();
                }
                else
                {
                    soundEmitter.transform.SetParent(transform);
                    soundEmitter.transform.position = Vector3.zero;
                    audioSource.resource = clip;
                    audioSource.loop = true;
                    audioSource.time = 0;
                    audioSource.volume = 1f;
                    audioSource.Play();
                }
            }
            else
            {
                soundEmitter = Pool<SoundEmitter>.Get();
                soundEmitter.transform.SetParent(transform);
                soundEmitter.transform.position = Vector3.zero;
                AudioSource audioSource = soundEmitter.GetComponent<AudioSource>();
                audioSource.outputAudioMixerGroup = musicGroup;
                audioSource.resource = clip;
                audioSource.loop = true;
                audioSource.volume = 1f;
                audioSource.Play();
                audioSource.spatialize = false;
                
                // soundEmitter.GetComponent<Poolable>().OnReleaseOnce += () => OnBackgroundMusicReleasedOnce(path);
                
                _cachedBackgroundSoundEmitters.Add(path, soundEmitter);
            }
            return soundEmitter;
        }
        
        /// <summary>
        /// 해당 Transform을 따라다니는 사운드 재생
        /// </summary>
        /// <param name="path"></param>
        /// <param name="target"></param>
        /// <param name="stopOnTargetNull"> 해당 타겟 transform이 사라지면 재생 멈춤</param>
        /// <param name="isLoop"></param>
        public void PlaySfxTo(string path, Transform target, bool stopOnTargetNull = true, bool isLoop = false,bool isSpatial = true)
        {
            AudioResource clip = GetAudioSource(path);
            if (clip == null)
                return;
            SoundEmitter soundEmitter = Pool<SoundEmitter>.Get();
            soundEmitter.transform.SetParent(transform);
            soundEmitter.SetTarget(target);
            soundEmitter.doStopOnTargetNull = stopOnTargetNull;
            AudioSource audioSource = soundEmitter.GetComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = sfxGroup;
            audioSource.spatialize = isSpatial;
            
            soundEmitter.SimplePlayAudioSource(clip, isLoop);
        }
        
        /// <summary>
        /// 해당 위치에서 사운드 재생
        /// </summary>
        /// <param name="path"></param>
        /// <param name="position"></param>
        /// <param name="isLoop"></param>
        public void PlaySfxAt(string path, Vector3 position, bool isLoop = false,bool isSpatial = true)
        {
            AudioResource clip = GetAudioSource(path);
            if (clip == null)
                return;
            SoundEmitter soundEmitter = Pool<SoundEmitter>.Get();
            soundEmitter.transform.SetParent(transform);
            soundEmitter.transform.position = position;
            AudioSource audioSource = soundEmitter.GetComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = sfxGroup;
            audioSource.spatialize = isSpatial;
            
            soundEmitter.SimplePlayAudioSource(clip, isLoop);
        }
        
        /// <summary>
        /// 전역 사운드 재생
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isLoop"></param>
        /// <param name="isSpatial"></param>
        public void PlaySfx(string path, bool isLoop = false,bool isSpatial = false)
        {
            AudioResource clip = GetAudioSource(path);
            if (clip == null)
                return;
            SoundEmitter soundEmitter = Pool<SoundEmitter>.Get();
            soundEmitter.transform.SetParent(transform);
            soundEmitter.transform.position = Vector3.zero;
            AudioSource audioSource = soundEmitter.GetComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = sfxGroup;
            audioSource.spatialize = isSpatial;
            
            soundEmitter.SimplePlayAudioSource(clip, isLoop);
        }
        
        public void FadeOutBackgroundMusic(float fadeOutTime)
        {
            if (string.IsNullOrEmpty(_currentPlayingBackgroundMusicPath) == false)
            {
                if (_cachedBackgroundSoundEmitters.TryGetValue(_currentPlayingBackgroundMusicPath, out SoundEmitter soundEmitter) && soundEmitter)
                {
                    soundEmitter.FadeOutAndStop(fadeOutTime);
                    soundEmitter.transform.SetParent(null);
                }
                _currentPlayingBackgroundMusicPath = string.Empty;
            }
        }
        
        public void StopBackgroundMusic()
        {
            if (string.IsNullOrEmpty(_currentPlayingBackgroundMusicPath) == false)
            {
                if (_cachedBackgroundSoundEmitters.TryGetValue(_currentPlayingBackgroundMusicPath,
                        out SoundEmitter soundEmitter) && soundEmitter)
                {
                    AudioSource audioSource = soundEmitter.GetComponent<AudioSource>();
                    audioSource.Stop();
                    soundEmitter.transform.SetParent(null);
                    Pool<SoundEmitter>.Return(soundEmitter);
                }
                _currentPlayingBackgroundMusicPath = string.Empty;
            }
        }
        
        public void StopCleanEmitter(SoundEmitter soundEmitter)
        {
            if (soundEmitter != null)
            {
                soundEmitter.Stop();
                soundEmitter.transform.SetParent(null);
                Pool<SoundEmitter>.Return(soundEmitter);
            }
        }
        
        
        // private void OnBackgroundMusicReleasedOnce(string path)
        // {
        //     if (string.IsNullOrEmpty(path) == false)
        //     {
        //         if (_cachedBackgroundSoundEmitters.ContainsKey(path))
        //         {
        //             _cachedBackgroundSoundEmitters.Remove(path);
        //         }
        //         if (_currentPlayingBackgroundMusicPath == path)
        //         {
        //             _currentPlayingBackgroundMusicPath = string.Empty;
        //         }
        //     }
        // }
    }
}