using Machamy.Utils;
using System;
using System.Collections.Generic;

namespace Machamy.DeveloperConsole.Commands
{


    /// <summary>
    /// (eng) A simple Command class that takes no arguments.<br/>
    /// (kor) 인자를 받지 않는 간단한 Command 클래스입니다. 
    /// </summary>
    public class SimpleCommand : IConsoleCommand
    {
        public string Command { get; }
        public string Description { get; }
        public string Signature => Command;
        private readonly Action _action;

        public SimpleCommand(string command, string description, Action action)
        {
            Command = command;
            Description = description;
            _action = action;
        }

        public void Execute(string[] args)
        {
            _action.Invoke();
        }
    }
    
    /// <summary>
    /// (eng) A simple Command class that takes one argument.<br/>
    /// It attempts to automatically convert the argument type.<br/>
    /// Supported types are int, float, bool, string, and enum.<br/>
    /// (kor) 하나의 인자를 받는 간단한 Command 클래스입니다.<br/>
    /// 인자 타입을 자동으로 변환하려고 시도합니다.<br/>
    /// 지원되는 타입은 int, float, bool, string, enum입니다.
    /// </summary>
    public class SimpleCommand<T> : IConsoleCommand
    {
        public string Command { get; }
        public string Description { get; }
        private readonly Action<T> _action;
        private readonly string _signature;
        public string Signature => _signature;
        public Action<string[], List<string>> AutoCompleteAction { get; set; }

        public SimpleCommand(string command, string description, Action<T> action, string signature = null)
        {
            Command = command;
            Description = description;
            _action = action;
            _signature = signature ?? $"{command} <{typeof(T).Name}>";
        }
        
        public void AutoComplete(Span<string> args, ref List<string> suggestions)
        {
            if (AutoCompleteAction != null)
            {
                AutoCompleteAction.Invoke(args.ToArray(), suggestions);
                return;
            }
            // 기본 자동완성 동작
            if (args.Length > 1)
                return;
            CommandHelper.DefaultAutoComplete(typeof(T), args, ref suggestions);
        }

        public void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                LogEx.LogError($"Command '{Command}' requires an argument of type {typeof(T).Name}");
                return;
            }

            try
            {
                T arg = CommandHelper.ParseArgument<T>(this, args[0]);
                _action.Invoke(arg);
            }
            catch (Exception e)
            {
                LogEx.LogError($"Failed to execute command '{Command}': {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// (eng) A simple Command class that takes two arguments.<br/>
    /// It attempts to automatically convert the argument types.<br/>
    /// Supported types are int, float, bool, string, and enum.<br/>
    /// (kor) 두 개의 인자를 받는 간단한 Command 클래스입니다.<br/>
    /// 인자 타입을 자동으로 변환하려고 시도합니다.<br/>
    /// 지원되는 타입은 int, float, bool, string, enum입니다.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class SimpleCommand<T1, T2> : IConsoleCommand
    {
        public string Command { get; }
        public string Description { get; }
        private readonly Action<T1, T2> _action;
        private readonly string _signature;
        public string Signature => _signature;
        public Action<string[], List<string>> AutoCompleteAction { get; set; }

        public SimpleCommand(string command, string description, Action<T1, T2> action, string signature = null)
        {
            Command = command;
            Description = description;
            _action = action;
            _signature = signature ?? $"{command} <{typeof(T1).Name}> <{typeof(T2).Name}>";
        }
        
        public void AutoComplete(Span<string> args, ref List<string> suggestions)
        {
            if (AutoCompleteAction != null)
            {
                AutoCompleteAction.Invoke(args.ToArray(), suggestions);
                return;
            }
            // 기본 자동완성 동작
            if (args.Length == 0)
                return;
            if (args.Length > 2)
                return;
            int argIndex = args.Length - 1;
            var currentArg = args[argIndex];
            var targetType = argIndex == 0 ? typeof(T1) : typeof(T2);
            CommandHelper.DefaultAutoComplete(targetType, args, ref suggestions);
        }

        public void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                LogEx.LogError($"Command '{Command}' requires two arguments of types {typeof(T1).Name} and {typeof(T2).Name}");
                return;
            }

            try
            {
                T1 arg1 = CommandHelper.ParseArgument<T1>(this, args[0]);
                T2 arg2 = CommandHelper.ParseArgument<T2>(this, args[1]);
                _action.Invoke(arg1, arg2);
            }
            catch (Exception e)
            {
                LogEx.LogError($"Failed to execute command '{Command}': {e.Message}");
            }
        }
    }
}