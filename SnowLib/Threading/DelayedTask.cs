using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SnowLib.Threading
{
    /// <summary>
    /// Delayed background non-reenterable task 
    /// </summary>
    public class DelayedTask
    {
        #region Private fields
        private const int retryInterval = 300;
        private const int stateFree = 0;
        private const int stateBusy = 1;
        private const int stateDisposed = 2;
        private int state;
        private readonly Timer timer;
        private readonly Action action;
        #endregion

        #region Public properties
        public bool IsBusy
        {
            get
            {
                return Volatile.Read(ref this.state) == stateBusy;
            }
        }

        public bool IsDisposed
        {
            get
            {
                return Volatile.Read(ref this.state) == stateDisposed;
            }
        }
        #endregion

        #region Constructors and initializers
        public DelayedTask(Action task)
        {
            if (task == null)
                throw new ArgumentNullException("task");
            this.timer = new Timer(timerCallback, null, Timeout.Infinite, Timeout.Infinite);
            this.action = task;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Try to run task immediately
        /// </summary>
        public void Run()
        {
            this.timer.Change(this.IsBusy ? retryInterval : 0, Timeout.Infinite);
        }

        /// <summary>
        /// Try to run task after dueMilliseconds
        /// </summary>
        /// <param name="dueMilliseconds">Run delay</param>
        public void Run(int dueMilliseconds)
        {
            this.timer.Change(dueMilliseconds, Timeout.Infinite);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Interlocked.Exchange(ref this.state, stateDisposed);
            this.timer.Dispose();
        }
        #endregion

        #region Private methods
        // main timer callback method
        private void timerCallback(object timerState)
        {
            int currState;
            if ((currState = Interlocked.CompareExchange(ref this.state, stateBusy, stateFree)) == stateFree)
            {
                this.action();
                Interlocked.CompareExchange(ref this.state, stateFree, stateBusy);
            }
            else if (currState == stateBusy)
            {
                try { this.timer.Change(retryInterval, Timeout.Infinite); }
                catch (ObjectDisposedException) { } // busy, try later
            }
        }
        #endregion
    }
}
