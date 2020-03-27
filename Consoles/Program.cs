using RoeHack.Forms;
using RoeHack.Library.Core;
using RoeHack.Library.Core.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace FileMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            Injector injector = new Injector();
            //var memTest = new MemTest();

            var processName = "Europa_Client";

            try
            {
                injector.Inject(processName);

                //memTest.Test(processName);
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

            try
            {
                injector.Close();
            }
            catch (System.Runtime.Remoting.RemotingException)
            {
            }

            Console.ReadKey(true);
        }
    }

    public class MemTest
    {
        private readonly ILog logger;

        public MemTest()
        {
            this.logger = new ConsoleLogger(nameof(MemTest), LogLevel.Debug);
        }

        public void Test(string processName)
        {
            var buffer = DumpMemory(processName);

            SaveDump(buffer);

            var sigEntities = new byte[] { 0x8B, 0x15, 0x00, 0x00, 0x00, 0x00, 0x3B, 0x04, 0x8A, 0x74, 0x00, 0x8B, 0x48, 0x08, 0x85, 0xC9 };
            var maskEntities= "xx????xxxx?xxxxx";

            var sigCamera = new byte[] { 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x11, 0xF3, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x8D, 0xB3, 0x00, 0x00, 0x00, 0x00 };
            var maskCamera = "xx????xxxx????xx??xx";
        }

        private const string FILENAME = "eof.dump";

        private void SaveDump(byte[] buffer)
        {
            using (var file = new FileStream(FILENAME, FileMode.Create))
            {
                file.Write(buffer, 0, buffer.Length);
            }
        }

        private byte[] ReadDump()
        {
            using (var file = new FileStream(FILENAME, FileMode.Open))
            {
                var buffer = new byte[file.Length];
                file.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        private byte[] DumpMemory(string processName)
        {
            var process = Process.GetProcessesByName(processName)
                .SingleOrDefault() ?? throw new AppException($"无法找到正在运行的 {processName} 应用.");

            var module = process.MainModule;
            var length = module.ModuleMemorySize;

            logger.Debug($"ModuleName: {module.ModuleName}, BaseAddress: {module.BaseAddress:X16}, MemorySize: {length:X8} ({length / 1024 / 1024}MB).");

            var buffer = new byte[length];
            var bufferAddress = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);

            logger.Debug($"Buffer Address: {bufferAddress:X16}");

            ReadProcessMemory(process.Handle, module.BaseAddress, bufferAddress, length, IntPtr.Zero);

            Console.WriteLine(string.Join(" ", buffer.Take(60).ToArray().Select(m => $"0x{m:X}")));

            return buffer;
        }

        private IntPtr ScanPattern(byte[] buffer, byte[] sig, string mask, int offset = 0)
        {
            var bufferAddress = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);

            for (int i = 0; i < buffer.Length - mask.Length + 1; i++)
            {
                if (buffer[i] == sig[0])
                {
                    for (int x = 0; ; x++)
                    {
                        if (mask[x] == 'x')
                        {
                            if (buffer[i + x] == sig[x])
                                continue;
                            else
                                break;
                        }
                        else if (mask[x] == 0x00)
                        {
                            return bufferAddress + i + offset;
                        }
                    }
                }
            }
            return IntPtr.Zero;
        }

        [DllImportAttribute("kernel32.dll", EntryPoint = "ReadProcessMemory")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, IntPtr lpNumberOfBytesRead);
    }

    struct Module
    {
        public string Name;
        public long Address;
        public int Size;

        public Module(string name, long address, int size)
        {
            Name = name;
            Address = address;
            Size = size;
        }

        public override string ToString()
        {
            return $"{Name}:\nBase Address: 0x{Address:X16}, MemorySize: 0x{Size:X16}";
        }
    }
}
