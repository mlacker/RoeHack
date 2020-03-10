using System;
using System.Collections.Generic;
using System.Text;

namespace RoeHack.Library.Core.Logging
{
    public class ConsoleLogger : AbstractLogger
    {
        private static readonly Dictionary<LogLevel, ConsoleColor> colors = new Dictionary<LogLevel, ConsoleColor>
        {
            { Logging.LogLevel.Error, ConsoleColor.Red },
            { Logging.LogLevel.Info, ConsoleColor.White },
            { Logging.LogLevel.Debug, ConsoleColor.Gray }
        };

        public ConsoleLogger(string logName, LogLevel logLevel)
            : base(logName, logLevel)
        {
        }

        public override void LogLevel(LogLevel level, string message, Exception exception = null)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append($"[{level.ToString().ToUpper()}]".PadRight(8));
            stringBuilder.Append(message);

            if (exception != null)
            {
                stringBuilder.Append(Environment.NewLine)
                    .Append(exception);
            }

            Console.ForegroundColor = colors[level];
            Console.Out.WriteLine(stringBuilder);
            Console.ResetColor();
        }
    }
}
