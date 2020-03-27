using System;

namespace RoeHack.Library.Core.Logging
{
    public interface IIpcLoggerInterface
    {
        void SendLogs(LogEntry[] logs);
    }
}
