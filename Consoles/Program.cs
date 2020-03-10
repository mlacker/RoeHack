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

            try
            {
                injector.Inject("notepad");
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
