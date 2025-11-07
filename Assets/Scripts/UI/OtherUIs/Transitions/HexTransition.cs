using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Machamy.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace UI.OtherUIs.Transitions
{
    public class HexTransition : MonoBehaviour
    {
        private static readonly int TileSizeHash = Shader.PropertyToID("_TileSize"); 
        private static readonly int ProgressHash = Shader.PropertyToID("_Progress");
        private static readonly int DirectoinHash = Shader.PropertyToID("_Direction");
        private static readonly int AngleHash = Shader.PropertyToID("_Angle");
        private static readonly int IntervalHash = Shader.PropertyToID("_Interval");
        private static readonly int XCountHash = Shader.PropertyToID("_XCount");
        private static readonly int StartXHash = Shader.PropertyToID("_StartX");
        private static readonly int EndXHash = Shader.PropertyToID("_EndX");
        
        
        
        private Image _image;
        [SerializeField,VisibleOnly(EditableIn.EditMode)]  float _tileSize = 1;
        [SerializeField,Range(0,1f)] float _progress = 0f;
        [SerializeField] DirectionType _directionType = DirectionType.Down2Up;
        [SerializeField] FadeType _fadeType = FadeType.In;
        [SerializeField,Range(0,3.141592f)] float _angle = 0f;
        [SerializeField,Range(0,2f)] float _interval = 0.1f;
        [SerializeField] Vector2 startConerOffset = Vector2.zero;
        [SerializeField] Vector2 endConerOffset = Vector2.zero;
        [SerializeField,VisibleOnly] float _xCount = 0f;
        
        public float Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                SetProgress(_progress);
            }
        }
        public DirectionType DirectionType
        {
            get => _directionType;
            set
            {
                _directionType = value;
                _image.material.SetFloat(DirectoinHash, _directionType == DirectionType.Down2Up ? 0f : 1f);
            }
        }

        public FadeType FadeType
        {
            get => _fadeType;
            set => _fadeType = value;
        }
        
        public float Angle
        {
            get => _angle;
            set
            {
                _angle = value;
                _image.material.SetFloat(AngleHash, _angle);
            }
        }
        
        public float Interval
        {
            get => _interval;
            set
            {
                _interval = value;
                _image.material.SetFloat(IntervalHash, _interval);
            }
        }
        

        public float TileSize
        {
            get => _tileSize;
            set
            {
                _tileSize = value;
                _image.material.SetFloat(TileSizeHash, _tileSize);
            }
        }
        
        public float XCount
        {
            get => _xCount;
            protected set
            {
                _xCount = value;
                _image.material.SetFloat(XCountHash, _xCount);
            }
        }

        public float StartX
        {
            get => _image.material.GetFloat(StartXHash);
            protected set { _image.material.SetFloat(StartXHash, value); }
        }
        public float EndX
        {
            get => _image.material.GetFloat(EndXHash);
            protected set { _image.material.SetFloat(EndXHash, value); }
        }


        private void Reset()
        {
            _image = GetComponent<Image>();
            _image.material = new Material(_image.material); // 인스턴스 복사
            InitEffect();
            _tileSize = _image.material.GetFloat(TileSizeHash);
            _progress = _image.material.GetFloat(ProgressHash);
            _directionType = DirectionType.Down2Up;
            _angle = _image.material.GetFloat(AngleHash);
            _interval = _image.material.GetFloat(IntervalHash);
            _xCount = _image.material.GetFloat(XCountHash);
            OnValidate();
        }

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.material = new Material(_image.material); // 인스턴스 복사
            InitEffect();
        }

        
        /// <summary>
        /// 정해진 TileSize에 맞춰 이펙트를 초기화합니다. <br/>
        /// XCount, StartX, XEnd 값을 자동으로 설정합니다.
        /// </summary>
        public void InitEffect()
        {
            _tileSize = _image.material.GetFloat(TileSizeHash);
            float dist = _image.transform.position.z - Camera.main.transform.position.z;
            int start = ((Vector2)(Camera.main.ScreenToWorldPoint(new Vector3(0, 0, dist)))).ToCoor(_tileSize).Pos.x;
            int end = ((Vector2)(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, dist))))
                .ToCoor(_tileSize).Pos.x;

            float xCount = end - start + 1;
            XCount = xCount;
            float startX = start;
            float xEnd = end;
            StartX = startX;
            EndX = xEnd;    
            
        }

        public void SetProgress(float progress)
        {
            _image.material.SetFloat(ProgressHash, progress);
        }
        
        public async UniTask PlayHexagonTransition(float duration, FadeType fade,
            DirectionType direction = DirectionType.Down2Up, Ease easeType = Ease.Linear, float angle = 0f)
        {
            gameObject.SetActive(true);
            DirectionType = direction;
            FadeType = fade;
            
            // Dotween 죽이기 
            DOTween.Kill(this);
            
            float elapsed = 0f;
            float from = fade == FadeType.In ? 1f : 0f;
            float to = fade == FadeType.In ? 0f : 1f;
            Progress = from;
            await DOTween.To(() => Progress, x => Progress = x, to, duration)
                .SetEase(easeType)
                .ToUniTask();
            Progress = to;
            
            if (fade == FadeType.Out)
            {
                gameObject.SetActive(false);
            }
            
        }
        
        public async UniTask PlayHexagonTransition(float duration, FadeType fade, AnimationCurve curve,
            DirectionType direction = DirectionType.Down2Up, float angle = 0f)
        {
            gameObject.SetActive(true);
            DirectionType = direction;
            FadeType = fade;
            
            float from = fade == FadeType.In ? 1f : 0f;
            float to = fade == FadeType.In ? 0f : 1f;
            Progress = from;
            await DOTween.To(() => Progress, x => Progress = x, to, duration)
                .SetEase(curve)
                .ToUniTask();
            Progress = to;
            
            if (fade == FadeType.Out)
            {
                gameObject.SetActive(false);
            }
        }
        
        

        private void OnValidate()
        {
            if (_image == null)
            {
                _image = GetComponent<Image>();
            }
            SetProgress(_progress);
            _image.material.SetFloat(DirectoinHash, _directionType == DirectionType.Down2Up ? 0f : 1f);
            _image.material.SetFloat(AngleHash, _angle);
            _image.material.SetFloat(IntervalHash, _interval);
            //_image.material.SetFloat(TileSizeHash, _tileSize);
            if (!Application.isPlaying)
            {
                InitEffect();
            }
        }
    }
}