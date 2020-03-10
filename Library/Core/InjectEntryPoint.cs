using EasyHook;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace RoeHack.Library.Core
{
    public class InjectEntryPoint : IEntryPoint, IDisposable
    {
        private readonly ServerInterface server;
        private IDirectXHooker hooker;

        public object NativeMethods { get; private set; }

        public InjectEntryPoint(RemoteHooking.IContext context, Parameter parameter)
        {
            server = RemoteHooking.IpcConnectClient<ServerInterface>(parameter.ChannelName);

            server.Ping();

            hooker = new DirectXHook.DriectX9Hooker(parameter);
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        public void Run(RemoteHooking.IContext context, Parameter parameter)
        {


            var d3D9Loaded = GetModuleHandle("d3d9.dll");
            var d3D10Loaded = GetModuleHandle("d3d10.dll");
            var d3D10_1Loaded = GetModuleHandle("d3d10_1.dll");
            var d3D11Loaded = GetModuleHandle("d3d11.dll");
            var d3D11_1Loaded = GetModuleHandle("d3d11_1.dll");
            if (d3D9Loaded!=IntPtr.Zero)
            {
                server.SendMessage("使用dx9");
            }
            if (d3D10Loaded != IntPtr.Zero)
            {
                server.SendMessage("使用dx10");
            }
            if (d3D10_1Loaded != IntPtr.Zero)
            {
                server.SendMessage("使用dx10_1");
            }
            if (d3D11Loaded != IntPtr.Zero)
            {
                server.SendMessage("使用dx11_1");
            }
            if (d3D11_1Loaded != IntPtr.Zero)
            {
                server.SendMessage("使用dx11_1");
            }

            server.IsInstalled(RemoteHooking.GetCurrentProcessId());

            hooker.Hooking();

            server.SendMessage("All hooks installed");



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
