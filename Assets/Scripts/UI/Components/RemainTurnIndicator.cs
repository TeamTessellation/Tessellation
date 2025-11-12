using Cysharp.Threading.Tasks;
using ExecEvents;
using Stage;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Components
{
    public class RemainTurnIndicator : UIBehaviour
    {
        [SerializeField] GameObject turnIndicatorHoldPrefab;
        
        protected override void Awake()
        {
            base.Awake();
            
        }
        
        [SerializeField] private int _maxTurns;
        [SerializeField] private int _remainingTurns;
        
        public int MaxTurns
        {
            get => _maxTurns;
            set
            {
                _maxTurns = value;
                
            }
        }
        public int RemainingTurns
        {
            get => _remainingTurns;
            set
            {
                _remainingTurns = value;
                
            }
        }
        
        public void UpdateVisuals()
        {
            if(transform.childCount < _maxTurns)
            {
                for (int i = transform.childCount; i < _maxTurns; i++)
                {
                    Instantiate(turnIndicatorHoldPrefab, transform);
                }
            }
            else if (transform.childCount > _maxTurns)
            {
                for (int i = transform.childCount - 1; i >= _maxTurns; i--)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
            }
            
            for (int i = 0; i < transform.childCount; i++)
            {
                if (i < _remainingTurns)
                {
                    transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
                }
            }
        }
        
        #if UNITY_EDITOR
        protected void UpdateVisualsEditor()
        {
            if (transform.childCount < _maxTurns)
            {
                for (int i = transform.childCount; i < _maxTurns; i++)
                {
                    Instantiate(turnIndicatorHoldPrefab, transform);
                }
            }else if (transform.childCount > _maxTurns)
            {
                for (int i = transform.childCount - 1; i >= _maxTurns; i--)
                {
                    //delaycall
                    GameObject toDelete = transform.GetChild(i).gameObject;
                    EditorApplication.delayCall += () =>
                    {
                        if (Application.isPlaying)
                        {
                            Destroy(toDelete);
                        }
                        else
                        {
                            DestroyImmediate(toDelete);
                        }
                    };
                }
            }
        }
        #endif


        protected override void Start()
        {
            base.Start();
            UpdateVisuals();
            ExecEventBus<TurnStartEventArgs>.RegisterStatic((int)ExecPriority.UIDefault, OnTurnStart);
            ExecEventBus<StageStartEventArgs>.RegisterStatic((int)ExecPriority.UIDefault, OnStageStart);
            
        }

        public UniTask OnStageStart(StageStartEventArgs args)
        {
            _maxTurns = args.StageModel.StageTurnLimit;
            _remainingTurns = _maxTurns;
            
            UpdateVisuals();
            return UniTask.CompletedTask;
        }

        public UniTask OnTurnStart(TurnStartEventArgs args)
        {
            _maxTurns = args.MaxTurnCount;
            _remainingTurns = _maxTurns - args.CurrentTurnCount;
            
            UpdateVisuals();
            return UniTask.CompletedTask;
        }
  

        protected override void OnValidate()
        {
            base.OnValidate();
            if(_maxTurns < 0) _maxTurns = 0;
            if(_remainingTurns < 0) _remainingTurns = 0;
            if(_remainingTurns > _maxTurns) _remainingTurns = _maxTurns;
            #if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateVisualsEditor();
            else
            {
                UpdateVisuals();
            }
            #else
            UpdateVisuals();
            #endif
        }
    }
}