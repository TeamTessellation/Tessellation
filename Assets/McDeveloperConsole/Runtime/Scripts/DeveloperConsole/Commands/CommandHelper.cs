using System;

namespace Machamy.DeveloperConsole.Commands
{
    /// <summary>
    /// (eng) Helper class for console commands.<br/>
    /// (kor) 콘솔 커맨드를 위한 헬퍼 클래스.
    /// </summary>
    public static class CommandHelper
    {
        public static void DefaultAutoComplete(Type type, Span<string> args, ref System.Collections.Generic.List<string> suggestions)
        {
            string currentArg = args.Length > 0 ? args[args.Length - 1] : "";
            
            if (type == typeof(bool))
            {
                if ("true".StartsWith(currentArg, StringComparison.OrdinalIgnoreCase))
                    suggestions.Add("true");
                if ("false".StartsWith(currentArg, StringComparison.OrdinalIgnoreCase))
                    suggestions.Add("false");
            }else if (type.IsEnum)
            {
                foreach (var name in Enum.GetNames(type))
                {
                    if (name.StartsWith(currentArg, StringComparison.OrdinalIgnoreCase))
                        suggestions.Add(name);
                }
            }else if (type == typeof(int))
            {
                if ("0".StartsWith(currentArg, StringComparison.OrdinalIgnoreCase))
                    suggestions.Add("0");
                if ("1".StartsWith(currentArg, StringComparison.OrdinalIgnoreCase))
                    suggestions.Add("1");
                if ("-1".StartsWith(currentArg, StringComparison.OrdinalIgnoreCase))
                    suggestions.Add("-1");
            }else if (type == typeof(float))
            {
                // 0.0, 0.5, 1.0
                if ("0.0".StartsWith(currentArg, StringComparison.OrdinalIgnoreCase))
                    suggestions.Add("0.0");
                if ("0.5".StartsWith(currentArg, StringComparison.OrdinalIgnoreCase))
                    suggestions.Add("0.5");
                if ("1.0".StartsWith(currentArg, StringComparison.OrdinalIgnoreCase))
                    suggestions.Add("1.0");
            }else if (type == typeof(string))
            {
                if("string".StartsWith(currentArg, StringComparison.OrdinalIgnoreCase))
                    suggestions.Add("string");
            }
        }
        
        public static T ParseArgument<T>(this IConsoleCommand consoleCommand, string arg)
        {
            try
            {
                return ParseArgument<T>(arg);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException($"Error in command '{consoleCommand.Command}': Cannot parse argument '{arg}' to type {typeof(T)}");
            }
        }
        
        public static T ParseArgument<T>(string arg)
        {
            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(arg, out int result))
                    return (T)(object)result;
            }
            else if (typeof(T) == typeof(float))
            {
                if (float.TryParse(arg, out float result))
                    return (T)(object)result;
            }
            else if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(arg, out bool result))
                    return (T)(object)result;
                if (arg == "1")
                    return (T)(object)true;
                if (arg == "0")
                    return (T)(object)false;
                if (arg.ToLower().Equals("t", StringComparison.OrdinalIgnoreCase))
                    return (T)(object)true;
                if (arg.ToLower().Equals("f", StringComparison.OrdinalIgnoreCase))
                    return (T)(object)false;
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)(object)arg;
            }
            
            throw new ArgumentException($"Cannot parse argument '{arg}' to type {typeof(T)}");
        }
    }
}