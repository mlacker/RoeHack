using RoeHack.Library.Core.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoeHack.Forms
{
    public class TextboxLogger : AbstractLogger
    {
        private IList<LogEntry> currentLogs = new List<LogEntry>();
        private IList<LogEntry> swapLogs = new List<LogEntry>();

        public TextboxLogger()
        {
            // Uncomment if thread exception occurs
            Control.CheckForIllegalCrossThreadCalls = false;

            Task.Factory.StartNew(FlushLogs, TaskCreationOptions.LongRunning);
        }

        public TextBox Textbox { get; set; }
        public int MaxLength { get => 6000; }

        public override void LogLevel(LogLevel level, string message, Exception exception = null)
        {
            currentLogs.Add(new LogEntry(level, message, exception));
        }

        private async Task FlushLogs()
        {
            while (true)
            {
                try
                {
                    if (currentLogs.Count > 0)
                    {
                        swapLogs = Interlocked.Exchange(ref currentLogs, swapLogs);

                        Flush(swapLogs);

                        swapLogs.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                await Task.Delay(100);
            }
        }

        private void Flush(IList<LogEntry> logs)
        {
            var stringBuilder = new StringBuilder(MaxLength, MaxLength);

            for (int i = logs.Count - 1; i >= 0; i--)
            {
                LogEntry log = logs[i];

                stringBuilder.Append($"[{log.Level.ToString().ToUpper(),-5}] ");
                stringBuilder.Append(log.Message);

                if (log.Exception != null)
                {
                    stringBuilder.Append(Environment.NewLine)
                        .Append(log.Exception);
                }

                stringBuilder.AppendLine();

                // drop logs, if text buffer overflow
                // i don't have any batter idea
                if (stringBuilder.Length + 800> stringBuilder.MaxCapacity)
                    break;
            }

            stringBuilder.Append(Textbox.Text, 0, Math.Min(Textbox.Text.Length, MaxLength - stringBuilder.Length));

            Textbox.Text = stringBuilder.ToString();
        }
    }
}
