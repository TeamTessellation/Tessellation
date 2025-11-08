using System;
using System.Collections;
using UnityEngine;

namespace Sound
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : MonoBehaviour, IPoolAble
    {
        [SerializeField] private Transform targetTransform;
        
        public bool doStopOnTargetNull = true;
            
        private bool isTargetSet;
        
        // private Poolable poolable;
        private AudioSource _audioSource;
        private void Awake()
        {
            // poolable = GetComponent<Poolable>();
            _audioSource = GetComponent<AudioSource>();
        }

        // private void Start()
        // {
        //     if (audioSource == null)
        //     {
        //         Debug.LogError("AudioSource is not assigned.");
        //         return;
        //     }
        // }

        private void Update()
        {
            // 음악 재생이 멈추고, 루프가 아니면 풀에 반환
            if(!_audioSource.isPlaying && !_audioSource.loop)
            {
                Pool<SoundEmitter>.Return(this);
            }
        }
        public void Resume()
        {
            _audioSource.Play();
        }

        public void Pause()
        {
            _audioSource.Pause();
        }

        public void Stop()
        {
            _audioSource.Stop();
        }
        
        public bool IsPlaying => _audioSource.isPlaying;
        public bool IsLooping => _audioSource.loop;
        public AudioClip CurrentlyPlayingAudio => _audioSource.clip;
        public bool IsPaused => _audioSource.isPlaying == false && _audioSource.time > 0;

        public void PlayAudioClip(AudioClip clip, AudioConfigurationSO setting, bool isLoop, Vector3 pos = default)
        {
            _audioSource.clip = clip;

            _audioSource.transform.position = pos;
            _audioSource.loop = isLoop;
            setting.ApplyTo(_audioSource);
            _audioSource.Play();

            // if (!isLoop)
            // {
            //     StartCoroutine(SoundTimer(clip.length));
            // }
        }
        
        /// <summary>
        /// 간단하게 사운드 재생
        /// TODO: 위에걸로 바꿔야함
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="isLoop"></param>
        public void SimplePlayAudioClip(AudioClip clip, bool isLoop)
        {
            _audioSource.clip = clip;
            _audioSource.loop = isLoop;
            _audioSource.Play();
            if (!isLoop)
            {
                StartCoroutine(SoundTimer(clip.length));
            }
        }
        
        public void SetVolume(float volume)
        {
            _audioSource.volume = volume;
        }
        public void FadeSound(float targetVolume, float duration)
        {
            StartCoroutine(FadeSoundCoroutine(targetVolume, duration));
        }
        public void FadeOutAndStop(float duration)
        {
            StartCoroutine(FadeSoundCoroutine(0,duration, Stop));
        }
        private IEnumerator FadeSoundCoroutine(float targetVolume, float duration, Action onComplete = null)
        {
            float startVolume = _audioSource.volume;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                _audioSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
                yield return null;
            }

            _audioSource.volume = targetVolume;
            onComplete?.Invoke();
        }
        
        private IEnumerator SoundTimer(float time)
        {
            yield return new WaitForSeconds(time);
            Pool<SoundEmitter>.Return(this);
            
        }
        
        
        public void SetTarget(Transform target)
        {
            targetTransform = target;
            isTargetSet = target != null;
        }

        private void LateUpdate()
        {
            if (targetTransform != null)
            {
                // 타겟이 살아있으면 따라감
                transform.position = targetTransform.position;
                transform.rotation = targetTransform.rotation;
            }
            else
            {
                if(doStopOnTargetNull && isTargetSet)
                {
                    // doStopOnTargetNull이 True고 타겟이 죽으면 풀에 반환
                    _audioSource.Stop();
                    Pool<SoundEmitter>.Return(this);
                    isTargetSet = false;
                }
            }
        }

        public void Reset()
        {
            targetTransform = null;
            isTargetSet = false;
            _audioSource = GetComponent<AudioSource>();
            _audioSource.clip = null;
            _audioSource.loop = false;
            _audioSource.volume = 1.0f;
        }
    }
}