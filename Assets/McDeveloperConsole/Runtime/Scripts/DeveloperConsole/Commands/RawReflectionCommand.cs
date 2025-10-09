using System;
using System.Collections.Generic;

namespace Machamy.DeveloperConsole.Commands
{
    /// <summary>
    /// (eng) A Command class that invokes a static method via reflection.<br/>
    /// The method must have the signature: static void MethodName(string[] args)<br/>
    /// (kor)
    /// 리플렉션을 통해 정적 메서드를 호출하는 Command 클래스입니다.<br/>
    /// 메서드는 다음과 같은 시그니처를 가져야 합니다: static void MethodName(string[] args)
    /// </summary>
    public class RawReflectionCommand : IConsoleCommand
    {
        public string Command { get; }
        public string Description { get; }
        private string _signature;
        public string Signature => _signature;
        private readonly System.Reflection.MethodInfo _method;
        
        private string[] _arg0AutoComplete;
        public string[] Arg0AutoComplete => _arg0AutoComplete;
        
        public RawReflectionCommand(string commandName, string description, System.Reflection.MethodInfo method, string signature = null)
        {
            Command = commandName;
            Description = description;
            _method = method;
            _signature = signature ?? $"{commandName} <string[] args>";
        }
        
        public void AutoComplete(Span<string> args, ref List<string> suggestions)
        {
            if (args.Length == 0)
            {
                // No arguments yet, suggest command name or first argument options
                if (_arg0AutoComplete != null)
                    suggestions.AddRange(_arg0AutoComplete);
                return;
            }
            
            int argIndex = args.Length - 1;
            var currentArg = args[argIndex];
            if (argIndex == 0)
            {
                // 첫 번째 인자라면, 미리 지정된 자동완성 옵션이 있다면 그것들 중에서
                if (_arg0AutoComplete != null)
                {
                    foreach (var option in _arg0AutoComplete)
                    {
                        if (option.StartsWith(currentArg, StringComparison.OrdinalIgnoreCase))
                            suggestions.Add(option);
                    }
                }
            }
            // 두 번째 인자부터는 자동완성 없음
        }
        
        public void SetArg0AutoComplete(string[] suggestions)
        {
            _arg0AutoComplete = suggestions;
        }
        public void Execute(string[] args)
        {
            _method.Invoke(null, new object[] { args });
        }
    }
}