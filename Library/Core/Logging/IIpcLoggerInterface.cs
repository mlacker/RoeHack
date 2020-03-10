using System;

namespace RoeHack.Library.Core.Logging
{
    public interface IIpcLoggerInterface
    {
        void WriteInternal(LogLevel level, string message, Exception exception);
    }
}
