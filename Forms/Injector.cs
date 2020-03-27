using EasyHook;
using RoeHack.Library.Core;
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
        private bool injected = false;

        public Injector()
        {
            server = new ServerInterface();
            logger = new ConsoleLogger("Injector", LogLevel.Debug);
        }

        public Parameter Parameter { get; private set; } = new Parameter();

        public bool Injected { get => injected; }

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

                injected = true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("向目标注入时有一个错误：", ex);
            }
        }

        public void Close()
        {
            server.Close();

            injected = false;
        }
    }
}
