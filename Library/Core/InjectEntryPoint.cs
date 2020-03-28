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

            hooker = GetCurrentVerionDirectX(parameter);
        }

        public void Run(RemoteHooking.IContext context, Parameter parameter)
        {
            logger.Info($"Injector has injected payload into process {RemoteHooking.GetCurrentProcessId()}.");

            hooker?.Hooking();

            server.OnClosed += proxy.Close;
            proxy.OnClosed += OnClosed;

            logger.Debug("All hooks installed");

            BlockedCheckStatus();

            Dispose();

            logger.Info("Injection already detached.");

            // Waiting the message send to client
            Thread.Sleep(300);
        }

        public void Dispose()
        {
            // Remove hooks
            hooker?.Dispose();

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

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        private IDirectXHooker GetCurrentVerionDirectX(Parameter parameter)
        {
            var versions = new System.Collections.Generic.Dictionary<string, string>()
            {
                { "9", "d3d9.dll" },
                { "10", "d3d10.dll" },
                { "10_1", "d3d10_1.dll" },
                { "11", "d3d11.dll" },
                { "12", "d3d12.dll" }
            };
            var checkedVersions = versions
                .Where(it => GetModuleHandle(it.Value) != IntPtr.Zero)
                .Select(it => it.Key);

            if (checkedVersions.Count() > 0)
                logger.Debug($"The driver version of the process available has been detected：{string.Join(", ", checkedVersions)}.");

            var currentVersion = checkedVersions
                .LastOrDefault();

            IDirectXHooker hooker = null;
            if (currentVersion != null)
            {
                logger.Debug($"Current DirectX version is {currentVersion}.");

                switch (currentVersion)
                {
                    case "9":
                        hooker = new DirectXHooker.DriectX9Hooker(parameter, logger);
                        break;
                    case "11":
                        hooker = new DirectXHooker.DriectX11Hooker(parameter);
                        break;
                    case "12":
                        hooker = new DirectXHooker.DriectX12Hooker(parameter, logger);
                        break;
                    default:
                        logger.Error($"Unknown {currentVersion} version of DirectX.");
                        break;
                }
            }
            else
            {
                logger.Error($"The DirectX version of the target process was not detected.");
            }

            return hooker;
        }
    }
}
