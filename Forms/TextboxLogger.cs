using RoeHack.Library.Core.Logging;
using System;
using System.Text;
using System.Windows.Forms;

namespace RoeHack.Forms
{
    public class TextboxLogger : AbstractLogger
    {
        private readonly TextBoxBase textbox;
        private readonly int maxLength;

        public TextboxLogger(TextBoxBase textbox)
        {
            this.textbox = textbox;
            this.maxLength = textbox.MaxLength;
        }

        public override void LogLevel(LogLevel level, string message, Exception exception = null)
        {
            var text = textbox.Text;
            var stringBuilder = new StringBuilder(
                text,
                Math.Max(text.Length - maxLength, 0), 
                Math.Min(text.Length, maxLength),
                maxLength);

            stringBuilder.AppendLine();
            stringBuilder.Append($"[{level.ToString().ToUpper()}]".PadRight(8));
            stringBuilder.Append(message);

            if (exception != null)
            {
                stringBuilder.Append(Environment.NewLine)
                    .Append(exception);
            }

            textbox.Text = stringBuilder.ToString();

            textbox.SelectionStart = textbox.Text.Length;
            textbox.ScrollToCaret();
        }
    }
}
