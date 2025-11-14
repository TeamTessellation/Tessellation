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
        [SerializeField] private bool useMixerVolume = true; // AudioMixer 사용 여부
        
        private Dictionary<string, AudioResource> _cachedAudioClips = new Dictionary<string, AudioResource>();
        private Dictionary<string, SoundEmitter> _cachedBackgroundSoundEmitters = new Dictionary<string, SoundEmitter>();
        private string _currentPlayingBackgroundMusicPath = string.Empty;
        
        // 모든 활성 SoundEmitter 추적
        private List<SoundEmitter> _activeSfxEmitters = new List<SoundEmitter>();
        
        // AudioSource 직접 제어용 볼륨 저장
        private float _directMasterVolume = 0.75f;
        private float _directMusicVolume = 0.75f;
        private float _directSfxVolume = 0.75f;

        protected override void AfterAwake()
        {
            base.AfterAwake();
            #if UNITY_WEBGL
            // 웹 빌드에서는 AudioMixer 사용 안함
            useMixerVolume = false;
            #endif
        }

        private void Start()
        {
            InitializeAudioMixer();

            float savedMasterVolume = PlayerPrefs.GetFloat("MasterVolume", defaultVolume);
            float savedMusicVolume = PlayerPrefs.GetFloat("BackgroundVolume", defaultVolume);
            float savedSfxVolume = PlayerPrefs.GetFloat("SFXVolume", defaultVolume);
            
            // direct 변수들을 저장된 값으로 초기화
            _directMasterVolume = savedMasterVolume;
            _directMusicVolume = savedMusicVolume;
            _directSfxVolume = savedSfxVolume;
            
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

        private void InitializeAudioMixer()
        {
            if (audioMixer == null)
            {
                Debug.Log("[SoundManager] Loading AudioMixer from Resources...");
                audioMixer = Resources.Load<AudioMixer>("Audio/MainMixer");
                if (audioMixer == null)
                {
                    Debug.LogError("[SoundManager] Failed to load AudioMixer from Resources/Audio/MainMixer");
                    return;
                }
            }

            bool printLog = false;
            if (masterGroup == null)
            {
                masterGroup = audioMixer.FindMatchingGroups("Master")[0];
                printLog = true;
            }
            if (musicGroup == null)
            {
                musicGroup = audioMixer.FindMatchingGroups("Background")[0];
                printLog = true;
            }
            if (sfxGroup == null)
            {
                sfxGroup = audioMixer.FindMatchingGroups("SFX")[0];
                printLog = true;
            }
                
            if (printLog)
                Debug.Log($"[SoundManager] AudioMixer initialized. Master: {masterGroup != null}, Music: {musicGroup != null}, SFX: {sfxGroup != null}");
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
            InitializeAudioMixer(); // audioMixer가 null이면 로드
            
            if (audioMixer == null)
            {
                Debug.LogError("[SoundManager] Cannot set master volume: audioMixer is null");
                return;
            }
            
            var clampedPercent = Mathf.Clamp(percent, 0.0001f, 1f);
            PlayerPrefs.SetFloat("MasterVolume", clampedPercent);
            if (useMixerVolume)
            {
                audioMixer.SetFloat("MasterVolume", Mathf.Log10(clampedPercent) * 20);
            }
            else
            {
                _directMasterVolume = clampedPercent;
                // 이미 재생 중인 모든 오디오의 볼륨 업데이트
                UpdateAllActiveAudioVolumes();
            }
            
// #if UNITY_EDITOR
//             print($"Setting Master Volume: {clampedPercent}");
//             debugMasterVolume = clampedPercent;
// #endif
            PlayerPrefs.Save();
        }
        /// <param name="percent"> 0.0001 ~ 1의 값.</param>
        public void SetMusicVolume(float percent)
        {
            InitializeAudioMixer(); // audioMixer가 null이면 로드
            
            if (audioMixer == null)
            {
                Debug.LogError("[SoundManager] Cannot set music volume: audioMixer is null");
                return;
            }
            
            var clampedPercent = Mathf.Clamp(percent, 0.0001f, 1f);
            PlayerPrefs.SetFloat("BackgroundVolume", clampedPercent);
            
            if (useMixerVolume)
            {
                audioMixer.SetFloat("BackgroundVolume", Mathf.Log10(clampedPercent) * 20);
            }
            else
            {
                _directMusicVolume = clampedPercent;
                // 이미 재생 중인 배경음악의 볼륨 업데이트
                UpdateBackgroundMusicVolumes();
            }
// #if UNITY_EDITOR
//             debugMusicVolume = clampedPercent;
// #endif
            PlayerPrefs.Save();
        }
        /// <param name="percent"> 0.0001 ~ 1의 값.</param>
        public void SetSfxVolume(float percent)
        {
            InitializeAudioMixer(); // audioMixer가 null이면 로드
            
            if (audioMixer == null)
            {
                Debug.LogError("[SoundManager] Cannot set SFX volume: audioMixer is null");
                return;
            }
            
            var clampedPercent = Mathf.Clamp(percent, 0.0001f, 1f);
            PlayerPrefs.SetFloat("SFXVolume", clampedPercent);
            
            if (useMixerVolume)
            {
                audioMixer.SetFloat("SFXVolume", Mathf.Log10(clampedPercent) * 20);
            }
            else
            {
                _directSfxVolume = clampedPercent;
                // 이미 재생 중인 SFX의 볼륨 업데이트는 UpdateAllActiveAudioVolumes에서 처리
                UpdateAllActiveAudioVolumes();
            }
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
            if (_cachedAudioClips.TryGetValue(name, out resource))
            {
                return resource;
            }
            
            // 웹 빌드 디버깅용 로그
            Debug.Log($"[SoundManager] Loading audio: Audio/{name}");
            
            resource = Resources.Load<AudioResource>($"Audio/{name}");
            if (resource != null)
            {
                _cachedAudioClips.Add(name, resource);
                Debug.Log($"[SoundManager] Audio loaded successfully: {name}");
                return resource;
            }
            
            Debug.LogError($"[SoundManager] Audio clip not found: Audio/{name}");
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
                Debug.Log("Checking if sound emitter is playing");
                AudioSource audioSource = soundEmitter.GetComponent<AudioSource>();
                if (soundEmitter.IsPaused && ifPausedResume)
                {
                    audioSource.volume = useMixerVolume ? 1f : (_directMasterVolume * _directMusicVolume);
                    audioSource.UnPause();
                }
                else
                {
                    soundEmitter.transform.SetParent(transform);
                    soundEmitter.transform.position = Vector3.zero;
                    audioSource.resource = clip;
                    audioSource.loop = true;
                    audioSource.volume = useMixerVolume ? 1f : (_directMasterVolume * _directMusicVolume);
                    audioSource.Play();
                }
            }
            else
            {
                soundEmitter = Pool<SoundEmitter>.Get();
                soundEmitter.transform.SetParent(transform);
                soundEmitter.transform.position = Vector3.zero;
                AudioSource audioSource = soundEmitter.GetComponent<AudioSource>();
                
                if (useMixerVolume)
                {
                    audioSource.outputAudioMixerGroup = musicGroup;
                    audioSource.volume = 1f;
                }
                else
                {
                    audioSource.outputAudioMixerGroup = null;
                    audioSource.volume = _directMasterVolume * _directMusicVolume;
                }
                
                audioSource.resource = clip;
                audioSource.loop = true;
                audioSource.Play();
                audioSource.spatialize = false;

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
            {
                Debug.LogWarning($"[SoundManager] PlaySfxTo failed: clip is null for path: {path}");
                return;
            }
            
            SoundEmitter soundEmitter = Pool<SoundEmitter>.Get();
            soundEmitter.transform.SetParent(transform);
            soundEmitter.SetTarget(target);
            soundEmitter.doStopOnTargetNull = stopOnTargetNull;
            AudioSource audioSource = soundEmitter.GetComponent<AudioSource>();
            
            if (useMixerVolume)
            {
                if (sfxGroup != null)
                {
                    audioSource.outputAudioMixerGroup = sfxGroup;
                }
                else
                {
                    Debug.LogWarning("[SoundManager] sfxGroup is null! Audio may not play correctly.");
                }
                audioSource.volume = 1f;
            }
            else
            {
                audioSource.outputAudioMixerGroup = null;
                audioSource.volume = _directMasterVolume * _directSfxVolume;
            }
            
            // List에 추가
            _activeSfxEmitters.Add(soundEmitter);
            
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
            {
                Debug.LogWarning($"[SoundManager] PlaySfxAt failed: clip is null for path: {path}");
                return;
            }
            
            SoundEmitter soundEmitter = Pool<SoundEmitter>.Get();
            soundEmitter.transform.SetParent(transform);
            soundEmitter.transform.position = position;
            AudioSource audioSource = soundEmitter.GetComponent<AudioSource>();
            
            if (useMixerVolume)
            {
                if (sfxGroup != null)
                {
                    audioSource.outputAudioMixerGroup = sfxGroup;
                }
                else
                {
                    Debug.LogWarning("[SoundManager] sfxGroup is null! Audio may not play correctly.");
                }
                audioSource.volume = 1f;
            }
            else
            {
                audioSource.outputAudioMixerGroup = null;
                audioSource.volume = _directMasterVolume * _directSfxVolume;
            }
            
            // List에 추가
            _activeSfxEmitters.Add(soundEmitter);
            
            audioSource.spatialize = isSpatial;
            soundEmitter.SimplePlayAudioSource(clip, isLoop);
        }
        
        /// <summary>
        /// 전역 사운드 재생
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isLoop"></param>
        /// <param name="isSpatial"></param>
        public SoundEmitter PlaySfx(string path, bool isLoop = false,bool isSpatial = false, float pitch = 1f)
        {
            AudioResource clip = GetAudioSource(path);
            if (clip == null)
            {
                Debug.LogWarning($"[SoundManager] PlaySfx failed: clip is null for path: {path}");
                return null;
            }
            
            SoundEmitter soundEmitter = Pool<SoundEmitter>.Get();
            soundEmitter.transform.SetParent(transform);
            soundEmitter.transform.position = Vector3.zero;
            AudioSource audioSource = soundEmitter.GetComponent<AudioSource>();
            
            if (useMixerVolume)
            {
                if (sfxGroup != null)
                {
                    audioSource.outputAudioMixerGroup = sfxGroup;
                }
                else
                {
                    Debug.LogWarning("[SoundManager] sfxGroup is null! Audio may not play correctly.");
                }
                audioSource.volume = 1f;
            }
            else
            {
                audioSource.outputAudioMixerGroup = null;
                audioSource.volume = _directMasterVolume * _directSfxVolume;
            }
            
            // List에 추가
            _activeSfxEmitters.Add(soundEmitter);
            
            audioSource.spatialize = isSpatial;
            audioSource.pitch = pitch;
            
            soundEmitter.SimplePlayAudioSource(clip, isLoop);
            return soundEmitter;
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
        
        
        /// <summary>
        /// 이미 재생 중인 배경음악의 볼륨을 업데이트합니다.
        /// </summary>
        private void UpdateBackgroundMusicVolumes()
        {
            if (useMixerVolume) return;
            
            foreach (var kvp in _cachedBackgroundSoundEmitters)
            {
                if (kvp.Value != null)
                {
                    AudioSource audioSource = kvp.Value.GetComponent<AudioSource>();
                    if (audioSource != null && audioSource.isPlaying)
                    {
                        audioSource.volume = _directMasterVolume * _directMusicVolume;
                    }
                }
            }
        }
        
        /// <summary>
        /// 이미 재생 중인 모든 오디오의 볼륨을 업데이트합니다.
        /// (배경음악 + SFX 모두)
        /// </summary>
        private void UpdateAllActiveAudioVolumes()
        {
            if (useMixerVolume) return;
            
            // 배경음악 업데이트
            UpdateBackgroundMusicVolumes();
            
            // SFX 업데이트 - List를 사용하여 효율적으로 처리
            CleanupInactiveSfxEmitters(); // 재생 끝난 emitter 제거
            
            foreach (var emitter in _activeSfxEmitters)
            {
                if (emitter != null)
                {
                    AudioSource audioSource = emitter.GetComponent<AudioSource>();
                    if (audioSource != null && audioSource.isPlaying)
                    {
                        audioSource.volume = _directMasterVolume * _directSfxVolume;
                    }
                }
            }
        }
        
        /// <summary>
        /// 재생이 끝났거나 null인 SoundEmitter를 리스트에서 제거합니다.
        /// </summary>
        private void CleanupInactiveSfxEmitters()
        {
            _activeSfxEmitters.RemoveAll(emitter => 
            {
                if (emitter == null) return true;
                AudioSource audioSource = emitter.GetComponent<AudioSource>();
                return audioSource == null || !audioSource.isPlaying;
            });
        }
        
        /// <summary>
        /// Update에서 주기적으로 리스트를 정리합니다.
        /// </summary>
        private void Update()
        {
            // 5초마다 한 번씩 정리 (프레임마다 하면 비효율적)
            if (Time.frameCount % 300 == 0) // 60fps 기준 5초
            {
                CleanupInactiveSfxEmitters();
            }
        }
    }
}
