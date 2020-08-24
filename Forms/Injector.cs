using EasyHook;
using RoeHack.Library.Core;
using RoeHack.Library.Core.Logging;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace RoeHack.Forms
{
    public class Injector
    {
        private readonly ILog logger;
        private ClientInterface server;

        public Injector(ILog logger)
        {
            this.logger = logger;
        }

        public Parameter Parameter { get; private set; } = new Parameter();

        public bool Injected { get; private set; }

        public void Inject(string processName)
        {
            if (Injected)
                return;

            var process = Process.GetProcessesByName(processName)
                .FirstOrDefault() ?? throw new AppException($"无法找到正在运行的 {processName} 应用.");

            if (!RemoteHooking.IsAdministrator)
            {
                // Please run the program as an administrator
                throw new AppException("请以管理员身份运行程序.");
            }

            if (Parameter.DirectXVersion == DirectXVersion.Unkonwn)
            {
                Parameter.DirectXVersion = GetCurrentDirectXVerion(process);
            }
            logger.Info($"Current DirectX version is {Parameter.DirectXVersion}.");

            if (Parameter.DirectXVersion == DirectXVersion.D3D9)
            {
                Parameter.ProcAddress = GetAddress();
            }

            var injectionLibrary = typeof(InjectEntryPoint).Assembly.Location;

            var server = new ServerInterface(logger);

            string channelName = null;
            RemoteHooking.IpcCreateServer(
                ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton, server);
            Parameter.ChannelName = channelName;

            try
            {
                logger.Info($"Attemption to inject into process {process.ProcessName}({process.Id})");

                RemoteHooking.Inject(
                    process.Id,
                    injectionLibrary,
                    injectionLibrary,
                    Parameter
                    );

                Injected = true;
                this.server = server;
            }
            catch (Exception ex)
            {
                logger.Error("There was an error while injecting into target:", ex);
            }
        }

        public void Close()
        {
            if (!Injected)
                return;

            try
            {
                server.Close();
            }
            catch (System.Runtime.Remoting.RemotingException)
            {
            }

            Injected = false;
        }

        private DirectXVersion GetCurrentDirectXVerion(Process process)
        {
            var versions = new HashSet<DirectXVersion>();
            // If run as x64 mode, cannot found d3d module.
            foreach (var module in process.Modules.Cast<ProcessModule>())
            {
                switch (module.ModuleName)
                {
                    case "d3d9.dll":
                        versions.Add(DirectXVersion.D3D9);
                        break;
                    case "d3d10.dll":
                        versions.Add(DirectXVersion.D3D10);
                        break;
                    case "d3d10_1.dll":
                        versions.Add(DirectXVersion.D3D10);
                        break;
                    case "d3d11.dll":
                        versions.Add(DirectXVersion.D3D11);
                        break;
                    case "d3d12.dll":
                        versions.Add(DirectXVersion.D3D12);
                        break;
                    default:
                        break;
                }
            }

            if (versions.Count() > 0)
            {
                logger.Debug($"The version of the process available has been detected: {string.Join(", ", versions)}.");
            }
            else
            {
                logger.Error($"The available version of the target process was not detected.");
                throw new AppException("目标进程未检测到可用的驱动版本");
            }

            return versions.OrderBy(m => m).LastOrDefault();
        }

        private List<IntPtr> GetAddress()
        {
            var address = new List<IntPtr>();

            using (var d3d = new Direct3D())
            using (var renderForm = new System.Windows.Forms.Form())
            using (var device = new Device(d3d, 0, DeviceType.NullReference, IntPtr.Zero, CreateFlags.HardwareVertexProcessing, new PresentParameters() { BackBufferWidth = 1, BackBufferHeight = 1, DeviceWindowHandle = renderForm.Handle }))
            {
                IntPtr vTable = Marshal.ReadIntPtr(device.NativePointer);
                for (int i = 0; i < 119; i++)
                    address.Add(Marshal.ReadIntPtr(vTable, i * IntPtr.Size));
            }

            return address;
        }
    }
}
