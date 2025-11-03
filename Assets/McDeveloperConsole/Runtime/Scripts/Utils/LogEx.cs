using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
#endif
namespace Machamy.Utils
{
    public interface ILogExSupport
    {
        /// <summary>
        /// 해당 로그 레벨 미만은 출력하지 않습니다.
        /// </summary>
        /// <example>
        /// LogLevel.Warning로 설정하면 Warning과 Error만 출력되고 Info는 출력되지 않습니다.
        /// </example>
        public LogEx.LogLevel LogLevel { get; }
    }
    /// <summary>
    /// (eng) Extended logging utility for Unity.<br/>
    /// Provides enhanced logging capabilities with class and method context.<br/>
    /// (kor) 유니티를 위한 확장된 로깅 유틸리티입니다.<br/>
    /// 클래스 및 메서드 컨텍스트와 함께 향상된 로깅 기능을 제공합니다.<br/>
    /// </summary>
    public static class LogEx
    {
        const string LoggerName = "LogEx";
        const string LoggerCs = "LogEx.cs";
        
        public enum LogLevel
        {
            Info,
            Warning,
            Error,
            Assert,
            NoLog,
        }

        public static bool IsEditor()
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

#if DONT_USE_LOGEX_IN_BUILD
        [Conditional("UNITY_EDITOR")]
#endif
        public static void Log(object obj, LogLevel level = LogLevel.Info, int depth = 1)
        {
            Log(obj?.ToString() ?? "null", level, depth + 1);
        }

#if DONT_USE_LOGEX_IN_BUILD
        [Conditional("UNITY_EDITOR")]
#endif
        public static void Log(string message, LogLevel level = LogLevel.Info, int depth = 1)
        {
            var stackTrace = new StackTrace(true);
            var frame = stackTrace.GetFrame(depth);
            var classType = frame.GetMethod().DeclaringType;
            var isLoggingClass = typeof(ILogExSupport).IsAssignableFrom(classType);
            if (isLoggingClass)
            {
                if (Activator.CreateInstance(classType!) is ILogExSupport instance && level < instance.LogLevel)
                {
                    return;
                }
            }
            var className = classType?.Name ?? "UnknownClass";
            var method = frame.GetMethod();
            // [] 가 이미 있으면 []는 분홍색으로
            // %className% 와 %methodName%은 치환
            string logMessage;

            string GetPretty(string text)
            {
                StringBuilder sb = new StringBuilder(text);
                sb.Replace("%class%", $"<color=#4FC3F7>{className}</color>");
                sb.Replace("%method%", $"<color=#FFD54F>{method.Name}</color>");
                int startIdx = sb.ToString().IndexOf("[", StringComparison.Ordinal);
                int endIdx = sb.ToString().IndexOf("]", StringComparison.Ordinal);
                if (startIdx != -1 && endIdx != -1 && endIdx > startIdx)
                {
                    string insideBrackets = sb.ToString().Substring(startIdx + 1, endIdx - startIdx - 1);
                    string coloredInside = $"<color=#FF4081>{insideBrackets}</color>";
                    sb.Remove(startIdx + 1, endIdx - startIdx - 1);
                    sb.Insert(startIdx + 1, coloredInside);
                }
                return sb.ToString();
            }
            
            if(message.StartsWith("[") && message.Contains("]"))
            {
                logMessage = GetPretty(message);
            }
            else
            {
                logMessage = GetPretty($"[%class% :: %method%] {message}");
            }
            
            // if (message.StartsWith("[") && message.Contains("]"))
            // {
            //      
            // }
            // else
            // {
            //     logMessage = $"[<color=#4FC3F7>{className}</color> :: <color=#FFD54F>{method.Name}</color>] {message}";
            // }
            switch (level)
            {
                case LogLevel.NoLog:
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log(logMessage);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(logMessage);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(logMessage);
                    break;
                case LogLevel.Assert:
                    UnityEngine.Debug.LogAssertion(logMessage);
                    break;
            }
        }

#if DONT_USE_LOGEX_IN_BUILD
        [Conditional("UNITY_EDITOR")]
#endif
        public static void Log(Exception ex, int depth = 1)
        {
            var stackTrace = new StackTrace(true);
            var frame = stackTrace.GetFrame(depth);
            var classType = frame.GetMethod().DeclaringType;
            var isLoggingClass = typeof(ILogExSupport).IsAssignableFrom(classType);
            if (isLoggingClass)
            {
                if (Activator.CreateInstance(classType!) is ILogExSupport instance && LogLevel.Error < instance.LogLevel)
                {
                    return;
                }
            }
            var className = classType?.Name ?? "UnknownClass";
            var method = frame.GetMethod();
            string logMessage = $"[{className} :: {method.Name}] Exception occurred - {ex.Message}\n{ex.StackTrace}";
            UnityEngine.Debug.LogError(logMessage);
        }

#if DONT_USE_LOGEX_IN_BUILD
        [Conditional("UNITY_EDITOR")]
#endif
        public static void LogWarning(object obj, int depth = 1)
        {
            Log(obj?.ToString() ?? "null", LogLevel.Warning, depth + 1);
        }
        
#if DONT_USE_LOGEX_IN_BUILD
        [Conditional("UNITY_EDITOR")]
#endif
        public static void LogWarning(string message, int depth = 1)
        {
            Log(message, LogLevel.Warning, depth + 1);
        }
        
#if DONT_USE_LOGEX_IN_BUILD
        [Conditional("UNITY_EDITOR")]
#endif
        public static void LogError(object obj, int depth = 1)
        {
            Log(obj?.ToString() ?? "null", LogLevel.Error, depth + 1);
        }

#if DONT_USE_LOGEX_IN_BUILD
        [Conditional("UNITY_EDITOR")]
#endif
        public static void LogError(string message, int depth = 1)
        {
            Log(message, LogLevel.Error, depth + 1);
        }
        
#if DONT_USE_LOGEX_IN_BUILD
        [Conditional("UNITY_EDITOR")]
#endif
        public static void LogError(Exception ex, int depth = 1)
        { 
            Log(ex, depth + 1);
        }
        
#if UNITY_EDITOR
        [OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj == null || obj.name != LoggerName)
            {
                return false;
            }
            string stackTrace = GetStackTrace();
            if(!string.IsNullOrEmpty(stackTrace)) // can customize the label to be added here; the original code is confusing and does not need to be modified, you need to locate it yourself;
            {
                Match matches = Regex.Match(stackTrace, @"\(at(.+)\)", RegexOptions.IgnoreCase);
                string pathline = "";
                while (matches.Success)
                {
                    pathline = matches.Groups[1].Value;
                    if (!pathline.Contains(LoggerCs)) break;
                    matches = matches.NextMatch();
                }

                if (matches.Success)
                {
                    pathline = matches.Groups[1].Value;
                    pathline = pathline.Replace(" ", "");

                    int split_index = pathline.LastIndexOf(":");
                    string path = pathline.Substring(0, split_index);
                    line = Convert.ToInt32(pathline.Substring(split_index + 1));
                    string fullpath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
                    fullpath = fullpath + path;
                    string strPath = System.IO.Path.DirectorySeparatorChar == '\\' ? fullpath.Replace('/', '\\') : fullpath.Replace('\\', '/');
                    UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(strPath, line);
                }
                return true;
                
            }
            return false;
        }


