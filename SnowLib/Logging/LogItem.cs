using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnowLib.Extensions;

namespace SnowLib.Logging
{
    /// <summary>
    /// Элемент журнала сообщений, сохраняемый в 
    /// текстовый файл или выводимый на экран
    /// </summary>
    public class LogItem : EventArgs
    {
        public const int MaxLineSourceLength = 128;
        public const int MaxLineMessageLength = 512;
        public const int MaxLineDetailsLength = 1024;

        /// <summary>
        /// Дата создания сообщения
        /// </summary>
        public readonly DateTime CreatedAt;
        /// <summary>
        /// Важность сообщения
        /// </summary>
        public readonly LogSeverity Severity;
        /// <summary>
        /// Источник сообщения
        /// </summary>
        public readonly string Source;
        /// <summary>
        /// Основной текст сообщения
        /// </summary>
        public readonly string Message;
        /// <summary>
        /// Детали сообщения
        /// </summary>
        public readonly string Details;

        public LogItem(LogSeverity severity, string source, string message, string details)
        {
            this.CreatedAt = DateTime.Now;
            this.Severity = severity;
            this.Source = source;
            this.Message = message;
            this.Details = details;
        }

        /// <summary>
        /// Преобразовать сообщение в строку, добавив к StringBuilder
        /// </summary>
        /// <param name="dest">Приемник</param>
        public void AppendLine(StringBuilder dest)
        {
            dest.Append(this.CreatedAt.ToString("HH:mm:ss"));
            dest.Append('\t');
            dest.Append(this.Severity.ToString());
            dest.Append('\t');
            dest.Append(this.Source.UnformatAndTrim(MaxLineSourceLength));
            dest.Append('\t');
            dest.Append(this.Message.UnformatAndTrim(MaxLineMessageLength));
            dest.Append('\t');
            dest.Append(this.Details.UnformatAndTrim(MaxLineDetailsLength));
            dest.Append(Environment.NewLine);
        }
    }
}
