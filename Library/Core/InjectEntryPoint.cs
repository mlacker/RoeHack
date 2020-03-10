using EasyHook;
using RoeHack.Library.Core.Logging;
using System;
using System.Threading;

namespace RoeHack.Library.Core
{
    public class InjectEntryPoint : IEntryPoint, IDisposable
    {
        private readonly ServerInterface server;
        private readonly IpcConnectLogger logger;
        private IDirectXHooker hooker;

        public InjectEntryPoint(RemoteHooking.IContext context, Parameter parameter)
        {
            server = RemoteHooking.IpcConnectClient<ServerInterface>(parameter.ChannelName);
            logger = new IpcConnectLogger(server);

            server.Ping();

            hooker = new DirectXHook.DriectX9Hooker(parameter, logger);
        }

        public void Run(RemoteHooking.IContext context, Parameter parameter)
        {
            logger.Debug($"Injector has injected payload into process {RemoteHooking.GetCurrentProcessId()}.");

            hooker.Hooking();

            logger.Debug("All hooks installed");

            try
            {
                // Loop until injector closes (i.e. IPC fails)
                while (true)
                {
                    Thread.Sleep(1000);

                    server.Ping();
                }
            }
            catch
            {
                // Ping() or ReportMessages() will raise an exception if host is unreachable
            }

            Dispose();
        }

        public void Dispose()
        {
            // Remove hooks
            hooker.Dispose();

            // Finalise cleanup of hooks
            LocalHook.Release();
        }
    }
}
