using System;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;

namespace SnowLib.WCF
{
    /// <summary>
    /// Класс для поддержки дуплексного соединения с сервисом в клиенте
    /// </summary>
    /// <typeparam name="T">Тип прямого контракта сервиса</typeparam>
    public class DuplexConnection<T> where T : class
    {
        #region Открытые поля и методы
        public T Client
        {
            get 
            {
                T cli = Volatile.Read(ref this.client);
                if (cli == null)
                    throw new InvalidOperationException(Messages.Get("NoClientConnection"));
                return this.client; 
            }
        }
        public readonly string ConnectionString;
        public int RetrySeconds;
        #endregion
        
        #region Закрытые поля
        private T client;
        private readonly Func<object> getCallback;
        private readonly Func<T, object> initAsync;
        private readonly SendOrPostCallback updateUI;
        private readonly SynchronizationContext syncContext;
        private int opening;
        private Timer retryTimer;
        #endregion

        #region Конструкторы, иницилизация и завершение
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connectionString">Строка подключения к сервису</param>
        /// <param name="getCallbackInstance">Метод получения экземпляра объекта, реализующено интерфейс обратного вызова для контракта T</param>
        /// <param name="onInitAsync">Метод, вызываемый после успешного создания подключения (подписка на события от сервиса и.т.п)</param>
        public DuplexConnection(string connectionString,
            Func<object> getCallbackInstance, 
            Func<T, object> onInitAsync, SendOrPostCallback onUpdateUI)
        {
            if (getCallbackInstance == null)
                throw new ArgumentNullException("getCallbackInstance");
            this.ConnectionString = connectionString;
            this.getCallback = getCallbackInstance;
            this.initAsync = onInitAsync;
            this.updateUI = onUpdateUI;
            this.syncContext = SynchronizationContext.Current;
            this.RetrySeconds = 10;
            this.retryTimer = new Timer(this.open, null, 0, Timeout.Infinite);
        }

        /// <summary>
        /// Закрыте подключения
        /// </summary>
        public void Close()
        {
            while (Interlocked.CompareExchange(ref this.opening, 1, 0) != 0)
                Thread.Sleep(100);
            if (this.retryTimer != null)
            {
                this.retryTimer.Dispose();
                this.retryTimer = null;
            }
            ServiceClient.Close(this.client);
            Interlocked.Exchange(ref this.opening, 0);
        }
        #endregion

        #region Закрытые методы
        private void open(object state)
        {
            if (Interlocked.CompareExchange(ref this.opening, 1, 0) == 0)
            {
                Exception exception = null;
                if (this.retryTimer != null)
                    this.retryTimer.Change(Timeout.Infinite, Timeout.Infinite);
                T newClient = null;
                try
                {
                    newClient = ServiceClient.Get<T>(this.ConnectionString, this.getCallback());
                    object initResult = null;
                    if (this.initAsync != null)
                        initResult = this.initAsync(newClient);
                    ICommunicationObject comm = (ICommunicationObject)newClient;
                    comm.Faulted += commFaulted;
                    if (this.retryTimer != null)
                    {
                        this.retryTimer.Dispose();
                        this.retryTimer = null;
                    }
                    updateUIState(initResult);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    ServiceClient.Close(newClient);
                    newClient = null;
                    if (this.retryTimer == null)
                        this.retryTimer = new Timer(open);
                    this.retryTimer.Change(RetrySeconds * 1000, Timeout.Infinite);
                    updateUIState(ex);
                }
                T oldClient = Interlocked.Exchange(ref this.client, newClient);
                ServiceClient.Close(oldClient);
                Interlocked.Exchange(ref this.opening, 0);
            }
        }

        private void updateUIState(object state)
        {
            if (this.updateUI!=null)
            {
                if (this.syncContext == null)
                    this.updateUI(state);
                else
                    this.syncContext.Post(this.updateUI, state);
            }
        }

        private void commFaulted(object sender, EventArgs e)
        {
            ServiceClient.Close(sender);
            open(null);
        }
        #endregion
    }
}
