using System;
using System.Collections.Generic;
using Machamy.DeveloperConsole.Commands;
using UnityEngine;

namespace Machamy.DeveloperConsole
{
    /// <summary>
    /// (eng) Built-in console commands for the developer console.<br/>
    /// You can implement commands like this class to have them automatically registered.<br/>
    /// (kor) 개발자 콘솔을 위한 내장 콘솔 명령어들입니다.
    /// 이 클래스처럼 명령어를 구현하면 자동으로 등록됩니다.
    /// </summary>
    public static class BuiltInCommands
    {
#if !DO_NOT_USE_DEBUG_CONSOLE
         /// <summary>
        /// (eng) A built-in 'help' command that lists all registered commands or provides details for a specific command.<br/>
        /// (kor) 등록된 모든 명령어를 나열하거나 특정 명령어에 대한 세부 정보를 제공하는 내장 'help' 명령어입니다.
        /// </summary>
        private class HelpCommand : IConsoleCommand
        {
            public string Command => "help";
            public string Description => "등록된 모든 명령어를 출력합니다.";
            public string Signature => "help [command]";

            public void Execute(string[] args)
            {
                if(args.Length == 0)
                {
                    var commands = CommandLibrary.GetAllCommands();
                    McConsole.MessageInfo("Available Commands:");
                    foreach (var command in commands)
                    {
                        McConsole.MessageDefault($"- {command.Signature}: {command.Description}");
                    }
                    return;
                }
                else
                {
                    string commandName = args[0];
                    if (CommandLibrary.TryGetCommand(commandName, out var command))
                    {
                        McConsole.MessageInfo( $"Command: {command.Signature}");
                        McConsole.MessageDefault($"Description: {command.Description}");
                    }
                    else
                    {
                        McConsole.MessageError($"No help available for unknown command: '{commandName}'");
                    }

                    return;
                }
            }
            
            public void AutoComplete(Span<string> args, ref List<string> suggestions)
            {
                if (args.Length == 0)
                {
                    // 모든 명령어 제안
                    foreach (var cmd in CommandLibrary.GetAllCommands())
                    {
                        suggestions.Add(cmd.Command);
                    }
                    return;
                }
                
                int argIndex = args.Length - 1;
                var currentArg = args[argIndex];
                if (argIndex == 0)
                {
                    // 첫 번째 인자라면, 등록된 명령어들 중에서
                    foreach (var cmd in CommandLibrary.GetAllCommands())
                    {
                        if (cmd.Command.StartsWith(currentArg, StringComparison.OrdinalIgnoreCase))
                            suggestions.Add(cmd.Command);
                    }
                }
                // 두 번째 인자부터는 자동완성 없음
            }
        }

        private static SimpleCommand ping; 
        private static HelpCommand helpCommand = new HelpCommand();
        private static RawCommand help2Command;

        static BuiltInCommands()
        {
            /*
             * (eng) Built-in commands registration
             * (kor) 내장 명령어 등록
             */
            ping = new SimpleCommand(
                "ping",
                "Checks the responsiveness of the console.",
                () => McConsole.Print("Pong!")
            );
            
            help2Command = new RawCommand("help2", "Provides help information for commands. (RawCommand version)", "help2 [command]",
                action:(args) =>
            {
                if(args.Length == 0)
                {
                    var commands = CommandLibrary.GetAllCommands();
                    McConsole.MessageInfo("Available Commands:");
                    foreach (var command in commands)
                    {
                        McConsole.MessageDefault($"- {command.Signature}: {command.Description}");
                    }
                    return;
                }
                else
                {
                    string commandName = args[0];
                    if (CommandLibrary.TryGetCommand(commandName, out var command))
                    {
                        McConsole.MessageInfo( $"Command: {command.Signature}");
                        McConsole.MessageDefault($"Description: {command.Description}");
                    }
                    else
                    {
                        McConsole.MessageError($"No help available for unknown command: '{commandName}'");
                    }

                    return;
                }
            },  autoCompleteAction:(args, suggestions) =>
            {
                if (args.Length == 0)
                {
                    // 모든 명령어 제안
                    foreach (var cmd in CommandLibrary.GetAllCommands())
                    {
                        suggestions.Add(cmd.Command);
                    }
                    return;
                }
                
                int argIndex = args.Length - 1;
                var currentArg = args[argIndex];
                if (argIndex == 0)
                {
                    // 첫 번째 인자라면, 등록된 명령어들 중에서
                    foreach (var cmd in CommandLibrary.GetAllCommands())
                    {
                        if (cmd.Command.StartsWith(currentArg, StringComparison.OrdinalIgnoreCase))
                            suggestions.Add(cmd.Command);
                    }
                }
                // 두 번째 인자부터는 자동완성 없음
            });
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void RegisterCommands()
        {
            CommandLibrary.RegisterCommand(ping);
            CommandLibrary.RegisterCommand(helpCommand);
            CommandLibrary.RegisterCommand(help2Command);
        }
        #endif
    }
}