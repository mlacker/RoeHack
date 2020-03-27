using System;

namespace RoeHack.Library.Core.Logging
{
    public struct LogEntry
    {
        public LogEntry(LogLevel level, string message, Exception exception)
        {
            Level = level;
            Message = message;
            Exception = exception;
        }

        public LogLevel Level { get; }
        public string Message { get; }
        public Exception Exception { get; }
    }
}
