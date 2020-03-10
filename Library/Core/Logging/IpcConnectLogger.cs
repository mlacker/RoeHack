using System;

namespace RoeHack.Library.Core.Logging
{
    public class IpcConnectLogger : AbstractLogger
    {
        private readonly IIpcLoggerInterface server;

        public IpcConnectLogger(IIpcLoggerInterface server) : base(string.Empty, Logging.LogLevel.Debug)
        {
            this.server = server;
        }

        public override void LogLevel(LogLevel level, string message, Exception exception = null)
        {
            server.WriteInternal(level, message, exception);
        }
    }
}
