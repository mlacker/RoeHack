using EasyHook;
using RoeHack.Library.Core;
using System;
using System.Diagnostics;
using System.Linq;

namespace RoeHack.Forms
{
    class SampleInject
    {
        public void Main(string processName)
        {
            var process = Process.GetProcessesByName(processName).Single();

            string injectionLibrary = typeof(InjectEntryPoint).Assembly.Location;

            string channelName = null;
            RemoteHooking.IpcCreateServer<ServerInterface>(
                ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton);

            var parameter = new Parameter()
            {
                ChannelName = channelName
            };

            try
            {
                Console.WriteLine($"Attemption to inject into process {process.Id}");

                RemoteHooking.Inject(
                    process.Id,
                    injectionLibrary,
                    injectionLibrary,
                    parameter
                    );
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was an error while injecting into target:");
                Console.ResetColor();
                Console.WriteLine(ex.ToString());
            }

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("<Press any key to exit>");
            Console.ResetColor();
            Console.ReadKey();
        }
    }
}
