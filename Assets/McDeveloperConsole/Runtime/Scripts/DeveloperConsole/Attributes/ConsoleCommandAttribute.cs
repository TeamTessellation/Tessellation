using System;
using UnityEngine.Scripting;

namespace Machamy.DeveloperConsole.Attributes
{
    /// <summary>
    /// (eng) Attribute to mark methods as console commands.<br/>
    /// (kor) 메서드를 콘솔 명령어로 표시하는 특성입니다
    /// </summary>
    [Preserve]
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class ConsoleCommandAttribute : Attribute
    {
        public string Command { get; }
        public string Description { get; }
        
        public string Signature { get;}

        public string[] Arg0AutoComplete { get; }

        public ConsoleCommandAttribute(string command, string description = "", string signature = null, string[] arg0AutoComplete = null)
        {
            Command = command;
            Description = description;
            Signature = signature;
            Arg0AutoComplete = arg0AutoComplete;
        }
    }
    
    [Preserve]
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ConsoleCommandClassAttribute : Attribute
    {
    }
}