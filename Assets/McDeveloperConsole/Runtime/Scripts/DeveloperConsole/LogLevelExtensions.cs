using UnityEngine;

namespace Machamy.DeveloperConsole
{
    public static class LogLevelExtensions
    {
        public static LogType ToLogType(this LogLevel level)
        {
            return level switch
            {
                LogLevel.Exception => LogType.Exception,
                LogLevel.Error => LogType.Error,
                LogLevel.Warning => LogType.Warning,
                LogLevel.Info => LogType.Log,
                _ => LogType.Log,
            };
        }
        public static LogLevel FromLogType(this LogType type)
        {
            return type switch
            {
                LogType.Exception => LogLevel.Exception,
                LogType.Error => LogLevel.Error,
                LogType.Warning => LogLevel.Warning,
                LogType.Log => LogLevel.Info,
                _ => LogLevel.Info,
            };
        }
            
        public static bool Includes(this LogLevel current, LogLevel other)
        {
            return current >= other;
        }
        
    }
}