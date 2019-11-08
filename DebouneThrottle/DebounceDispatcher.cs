using System;
using System.Threading.Tasks;

namespace DebouneThrottle
{
    /// <summary>
    /// The debounce dispatcher delays the invoking of Action until the calls stopped for a predetermined interval
    /// </summary>
    public class DebounceDispatcher : DebounceDispatcher<bool>
    {
        #region --- ctor ---
        /// <summary>
        /// The debounce dispatcher delays the invoking of Action until the calls stopped for a predetermined interval
        /// </summary>
        /// <param name="interval">The interval in milliseconds that must elapse after the last call so that a Action to be invoked</param>
        public DebounceDispatcher(int interval) : base(interval)
        {
        }
        #endregion

        #region --- public methods ---
        /// <summary>
        /// The call of the Action that will be invoked after waiting for the time interval if there are no more calls in this interval
        /// </summary>
        /// <param name="action">Action that will be invoked after waiting for the time interval</param>
        /// <returns>Task that will complete when Action will be invoked</returns>
        public Task DebounceAsync(Func<Task> action)
        {
            return base.DebounceAsync(async () =>
            {
                await action.Invoke();
                return true;
            });
        }
        /// <summary>
        /// The call of the Action that will be invoked after waiting for the time interval if there are no more calls in this interval
        /// </summary>
        /// <param name="action">Action that will be invoked after waiting for the time interval</param>
        public void Debounce(Action action)
        {
            Func<Task<bool>> actionAsync = () => Task.Run(() =>
            {
                action.Invoke();
                return true;
            });

            DebounceAsync(actionAsync);
        }
        #endregion
    }

    /// <summary>
    /// The debounce dispatcher delays the invoking of Function until the calls stopped for a predetermined interval
    /// </summary>
    /// <typeparam name="T">The return Type of the Tasks. All tasks will return the same value if the invoking occurs once</typeparam>
    public class DebounceDispatcher<T>
    {
        #region --- private fields ---
        private DateTime lastInvokeTime;
        private readonly int interval;
        private Func<Task<T>> functToInvoke;
        private object locker = new object();
        private bool busy;
        private Task<T> waitingTask;
        #endregion

        #region --- ctor ---
        /// <summary>
        /// The debounce dispatcher delays the invoking of Function until the calls stopped for a predetermined interval
        /// </summary>
        /// <param name="interval">The interval in milliseconds that must elapse after the last call so that a Function to be invoked</param>
        public DebounceDispatcher(int interval)
        {
            this.interval = interval;
        }
        #endregion

        #region --- public methods ---
        /// <summary>
        /// The call of the Function that will be invoked after waiting for the time interval if there are no more calls in this interval
        /// </summary>
        /// <param name="functToInvoke">Function that will be invoked after waiting for the time interval</param>
        /// <returns>Task with a result that will complete when Function will be invoked. All tasks will return the same value if the invoking occurs once</returns>
        public Task<T> DebounceAsync(Func<Task<T>> functToInvoke)
        {
            lock (locker)
            {
                this.functToInvoke = functToInvoke;
                this.lastInvokeTime = DateTime.UtcNow;
                if (busy)
                {
                    return waitingTask;
                }

                busy = true;
                waitingTask = Task.Run(() =>
                {
                    do
                    {
                        int delay = (int)(interval - (DateTime.UtcNow - lastInvokeTime).TotalMilliseconds);
                        Task.Delay(delay).Wait();
                    } while ((DateTime.UtcNow - lastInvokeTime).TotalMilliseconds < interval);

                    T res;
                    try
                    {
                        res = this.functToInvoke.Invoke().Result;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        lock (locker)
                        {
                            busy = false;
                        }
                    }

                    return res;
                });
                return waitingTask;
            }
        }
        #endregion
    }
}
