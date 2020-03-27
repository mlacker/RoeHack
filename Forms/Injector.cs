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
                .SingleOrDefault() ?? throw new AppException($"无法找到正在运行的 {processName} 应用.");

            if (!RemoteHooking.IsAdministrator)
            {
                throw new AppException("请以管理员身份运行程序.");
            }

            var injectionLibrary = typeof(InjectEntryPoint).Assembly.Location;

            var server = new ServerInterface(logger);

            string channelName = null;
            RemoteHooking.IpcCreateServer(
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

                Injected = true;
                this.server = server;
            }
            catch (Exception ex)
            {
                logger.Error("向目标注入时有一个错误：", ex);
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
    }
}
