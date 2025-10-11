using Machamy.Utils;
using System;
using System.Collections.Generic;

namespace Machamy.DeveloperConsole.Commands
{
    /// <summary>
    /// (eng) A console command registered via reflection.<br/>
    /// Supported parameter types are int, float, bool, string, and enum.<br/>
    /// Each type is automatically parsed.<br/>
    /// (kor)
    /// 리플렉션을 통해 등록된 콘솔 커맨드입니다.<br/>
    /// 지원되는 매개변수 타입은 int, float, bool, string, enum입니다.<br/>
    /// 각 타입은 자동으로 파싱됩니다.
    /// </summary>
    public class ReflectionCommand : IConsoleCommand
    {
        public string Command { get; }
        public string Description { get; }
        
        private string _signature;
        public string Signature { get => _signature; }
        private readonly System.Reflection.MethodInfo _method;
        private readonly Type[] _paramType;
        private string[] _arg0AutoComplete;
        public string[] Arg0AutoComplete => _arg0AutoComplete;

        private ReflectionCommand(string commandName, string description, System.Reflection.MethodInfo method)
        {
            Command = commandName;
            Description = description;
            _method = method;
            _paramType = Array.ConvertAll(_method.GetParameters(), p => p.ParameterType);
        }
        
        public void SetArg0AutoComplete(string[] suggestions)
        {
            _arg0AutoComplete = suggestions;
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

            if (args.Length > _paramType.Length)
                return; // No more parameters to suggest for
            
            int argIndex = args.Length - 1;
            var currentArg = args[argIndex];
            var targetType = _paramType[argIndex];
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
                    return;
                }
            }
            
            // 기본 자동완성
            CommandHelper.DefaultAutoComplete(targetType, args, ref suggestions);
    }

        public void Execute(string[] args)
        {
            try
            {
                var convertedArgs = ConvertArguments(args);
                _method.Invoke(null, convertedArgs);
            }
            catch (Exception ex)
            {
                McConsole.MessageError($"Error executing command '{Command}': {ex.Message}");
            }
        }
        public static bool Create(string commandName, string description, System.Reflection.MethodInfo method,string signature, out ReflectionCommand command)
        {
            if (!method.IsStatic)
            {
                command = null;
                return false;
            }

            command = new ReflectionCommand(commandName, description, method);
            if (!AreAllParametersSupported(command._paramType))
            {
                command = null;
                return false;
            }
            command._signature = signature ?? $"{commandName} " + string.Join(" ", Array.ConvertAll(command._paramType, t => $"<{t.Name}>"));
            return true;
        }
        
        public static bool Create(string commandName, string description, System.Reflection.MethodInfo method, out ReflectionCommand command)
        {
            return Create(commandName, description, method, null, out command);
        }
        
        
        
        private static bool IsSupportedType(Type type)
        {
            return type == typeof(int) || type == typeof(float) || type == typeof(bool) || type == typeof(string) || type.IsEnum;
        }
        
        private static bool AreAllParametersSupported(Type[] types)
        {
            foreach (var type in types)
            {
                if (!IsSupportedType(type))
                    return false;
            }
            return true;
        }
        
        private object[] ConvertArguments(string[] args)
        {
            if (args.Length != _paramType.Length)
                throw new ArgumentException($"Argument count mismatch. Expected {_paramType.Length}, got {args.Length}");

            object[] convertedArgs = new object[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var targetType = _paramType[i];
                var arg = args[i];
                if (targetType == typeof(int))
                    convertedArgs[i] = this.ParseArgument<int>(arg);
                else if (targetType == typeof(float))
                    convertedArgs[i] = this.ParseArgument<float>(arg);
                else if (targetType == typeof(bool))
                    convertedArgs[i] = this.ParseArgument<bool>(arg);
                else if (targetType == typeof(string))
                    convertedArgs[i] = this.ParseArgument<string>(arg);
                else if (targetType.IsEnum)
                {
                    if (Enum.TryParse(targetType, arg, true, out object enumValue))
                        convertedArgs[i] = enumValue;
                    else if(int.TryParse(arg, out int enumInt))
                        convertedArgs[i] = Enum.ToObject(targetType, enumInt);
                    else
                        throw new ArgumentException($"Cannot parse argument '{arg}' to enum type {targetType}");
                }
                else
                {
                    throw new ArgumentException($"Unsupported parameter type: {targetType}");
                }
            }
            return convertedArgs;
        }
        

        
    }
}