        /// <summary>
        /// 유니티 콘솔창에서 스택트레이스를 가져옵니다.
        /// https://www.programmersought.com/article/3933462942/
        /// </summary>
        /// <returns></returns>
        private static string GetStackTrace()
        {
            // Find the assembly of UnityEditor.EditorWindow
            var assemblyUnityEditor = Assembly.GetAssembly(typeof(UnityEditor.EditorWindow));
            if (assemblyUnityEditor == null) return null;

            // Find the class UnityEditor.ConsoleWindow
            var typeConsoleWindow = assemblyUnityEditor.GetType("UnityEditor.ConsoleWindow");
            if (typeConsoleWindow == null) return null;
            // Find the member ms_ConsoleWindow in UnityEditor.ConsoleWindow
            var fieldConsoleWindow = typeConsoleWindow.GetField("ms_ConsoleWindow",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (fieldConsoleWindow == null) return null;
            // Get the value of ms_ConsoleWindow
            var instanceConsoleWindow = fieldConsoleWindow.GetValue(null);
            if (instanceConsoleWindow == null) return null;

            // If the focus window of the console window, get the stacktrace
            if ((object)UnityEditor.EditorWindow.focusedWindow == instanceConsoleWindow)
            {
                // Get the class ListViewState through the assembly
                var typeListViewState = assemblyUnityEditor.GetType("UnityEditor.ListViewState");
                if (typeListViewState == null) return null;

                // Find the member m_ListView in the class UnityEditor.ConsoleWindow
                var fieldListView = typeConsoleWindow.GetField("m_ListView",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (fieldListView == null) return null;

                // Get the value of m_ListView
                var valueListView = fieldListView.GetValue(instanceConsoleWindow);
                if (valueListView == null) return null;

                // Find the member m_ActiveText in the class UnityEditor.ConsoleWindow
                var fieldActiveText = typeConsoleWindow.GetField("m_ActiveText",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (fieldActiveText == null) return null;

                // Get the value of m_ActiveText, is the stacktrace we need
                string valueActiveText = fieldActiveText.GetValue(instanceConsoleWindow).ToString();
                return valueActiveText;
            }

            return null;
        }
#endif

        public static bool IsLogLevelEnabled(ILogExSupport obj, LogLevel level)
        {
            if (obj == null) return true; // null이면 무조건 출력
            return level >= obj.LogLevel;
        }
        
    }
}