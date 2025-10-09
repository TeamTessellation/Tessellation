using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Machamy.DeveloperConsole
{
    /// <summary>
    /// (eng) Message type for console messages.<br/>
    /// Used for categorizing and styling messages.<br/>
    /// (kor) 콘솔 메시지의 메시지 타입입니다.<br/>
    /// 메시지를 분류하고 스타일링하는 데 사용됩니다.
    /// </summary>
    public struct MessageType
    {
        private static List<MessageType> _allTypes = new List<MessageType>();
        
        public static readonly MessageType Default = new MessageType("default");
        public static readonly MessageType Info = new MessageType("info");
        public static readonly MessageType Warning = new MessageType("warn");
        public static readonly MessageType Error = new MessageType("error");
        public static readonly MessageType Debug = new MessageType("debug");
        public static readonly MessageType Success = new MessageType("success");
        public static readonly MessageType Gray = new MessageType("gray");
        public static readonly MessageType White = new MessageType("white");
        public static readonly MessageType Cyan = new MessageType("cyan");

        
        private string ussTag;

        
        
        public IReadOnlyList<MessageType> AllTypes => _allTypes;
        private MessageType(string ussTag)
        {
            this.ussTag = ussTag;
            _allTypes.Add(this);
        }
        
        public string UssTag => ussTag;
        public override string ToString() => ussTag;
        public static implicit operator string(MessageType type) => type.ussTag;

        [Preserve, RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            
        }
    }
}