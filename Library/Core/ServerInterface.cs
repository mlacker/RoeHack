using RoeHack.Library.Core.Logging;
using System;

namespace RoeHack.Library.Core
{
    /// <summary>
    /// Provides an interface for communicating from the client (target) to the server (injector)
    /// </summary>
    public class ServerInterface : MarshalByRefObject, IIpcLoggerInterface
    {
        public static ServerInterface Instance { get; private set; }

        private ILog logger;

        public ServerInterface()
        {
            Instance = this;
            logger = new ConsoleLogger ("ServerInterface", LogLevel.Debug);
        }

        public ILog Logger
        {
            set { logger = value; }
        }

        /// <summary>
        /// Called to confirm that the IPC channel is still open / host application has not closed
        /// </summary>
        public void Ping()
        {
        }

        public void WriteInternal(LogLevel level, string message, Exception exception)
        {
            logger.LogLevel(level, message, exception);
        }
    }
}
