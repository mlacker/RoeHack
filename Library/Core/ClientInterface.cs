using RoeHack.Library.Core.Logging;
using System;

namespace RoeHack.Library.Core
{
    public delegate void OnClosedHandle();

    /// <summary>
    /// Provides an interface for communicating from the client (target) to the server (injector)
    /// </summary>
    public interface ClientInterface 
    {
        event OnClosedHandle OnClosed;

        void Close();
    }

    public class ClientInterfaceEventProxy : MarshalByRefObject
    {
        public event OnClosedHandle OnClosed;

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void Close()
        {
            OnClosed?.Invoke();
        }
    }
}
