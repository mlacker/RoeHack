using RoeHack.Library.Core.Logging;
using System;
using System.Text;
using System.Windows.Forms;

namespace RoeHack.Forms
{
    public class TextboxLogger : AbstractLogger
    {
        public TextboxLogger()
        {
            // Uncomment if thread exception occurs
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        public TextBox Textbox { get; set; }

        public int MaxLength { get => Textbox.MaxLength; }

        public override void LogLevel(LogLevel level, string message, Exception exception = null)
        {
            var text = Textbox.Text;
            var stringBuilder = new StringBuilder(
                text,
                Math.Max(text.Length - MaxLength, 0),
                Math.Min(text.Length, MaxLength),
                MaxLength);

            stringBuilder.AppendLine();
            stringBuilder.Append($"[{level.ToString().ToUpper()}]".PadRight(8));
            stringBuilder.Append(message);

            if (exception != null)
            {
                stringBuilder.Append(Environment.NewLine)
                    .Append(exception);
            }

            Textbox.Text = stringBuilder.ToString();

            Textbox.SelectionStart = Textbox.Text.Length;
            Textbox.ScrollToCaret();
        }
    }
}
