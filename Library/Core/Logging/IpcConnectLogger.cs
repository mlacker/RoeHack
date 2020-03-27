using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoeHack.Library.Core.Logging
{
    public class IpcConnectLogger : AbstractLogger
    {
        private readonly IIpcLoggerInterface server;
        private IList<LogEntry> currentLogs = new List<LogEntry>();
        private IList<LogEntry> swapLogs = new List<LogEntry>();

        public IpcConnectLogger(IIpcLoggerInterface server)
        {
            this.server = server;

            Task.Factory.StartNew(SendLogsAndSwap, TaskCreationOptions.LongRunning);
        }

        public override void LogLevel(LogLevel level, string message, Exception exception = null)
        {
            currentLogs.Add(new LogEntry(level, message, exception));
        }

        private async Task SendLogsAndSwap()
        {
            try
            {
                while (true)
                {
                    if (currentLogs.Count > 0)
                    {
                        swapLogs = Interlocked.Exchange(ref currentLogs, swapLogs);

                        server.SendLogs(swapLogs.ToArray());

                        swapLogs.Clear();
                    }

                    await Task.Delay(100);
                }
            }
            catch (System.Runtime.Remoting.RemotingException)
            {
            }
        }
    }
}
