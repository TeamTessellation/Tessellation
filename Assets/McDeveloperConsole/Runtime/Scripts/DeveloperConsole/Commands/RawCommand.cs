using System;
using System.Collections.Generic;


namespace Machamy.DeveloperConsole.Commands
{
    /// <summary>
    /// (eng) A Command class that only takes string[] arguments.<br/>
    /// You need to implement parsing yourself.<br/>
    /// (kor)
    /// 오직 string[] 인자를 받는 Command 클래스입니다.<br/>
    /// 직접 파싱을 구현해야 합니다.
    /// </summary>
    public class RawCommand : IConsoleCommand
    {
        
        public string Command { get; }
        public string Description { get; }
        private string _signature;
        
        private readonly Action<string[]> _action;
        private Action<string[], List<string>> AutoCompleteAction { get;}
        
        public string Signature => _signature;
        
        public RawCommand(string command, string description, Action<string[]> action, string signature = null, Action<string[], List<string>> autoCompleteAction = null)
        {
            Command = command;
            Description = description;
            _action = action;
            _signature = signature ?? $"{command} <string[] args>";
            AutoCompleteAction = autoCompleteAction;
        }
        public RawCommand(string command, string description, string signature, Action<string[]> action, Action<string[], List<string>> autoCompleteAction = null)
        {
            Command = command;
            Description = description;
            _action = action;
            _signature = signature;
            AutoCompleteAction = autoCompleteAction;
        }
        public void Execute(string[] args)
        {
            _action.Invoke(args);
        }
        
        public void AutoComplete(Span<string> args, ref List<string> suggestions)
        {
            if (AutoCompleteAction != null)
            {
                AutoCompleteAction.Invoke(args.ToArray(), suggestions);
                return;
            }
        }
    }
}