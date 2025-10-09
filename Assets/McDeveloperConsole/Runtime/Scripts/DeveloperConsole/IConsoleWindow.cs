using UnityEngine;

namespace Machamy.DeveloperConsole
{
    public interface IConsoleWindow
    {
        void Open();
        void Close();
        /// <summary>
        /// 콘솔에 메시지를 보냅니다.
        /// </summary>
        void Message(string message);
        void Message(MessageType type, string message);
        void MessageInfo(string message) => Message(MessageType.Info, message);
        void MessageWarning(string message) => Message(MessageType.Warning, message);
        void MessageError(string message) => Message(MessageType.Error, message);
        void MessageDebug(string message) => Message(MessageType.Debug, message);
        void MessageSuccess(string message) => Message(MessageType.Success, message);
        /// <summary>
        /// 콘솔에 로그를 출력합니다.
        /// </summary>
        void Print(string message);
        void Print(LogType type, string message);
        void PrintInfo(string message) => Print(LogType.Log, message);
        void PrintWarning(string message) => Print(LogType.Warning, message);
        void PrintError(string message) => Print(LogType.Error, message);
        void PrintException(string message) => Print(LogType.Exception, message);
        void ClearHistory();
        void Toggle();
        bool IsOpen { get; }
    }
}