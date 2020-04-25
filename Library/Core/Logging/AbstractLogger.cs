using System;

namespace RoeHack.Library.Core.Logging
{
    public abstract class AbstractLogger : ILog
    {
        public void Debug(string message) => LogLevel(Logging.LogLevel.Debug, message);

        public void Info(string message) => LogLevel(Logging.LogLevel.Info, message);

        public void Error(string message, Exception exception = null) => LogLevel(Logging.LogLevel.Error, message, exception);

        public abstract void LogLevel(LogLevel level, string message, Exception exception = null);
    }
}
