using RoeHack.Library.Core.Logging;
using System;

namespace RoeHack.Library.Core
{
    /// <summary>
    /// Provides an interface for communicating from the client (target) to the server (injector)
    /// </summary>
    public class ServerInterface : MarshalByRefObject, IIpcLoggerInterface
    {
        private readonly ILog logger;

        public ServerInterface()
        {
            logger = new ConsoleLogger("ServerInterface", LogLevel.Debug);
        }

        public event OnClosedHandle OnClosed;

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

        public void Close()
        {
            if (OnClosed != null)
                OnClosed();
        }
    }


    public delegate void OnClosedHandle();

    public class ServerInterfaceEventProxy : MarshalByRefObject
    {
        public event OnClosedHandle OnClosed;

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void Close()
        {
            if (OnClosed != null)
                OnClosed();
        }
    }
}
