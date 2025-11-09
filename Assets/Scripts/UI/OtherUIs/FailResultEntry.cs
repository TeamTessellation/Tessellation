using Player;
using UI.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.OtherUIs
{
    public class FailResultEntry : UIBehaviour
    {
        [SerializeField] private FailResultUI failResultUI;
        [SerializeField] private CounterText failCountText;
 
        [SerializeField] private PlayerStatus.VariableKey variableKey;

        public enum Direction
        {
            Left,Right
        }

        [field:SerializeField] public Direction MoveDirection { get; private set; }
        [SerializeField] public Vector2 MoveVector => MoveDirection == Direction.Left ? Vector2.left : Vector2.right;
        
        public FailResultUI FailResultUI => failResultUI;
        public CounterText FailCountText => failCountText;
        
        public string VariableKey => variableKey.ToString();
        
        protected override void Reset()
        {
            base.Reset();
            failResultUI = GetComponentInParent<FailResultUI>();
            failCountText = GetComponentInChildren<CounterText>();
        }

        protected override void Awake()
        {
            base.Awake();
            if (failResultUI == null)
            {
                failResultUI = GetComponentInParent<FailResultUI>();
            }
            if (failCountText == null)
            {
                failCountText = GetComponentInChildren<CounterText>();
            }
        }

        protected override void Start()
        {
            base.Start();
            // gameObject.SetActive(false);
        }
    }
}