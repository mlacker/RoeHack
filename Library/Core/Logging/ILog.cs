using System;

namespace RoeHack.Library.Core.Logging
{
    public interface ILog
    {
        void Debug(string message);

        void Info(string message);

        void Error(string message, Exception exception = null);

        void LogLevel(LogLevel level, string message, Exception exception = null);
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Error
    }
}
