using UnityEngine;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 微图日志
    /// </summary>
    public static class MicroGraphLogger
    {
        /// <summary>
        /// 日志过滤器
        /// </summary>
        public static LogType LogFilter = LogType.Log;
        private static string LogTitle => $"[MicroGraph - {Time.frameCount}]:";
        public static void Log(object message)
        {
            if (LogType.Log <= LogFilter)
                Debug.Log($"{LogTitle} {message}");
        }
        public static void LogFormat(string format, params object[] args)
        {
            if (LogType.Log <= LogFilter)
                Debug.LogFormat($"{LogTitle} {format}", args);
        }
        public static void LogWarning(object message)
        {
            if (LogType.Warning <= LogFilter)
                Debug.LogWarning($"{LogTitle} {message}");
        }
        public static void LogWarningFormat(string format, params object[] args)
        {
            if (LogType.Warning <= LogFilter)
                Debug.LogWarningFormat($"{LogTitle} {format}", args);
        }
        public static void LogError(object message)
        {
            if (LogType.Error <= LogFilter)
                Debug.LogError($"{LogTitle} {message}");
        }
        public static void LogErrorFormat(string format, params object[] args)
        {
            if (LogType.Error <= LogFilter)
                Debug.LogErrorFormat($"{LogTitle} {format}", args);
        }
    }
}
