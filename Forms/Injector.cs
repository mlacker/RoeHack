using EasyHook;
using RoeHack.Library.Core;
using RoeHack.Library.Core.LockHead;
using RoeHack.Library.Core.Logging;
using System;
using System.Diagnostics;
using System.Linq;

namespace RoeHack.Forms
{
    public class Injector
    {
        private readonly ServerInterface server;
        private readonly ILog logger;
        private readonly IDoLockHead DoLockHead;

        public Injector()
        {
            server = new ServerInterface();
            logger = new ConsoleLogger("Injector", LogLevel.Debug);
            screenCapture = new Library.Core.LockHead.ScreenCapture();
            DoLockHead = new DoLockHead();
        }

        public Parameter Parameter { get; private set; } = new Parameter();

        public void Inject(string processName)
        {
            var process = Process.GetProcessesByName(processName)
                .SingleOrDefault() ?? throw new AppException($"无法找到正在运行的 {processName} 应用.");

            if (!RemoteHooking.IsAdministrator)
            {
                throw new AppException("请以管理员身份运行程序.");
            }

            var injectionLibrary = typeof(InjectEntryPoint).Assembly.Location;

            string channelName = null;
            RemoteHooking.IpcCreateServer<ServerInterface>(
                ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton, server);
            Parameter.ChannelName = channelName;

            try
            {
                logger.Info($"尝试注入目标进程 {process.ProcessName}({process.Id})");

                RemoteHooking.Inject(
                    process.Id,
                    injectionLibrary,
                    injectionLibrary,
                    Parameter
                    );
            }
            catch (Exception ex)
            {
                throw new ApplicationException("向目标注入时有一个错误：", ex);
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
            server.Close();
        }
    }
}
