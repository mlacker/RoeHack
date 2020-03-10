using System;

namespace RoeHack.Library.Core
{
    /// <summary>
    /// Provides an interface for communicating from the client (target) to the server (injector)
    /// </summary>
    public class ServerInterface : MarshalByRefObject
    {
        public void IsInstalled(int clientPid)
        {
            Console.WriteLine($"injector has injected payload into process {clientPid}.");
        }

        /// <summary>
        /// Called to confirm that the IPC channel is still open / host application has not closed
        /// </summary>
        public void Ping()
        {
        }

        public void SendMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
