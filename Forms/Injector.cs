using EasyHook;
using RoeHack.Library.Core;
using System;
using System.Diagnostics;
using System.Linq;

namespace RoeHack.Forms
{
    public class Injector
    {
        public Parameter Parameter { get; private set; } = new Parameter();

        public void Inject(string processName)
        {
            if (!RemoteHooking.IsAdministrator)
            {
                throw new AppException("请以管理员身份运行程序.");
            }

            var process = Process.GetProcessesByName(processName).SingleOrDefault();

            if (process == null)
            {
                throw new AppException($"无法找到正在运行的 {processName} 应用.");
            }

            var injectionLibrary = typeof(InjectEntryPoint).Assembly.Location;

            string channelName = null;
            RemoteHooking.IpcCreateServer<ServerInterface>(
                ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton);
            Parameter.ChannelName = channelName;

            try
            {
                Console.WriteLine($"Attemption to inject into process {process.Id}");

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
        }
    }
}
