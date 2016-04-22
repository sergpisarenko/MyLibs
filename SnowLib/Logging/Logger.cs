using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace SnowLib.Logging
{
    /// <summary>
    /// Класс, реализующий журналирование сообщений о событиях
    /// </summary>
    public sealed class Logger : IDisposable
    {
        #region Закрытые поля
        private const int defaultDelay = 200;
        private readonly string internalSource;
        private readonly ConcurrentQueue<LogItem> queue;
        private readonly SnowLib.Threading.DelayedTask task;
        private volatile ManualResetEventSlim processedEvent;
        #endregion

        #region Открытые поля
        public readonly string Folder;
        public readonly Encoding LogFileEncoding;
        public int MaxQueueSize;
        public event EventHandler<LogItem> Logged;
        #endregion

        #region Конструкторы и иницилизация
        public Logger(string folder)
        {
            this.Folder = folder;
            this.internalSource = this.GetType().FullName;
            this.queue = new ConcurrentQueue<LogItem>();
            this.LogFileEncoding = Encoding.UTF8;
            this.task = new SnowLib.Threading.DelayedTask(processQueue);
            this.MaxQueueSize = 100;
        }

        /// <summary>
        /// Останавливает обработку и освобождается связанные с ней ресурсы
        /// </summary>
        public void Dispose()
        {
            if (!this.task.IsDisposed)
            {
                this.task.Dispose();
                GC.SuppressFinalize(this);
            }
        }
        #endregion

        #region Основные методы работы
        /// <summary>
        /// Добавление сообщения в очередь 
        /// </summary>
        /// <param name="item">Сообщение</param>
        public void Log(LogItem item)
        {
            if (!this.task.IsDisposed)
            {
                if (this.queue.Count < this.MaxQueueSize)
                {
                    this.queue.Enqueue(item);
                    this.task.Run(defaultDelay);
                }
                else
                {
                    LogItem warningOverflow = new LogItem(LogSeverity.Warn, internalSource,
                        SnowLib.Messages.Format("LogQueueOverflow", this.MaxQueueSize), null);
                    logAppEvent(warningOverflow);
                    onLogged(warningOverflow);
                }
                onLogged(item);
            }
        }

        /// <summary>
        /// Добавление сообщения в очередь 
        /// </summary>
        /// <param name="severity">Важность</param>
        /// <param name="source">Источник</param>
        /// <param name="message">Основное сообщение</param>
        /// <param name="details">Детали</param>
        /// <returns>Элемент очереди сообщений</returns>
        public LogItem Log(LogSeverity severity, string source, string message, string details = null)
        {
            LogItem item = new LogItem(severity, source, message, details);
            Log(item);
            return item;
        }

        /// <summary>
        /// Добавление сообщения в очередь 
        /// </summary>
        /// <param name="severity">Важность</param>
        /// <param name="source">Источник</param>
        /// <param name="message">Основное сообщение</param>
        /// <param name="details">Детали</param>
        /// <returns>Элемент очереди сообщений</returns>
        public LogItem Log<T>(string source, string message, T ex) where T : Exception
        {
            LogItem item = new LogItem(LogSeverity.Error, source, message, getExceptionDescription(ex));
            Log(item);
            return item;
        }

        /// <summary>
        /// Добавление сообщения об ошибке в очередь 
        /// </summary>
        /// <param name="source">Источник</param>
        /// <param name="message">Основное сообщение</param>
        /// <param name="details">Детали</param>
        /// <returns>Элемент очереди сообщений</returns>
        public LogItem LogError(string source, string message, string details = null)
        {
            LogItem item = new LogItem(LogSeverity.Error, source, message, details);
            Log(item);
            return item;
        }

        /// <summary>
        /// Добавление предупреждающего сообщения в очередь 
        /// </summary>
        /// <param name="source">Источник</param>
        /// <param name="message">Основное сообщение</param>
        /// <param name="details">Детали</param>
        /// <returns>Элемент очереди сообщений</returns>
        public LogItem LogWarning(string source, string message, string details = null)
        {
            LogItem item = new LogItem(LogSeverity.Warn, source, message, details);
            Log(item);
            return item;
        }

        /// <summary>
        /// Добавление информационного сообщения в очередь 
        /// </summary>
        /// <param name="source">Источник</param>
        /// <param name="message">Основное сообщение</param>
        /// <param name="details">Детали</param>
        /// <returns>Элемент очереди сообщений</returns>
        public LogItem LogInfo(string source, string message, string details = null)
        {
            LogItem item = new LogItem(LogSeverity.Info, source, message, details);
            Log(item);
            return item;
        }

        /// <summary>
        /// Добавление отладочного сообщения в очередь 
        /// </summary>
        /// <param name="source">Источник</param>
        /// <param name="message">Основное сообщение</param>
        /// <param name="details">Детали</param>
        /// <returns>Элемент очереди сообщений</returns>
        public LogItem LogDebug(string source, string message, string details = null)
        {
            LogItem item = new LogItem(LogSeverity.Debug, source, message, details);
            Log(item);
            return item;
        }

        /// <summary>
        /// Запускает процесс обработки очереди и ожидает ее завершения, 
        /// но не более чем waitMilliseconds
        /// </summary>
        /// <param name="waitMilliseconds">Максимальное время ожидания</param>
        public void Flush(int waitMilliseconds = 1000)
        {
            if (!this.task.IsDisposed)
            {
                if (this.processedEvent != null)
                    this.processedEvent.Reset();
                else
                    this.processedEvent = new ManualResetEventSlim();
                this.task.Run();
                this.processedEvent.Wait(waitMilliseconds);
            }
        }
        #endregion

        #region Внутренние закрытые методы
        private void onLogged(LogItem item)
        {
            EventHandler<LogItem> handler = Volatile.Read(ref this.Logged);
            if (handler != null)
                handler(this, item);
        }

        private void logAppEvent(LogItem item)
        {
            EventLogEntryType entryType;
            switch(item.Severity)
            {
                case LogSeverity.Warn:
                    entryType = EventLogEntryType.Warning;
                    break;
                case LogSeverity.Error:
                    entryType = EventLogEntryType.Error;
                    break;
                default:
                    entryType = EventLogEntryType.Information;
                    break;
            }
            try
            {
                if (!EventLog.SourceExists(item.Source))
                    EventLog.CreateEventSource(item.Source, "Application");
                EventLog.WriteEntry(item.Source, item.Message, entryType, 0, 0,
                    String.IsNullOrEmpty(item.Details) ? null : Encoding.UTF8.GetBytes(item.Details));
                onLogged(item);
            }
            catch { }
        }

        private void processQueue()
        {
            string path = this.Folder;
            LogItem currItem = null, prevItem = null;
            ManualResetEventSlim pe = this.processedEvent;
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                StringBuilder sb = new StringBuilder(1024);
                while (this.queue.TryDequeue(out currItem))
                {
                    if (prevItem!=null && prevItem.CreatedAt != currItem.CreatedAt)
                    {
                        path = Path.Combine(this.Folder, prevItem.CreatedAt.ToString("yyyyMMdd") + ".txt");
                        File.AppendAllText(path, sb.ToString(), this.LogFileEncoding);
                        sb.Clear();                                                
                    }
                    currItem.AppendLine(sb);
                    prevItem = currItem;
                }
                if (prevItem !=null)
                {
                    path = Path.Combine(this.Folder, prevItem.CreatedAt.ToString("yyyyMMdd") + ".txt");
                    File.AppendAllText(path, sb.ToString(), this.LogFileEncoding);
                }
            }
            catch (IOException ioex)
            {
                if (currItem != null)
                    this.queue.Enqueue(currItem);
                LogItem errorIO = new LogItem(LogSeverity.Error, this.internalSource,
                    Messages.Format("ErrorWritingToTextLog", path), getExceptionDescription(ioex));
                logAppEvent(errorIO);
            }
            catch (Exception ex)
            {
                LogItem error = new LogItem(LogSeverity.Error, this.internalSource,
                    Messages.Get("ErrorProcessingLogQueue"), getExceptionDescription(ex));
                logAppEvent(error);
            }
            if (pe != null)
                pe.Set();
        }

        private string getExceptionDescription(Exception ex)
        {
            return ex.Message;
        }
        #endregion
    }
}
