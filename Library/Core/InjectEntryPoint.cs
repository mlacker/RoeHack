using EasyHook;
using RoeHack.Library.Core.Logging;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace RoeHack.Library.Core
{
    public class InjectEntryPoint : IEntryPoint, IDisposable
    {
        private readonly ServerInterface server;
        private readonly IpcConnectLogger logger;
        IDirectXHooker hooker;
        public InjectEntryPoint(RemoteHooking.IContext context, Parameter parameter)
        {
            server = RemoteHooking.IpcConnectClient<ServerInterface>(parameter.ChannelName);
            logger = new IpcConnectLogger(server);

            server.Ping();

            hooker = GetCurrentVerionDirectX(parameter);
        }

        public void Run(RemoteHooking.IContext context, Parameter parameter)
        {
            logger.Debug($"Injector has injected payload into process {RemoteHooking.GetCurrentProcessId()}.");

            hooker?.Hooking();

            logger.Debug("All hooks installed");

            BlockedCheckStatus();

            Dispose();
        }

        public void Dispose()
        {
            // Remove hooks
            hooker?.Dispose();

            // Finalise cleanup of hooks
            LocalHook.Release();
        }

        /// <summary>
        /// Loop until injector closes (i.e. IPC fails)
        /// </summary>
        private void BlockedCheckStatus()
        {
            try
            {
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
                { "11_1", "d3d11_1.dll" },
                { "12", "d3d12.dll" }
            };
            var currentVersion = versions
                .Where(it => GetModuleHandle(it.Value) != null)
                .Select(it => it.Key)
                .FirstOrDefault(); ;

            IDirectXHooker hooker = null;
            if (currentVersion == null)
            {
                logger.Error($"未检测到目标进程对应的 directx 版本.");
            }
            else
            {
                logger.Debug($"当前驱动版本为 {currentVersion}.");

                switch (currentVersion)
                {
                    case "9":
                        hooker = new DirectXHooker.DriectX9Hooker(parameter, logger);
                        break;
                    case "11":
                    case "11_1":
                        hooker = new DirectXHooker.DriectX11Hooker(parameter);
                        break;
                    default:
                        logger.Error($"DirectX{currentVersion} 版本尚未实现，请联系作者韦大神！");
                        break;
                }
            }

            return hooker;
        }
    }
}
