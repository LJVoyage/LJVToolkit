using UnityEngine;

namespace VoyageForge.Depot.Runtime.Console
{
    /// <summary>
    /// 控制台日志的过滤类型。
    /// 用于控制日志列表只显示某一类日志。
    /// </summary>
    public enum ConsoleFilter
    {
        /// <summary>显示所有日志。</summary>
        All,

        /// <summary>仅显示普通日志（LogType.Log）。</summary>
        Log,

        /// <summary>仅显示警告（LogType.Warning）。</summary>
        Warning,

        /// <summary>仅显示错误与异常（LogType.Error / Assert / Exception）。</summary>
        Error
    }

    /// <summary>
    /// 单条控制台日志记录（不可变值类型）。
    /// 由 <see cref="RuntimeConsole"/> 在收到 Unity 日志回调时创建并缓冲。
    /// </summary>
    public readonly struct ConsoleLogEntry
    {
        /// <summary>日志正文。</summary>
        public readonly string Message;

        /// <summary>调用堆栈（可能为空字符串）。</summary>
        public readonly string StackTrace;

        /// <summary>日志类型（Log / Warning / Error / Assert / Exception）。</summary>
        public readonly LogType Type;

        /// <summary>记录时间，格式 HH:mm:ss.fff。</summary>
        public readonly string Timestamp;

        /// <summary>
        /// 构造一条日志记录。
        /// </summary>
        /// <param name="message">日志正文。</param>
        /// <param name="stackTrace">调用堆栈。</param>
        /// <param name="type">日志类型。</param>
        /// <param name="timestamp">记录时间字符串。</param>
        public ConsoleLogEntry(string message, string stackTrace, LogType type, string timestamp)
        {
            Message = message;
            StackTrace = stackTrace;
            Type = type;
            Timestamp = timestamp;
        }
    }
}
