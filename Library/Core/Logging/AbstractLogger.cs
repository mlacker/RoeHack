using System;

namespace RoeHack.Library.Core.Logging
{
    public abstract class AbstractLogger : ILog
    {
        private readonly string logName;
        private readonly LogLevel logLevel;

        protected AbstractLogger(string logName, LogLevel logLevel)
        {
            this.logName = logName;
            this.logLevel = logLevel;
        }

        public bool IsDebugEnabled => IsLevelEnabled(Logging.LogLevel.Debug);

        public bool IsInfoEnabled => IsLevelEnabled(Logging.LogLevel.Info);

        public bool IsErrorEnabled => IsLevelEnabled(Logging.LogLevel.Error);

        public void Debug(string message)
        {
            if (IsDebugEnabled)
                LogLevel(Logging.LogLevel.Debug, message);
        }

        public void Info(string message)
        {
            if (IsInfoEnabled)
                LogLevel(Logging.LogLevel.Info, message);
        }

        public void Error(string message, Exception exception = null)
        {
            if (IsErrorEnabled)
                LogLevel(Logging.LogLevel.Error, message, exception);
        }

        public abstract void LogLevel(LogLevel level, string message, Exception exception = null);

        private bool IsLevelEnabled(LogLevel level)
        {
            return level >= logLevel;
        }
    }
}
