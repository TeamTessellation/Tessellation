using Player;
using UI.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.OtherUIs
{
    public class FailResultEntry : UIBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField] private FailResultUI failResultUI;
        [SerializeField] private CounterText failCountText;
 
        [SerializeField] private string variableKeyString;
        [HideInInspector][SerializeField] private PlayerStatus.VariableKey variableKey;

        public enum Direction
        {
            Left,Right
        }

        [field:SerializeField] public Direction MoveDirection { get; private set; }
        [SerializeField] public Vector2 MoveVector => MoveDirection == Direction.Left ? Vector2.left : Vector2.right;
        
        public FailResultUI FailResultUI => failResultUI;
        public CounterText FailCountText => failCountText;
        
        public string VariableKey => variableKeyString;
        
        protected void Reset()
        {
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

        public void OnBeforeSerialize()
        {
            // 에디터에서는 문자열이 직접 수정되므로 enum 동기화
            if (!string.IsNullOrEmpty(variableKeyString))
            {
                if (System.Enum.TryParse<PlayerStatus.VariableKey>(variableKeyString, out var parsedKey))
                {
                    variableKey = parsedKey;
                }
            }
        }

        public void OnAfterDeserialize()
        {
            // 문자열에서 enum으로 파싱
            if (System.Enum.TryParse<PlayerStatus.VariableKey>(variableKeyString, out var parsedKey))
            {
                variableKey = parsedKey;
            }
            else
            {
                variableKey = PlayerStatus.VariableKey.TotalScore;
                variableKeyString = variableKey.ToString();
            }
        }

    }
}