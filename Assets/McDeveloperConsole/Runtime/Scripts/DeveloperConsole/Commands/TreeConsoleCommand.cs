using System;
using System.Collections.Generic;

namespace Machamy.DeveloperConsole.Commands
{
    /// <summary>
    /// (eng) A command that can have sub-commands.<br/>
    /// (kor) 서브 커맨드를 가질 수 있는 커맨드입니다.
    /// </summary>
    [Obsolete( "TreeConsoleCommand is not Tested. Use with caution.", false )]
    public class TreeConsoleCommand : IConsoleCommand
    {
        public string Command { get; }
        public string Description { get; }
        
        
        private List<IConsoleCommand> _subCommands = new List<IConsoleCommand>();
        public TreeConsoleCommand(string command, string description = "")
        {
            Command = command;
            Description = description;
        }
        public TreeConsoleCommand AddSubCommand(IConsoleCommand command)
        {
            _subCommands.Add(command);
            return this;
        }
        public string Signature
        {
            get
            {
                return $"{Command} <sub-command>";
            }
        }

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                McConsole.Print($"Usage: {Signature}");
                return;
            }

            string subCommandName = args[0];
            var subCommand = _subCommands.Find(c => c.Command.Equals(subCommandName, StringComparison.OrdinalIgnoreCase));
            if (subCommand != null)
            {
                subCommand.Execute(args.Length > 1 ? args[1..] : Array.Empty<string>());
            }
            else
            {
                McConsole.Print($"Unknown sub-command '{subCommandName}'. Available sub-commands: {string.Join(", ", _subCommands.ConvertAll(c => c.Command))}");
            }
        }
    }
}