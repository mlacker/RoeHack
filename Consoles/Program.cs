using RoeHack.Forms;
using RoeHack.Library.Core;
using System;

namespace FileMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            Injector injector = new Injector();

            var processName = "Europa_Client";

            try
            {
                injector.Inject(processName);
            }
            catch (AppException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.WriteLine(ex.ToString());
            }

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("<Press any key to exit>");
            Console.ResetColor();
            Console.ReadKey(true);
        }
    }
}
