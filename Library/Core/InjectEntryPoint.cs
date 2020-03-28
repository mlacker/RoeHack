using EasyHook;
using RoeHack.Library.Core.Logging;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;

namespace RoeHack.Library.Core
{
    public class InjectEntryPoint : IEntryPoint, IDisposable
    {
        private readonly ServerInterface server;
        private readonly ClientInterfaceEventProxy proxy;
        private readonly IpcConnectLogger logger;
        private readonly IDirectXHooker hooker;
        private bool isClosed = false;

        public InjectEntryPoint(RemoteHooking.IContext context, Parameter parameter)
        {
            server = RemoteHooking.IpcConnectClient<ServerInterface>(parameter.ChannelName);
            var channel = new IpcServerChannel(
                parameter.ChannelName,
                parameter.ChannelName + Guid.NewGuid().ToString("N"),
                new BinaryServerFormatterSinkProvider()
                {
                    TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full
                });
            ChannelServices.RegisterChannel(channel, false);

            proxy = new ClientInterfaceEventProxy();

            logger = new IpcConnectLogger(server);

            server.Ping();

            hooker = GetHooker(parameter);
        }

        public void Run(RemoteHooking.IContext context, Parameter parameter)
        {
            logger.Info($"Injector has injected payload into process {RemoteHooking.GetCurrentProcessId()}.");

            if (hooker != null)
            {
                hooker.Hooking();

                server.OnClosed += proxy.Close;
                proxy.OnClosed += OnClosed;

                logger.Debug("All hooks installed");

                BlockedCheckStatus();

                Dispose();
            }

            logger.Info("Injection already detached.");

            // Waiting the message send to client
            Thread.Sleep(300);
        }

        public void Dispose()
        {
            // Remove hooks
            hooker.Dispose();

            // Finalise cleanup of hooks
            LocalHook.Release();

            logger.Debug("Resources has benn released.");
        }

        /// <summary>
        /// Loop until injector closes (i.e. IPC fails)
        /// </summary>
        private void BlockedCheckStatus()
        {
            try
            {
                while (!isClosed)
                {
                    Thread.Sleep(1000);

                    server.Ping();
                }

                server.OnClosed -= proxy.Close;
            }
            catch
            {
                // Ping() will raise an exception if host is unreachable
            }
        }

        private void OnClosed()
        {
            isClosed = true;
        }

        private IDirectXHooker GetHooker(Parameter parameter)
        {
            IDirectXHooker hooker = null;
            switch (parameter.DirectXVersion)
            {
                case DirectXVersion.D3D9:
                    hooker = new DirectXHooker.DriectX9Hooker(parameter, logger);
                    break;
                case DirectXVersion.D3D11:
                    hooker = new DirectXHooker.DriectX11Hooker(parameter);
                    break;
                case DirectXVersion.D3D12:
                    hooker = new DirectXHooker.DriectX12Hooker(parameter, logger);
                    break;
                default:
                    logger.Error($"Unknown {parameter.DirectXVersion} version of DirectX.");
                    break;
            }

            return hooker;
        }
    }
}
