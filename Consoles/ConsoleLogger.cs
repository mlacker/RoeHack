using RoeHack.Library.Core.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoeHack.Consoles.Logging
{
    public class ConsoleLogger : AbstractLogger
    {
        private static readonly Dictionary<LogLevel, ConsoleColor> colors = new Dictionary<LogLevel, ConsoleColor>
        {
            { Library.Core.Logging.LogLevel.Error, ConsoleColor.Red },
            { Library.Core.Logging.LogLevel.Info, ConsoleColor.White },
            { Library.Core.Logging.LogLevel.Debug, ConsoleColor.Gray }
        };

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
