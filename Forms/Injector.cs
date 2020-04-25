using EasyHook;
using RoeHack.Library.Core;
using RoeHack.Library.Core.LockHead;
using RoeHack.Library.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RoeHack.Forms
{
    public class Injector
    {
        private readonly ILog logger;
        private ClientInterface server;
        private readonly IDoLockHead DoLockHead;

        public Injector(ILog logger)
        {
            screenCapture = new Library.Core.LockHead.ScreenCapture();
            DoLockHead = new DoLockHead();
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

            // 锁头
            Detect(process.MainWindowHandle);
            screenCapture.SetForegroundWindow(process.MainWindowHandle);

        }

        IScreenCapture screenCapture;
        public void Detect(IntPtr pHandle)
        {
            Handle = pHandle;
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 70; //定时执行的时间精确到秒，那么Timer的时间间隔要小于1秒
            timer.Elapsed += new System.Timers.ElapsedEventHandler(RunNow);
            timer.Start();
        }
        bool isDetecting = false;
        IntPtr Handle;
        public void RunNow(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                //bool isForeground = screenCapture.CheckIsForeground(Handle);
                if (!isDetecting ) //如果相等则说明已经执行过了
                {
                    isDetecting = true;
                    int picWith = 120;
                    var image = screenCapture.CaptureWindow(Handle, picWith);
                    
                    DoLockHead.LockHead(image, picWith, picWith);
                    isDetecting = false;
                }
            }
            catch (Exception ex)
            {                                                                     
                isDetecting = false;
                logger.Error("cuowu", ex);
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
    }
}
