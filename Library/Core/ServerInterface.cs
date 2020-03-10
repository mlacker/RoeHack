using System;

namespace RoeHack.Library.Core
{
    /// <summary>
    /// Provides an interface for communicating from the client (target) to the server (injector)
    /// </summary>
    public class ServerInterface : MarshalByRefObject
    {
        /// <summary>
        /// Called to confirm that the IPC channel is still open / host application has not closed
        /// </summary>
        public void Ping()
        {
        }

        public void IsInstalled(int processId)
        {
            Console.WriteLine($"Injector has injected payload into process {processId}.");
        }

        public void SendMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
