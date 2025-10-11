using System;
using System.Collections.Generic;

namespace Machamy.DeveloperConsole.Commands
{
    /// <summary>
    /// (eng) Interface for console commands.<br/>
    /// (kor) 콘솔 커맨드를 위한 인터페이스.
    /// </summary>
    public interface IConsoleCommand
    {
        /// <summary>
        /// (eng) The command string that triggers this command (e.g., "spawn").<br/>
        /// (kor) 이 명령어를 실행하는 명령어 문자열입니다. (예: "spawn")
        /// </summary>
        string Command { get; }
        /// <summary>
        /// (eng) A brief description of what this command does.<br/>
        /// (kor) 이 명령어가 수행하는 작업에 대한 간단한 설명입니다.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// (eng) A string that describes the usage of this command (e.g., "spawn [entityName]").<br/>
        /// (kor) 이 명령어의 사용법을 설명하는 문자열입니다. (예: "spawn [entityName]")
        /// &lt;&gt; is for required arguments,
        /// [] is for optional arguments.
        /// </summary>
        string Signature { get; }
        void Execute(string[] args);
        
        /// <summary>
        /// (eng) Provides auto-completion suggestions based on the current arguments.<br/>
        /// (kor) 현재 인수를 기반으로 자동 완성 제안을 제공합니다.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="suggestions"></param>
        void AutoComplete(Span<string> args, ref List<string> suggestions) { }
    }
}