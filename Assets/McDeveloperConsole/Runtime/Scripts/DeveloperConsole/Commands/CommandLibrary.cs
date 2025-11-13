
using Machamy.Utils;
using System;
using System.Collections.Generic;
using Machamy.DeveloperConsole.Attributes;
using UnityEngine;
using UnityEngine.Scripting;

namespace Machamy.DeveloperConsole.Commands
{
    /// <summary>
    /// (eng) A static library that manages all console commands.<br/>
    /// (kor) 모든 콘솔 명령어를 관리하는 정적 라이브러리입니다.
    /// </summary>
    [Preserve]
    public static class CommandLibrary
    {
        /// <summary>
        /// (eng) A dictionary that maps command names to their corresponding IConsoleCommand instances.<br/>
        /// (kor) 명령어 이름을 해당 IConsoleCommand 인스턴스에 매핑하는 사전입니다.
        /// </summary>
        private static readonly SortedDictionary<string, IConsoleCommand> _commands = new SortedDictionary<string, IConsoleCommand>();
        
       
  

        
        
        /// <summary>
        /// (eng) Registers a new console command.<br/>
        /// If a command with the same name already exists, it will be overwritten.<br/>
        /// (kor) 새로운 콘솔 명령어를 등록합니다.<br/>
        /// 동일한 이름의 명령어가 이미 존재하는 경우, 덮어쓰게 됩니다.
        /// </summary>
        /// <param name="command"></param>
        public static void RegisterCommand(IConsoleCommand command)
        {
            if (_commands.ContainsKey(command.Command))
            {
                LogEx.LogWarning($"Command '{command.Command}' is already registered. Overwriting.");
            }
            else
            {
                LogEx.Log($"Registered command: {command.Command} ({command.GetType().Name})");
            }
            _commands[command.Command] = command;
        }
        
        /// <summary>
        /// (eng) Unregisters a console command by its name.<br/>
        /// If the command does not exist, a warning will be logged.<br/>
        /// (kor) 콘솔 명령어를 이름으로 등록 해제합니다.<br/>
        /// 명령어가 존재하지 않는 경우, 경고가 기록됩니다.
        /// </summary>
        /// <param name="commandName"></param>
        public static void UnregisterCommand(string commandName)
        {
            if (_commands.Remove(commandName))
            {
                LogEx.Log($"Unregistered command: {commandName}");
            }
            else
            {
                LogEx.LogWarning($"Command '{commandName}' is not registered.");
            }
        }

        /// <summary>
        /// (eng) Tries to get a registered console command by its name.<br/>
        /// Returns true if found, false otherwise.<br/>
        /// (kor) 이름으로 등록된 콘솔 명령어를 가져오려고 시도합니다.<br/>
        /// 찾으면 true, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static bool TryGetCommand(string commandName, out IConsoleCommand command)
        {
#if !DO_NOT_USE_DEBUG_CONSOLE
            return _commands.TryGetValue(commandName, out command);
#else
            command = null;
            return false;
#endif
        }

        /// <summary>
        /// (eng) Gets all registered console commands.<br/>
        /// (kor) 등록된 모든 콘솔 명령어를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IConsoleCommand> GetAllCommands()
        {
            return _commands.Values;
        }
        

    #if !DO_NOT_USE_DEBUG_CONSOLE
        /// <summary>
        /// (eng) 
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            /*
             * ConsoleCommandAttribute 를 전부 찾아서 등록
             */
            var commandTypes = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in commandTypes)
            {
                foreach (var type in assembly.GetTypes())
                {
                    var classAttrs = type.GetCustomAttributes(typeof(ConsoleCommandClassAttribute), false);
                    if (classAttrs.Length > 0)
                    {
                        object command = Activator.CreateInstance(type);
                        if (command is IConsoleCommand consoleCommand)
                        {
                            RegisterCommand(consoleCommand);
                        }
                        else
                        {
                            LogEx.LogWarning($"Failed to create console command for class \"{type.FullName}\". \n Make sure the class implements IConsoleCommand interface and has a parameterless constructor.");
                        }
                    }
                    foreach (var method in type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
                    {
                        var attrs = method.GetCustomAttributes(typeof(ConsoleCommandAttribute), false);
                        if (attrs.Length > 0)
                        {
                            var attr = (ConsoleCommandAttribute)attrs[0];
                            if(method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string[]))
                            {
                                // RawReflectionCommand 생성
                                var rawCommand = new RawReflectionCommand(attr.Command, attr.Description, method, attr.Signature);
                                if(attr.Arg0AutoComplete != null)
                                    rawCommand.SetArg0AutoComplete(attr.Arg0AutoComplete);
                                RegisterCommand(rawCommand);
                            }else{
                                // ReflectionCommand 생성
                                bool success = ReflectionCommand.Create(attr.Command, attr.Description, method, attr.Signature, out ReflectionCommand consoleCommand);
                                if (success)
                                {
                                    if(attr.Arg0AutoComplete != null)
                                        consoleCommand.SetArg0AutoComplete(attr.Arg0AutoComplete);
                                    RegisterCommand(consoleCommand);
                                }
                                else
                                {
                                    LogEx.LogWarning($"Failed to create console command for method \"{type.FullName}.{method.Name}\". \n Make sure the method is static and all parameter types are supported.");
                                }
                            }
                            
                        }
                    }
                }
            }
        }
    #endif
        
    }
}