using System;
using System.Threading.Tasks;

namespace DebounceThrottle
{
    /// <summary>
    /// The Throttle dispatcher provides one Action invoking at a specific time interval
    /// </summary>
    public class ThrottleDispatcher : ThrottleDispatcher<bool>
    {
        #region --- ctor ---
        /// <summary>
        /// The Throttle dispatcher provides one Action invoking at a specific time interval
        /// </summary>
        /// <param name="interval">The time interval when should be only one invoking</param>
        /// <param name="intervalFromStart">true - Countdown of the time interval from the beginning of the Action invoking. false - Countdown of the time interval from the complete of the Action invoking</param>
        /// <param name="resetIntervalOnException">true - if an error occurs, ignore call and do not wait for the time interval for next call</param>
        public ThrottleDispatcher(int interval, bool intervalFromStart = false, bool resetIntervalOnException = false)
            : base(interval, intervalFromStart, resetIntervalOnException)
        {
        }
        #endregion

        #region --- public methods ---
        /// <summary>
        /// Call the Action that will be invoked if it was the only one at a specific time interval 
        /// </summary>
        /// <param name="action">Action that will be invoked</param>
        /// <returns>Task that will complete when one of called Action will be invoked</returns>
        public Task ThrottleAsync(Func<Task> action)
        {
            return base.ThrottleAsync(() =>
            {
                action.Invoke().Wait();
                return Task.FromResult(true);
            });
        }
        /// <summary>
        /// Call the Action that will be invoked if it was the only one at a specific time interval
        /// </summary>
        /// <param name="action">Action that will be invoked</param>
        public void Throttle(Action action)
        {
            Func<Task<bool>> actionAsync = () => Task.Run(() =>
            {
                action.Invoke();
                return true;
            });

            ThrottleAsync(actionAsync);
        }
        #endregion
    }
    /// <summary>
    /// The Throttle dispatcher provides one Function invoking at a specific time interval
    /// </summary>
    /// <typeparam name="T">The return Type of the Tasks. All tasks will return the same value if the invoking occurs once</typeparam>
    public class ThrottleDispatcher<T>
    {
        #region --- private fields ---
        private Func<Task<T>> functToInvoke;
        private readonly int interval;
        private readonly bool isIntervalSinceInvokeTime;
        private readonly bool resetIntervalOnException;
        private readonly object locker = new object();
        private bool busy;
        private bool waiting;
        private Task<T> waitingTask;
        private Task intervalTask;
        private DateTime? invokeStartTime;
        #endregion

        #region --- ctor ---
        /// <summary>
        /// The Throttle dispatcher provides one Function invoking at a specific time interval
        /// </summary>
        /// <param name="interval">The time interval when should be only one invoking</param>
        /// <param name="intervalFromStart">true - Countdown of the time interval from the beginning of the Function invoking. false - Countdown of the time interval from the complete of the Function invoking</param>
        /// <param name="resetIntervalOnException">true - if an error occurs, ignore call and do not wait for the time interval for next call</param>
        public ThrottleDispatcher(int interval, bool isIntervalSinceInvokeTime = false, bool resetIntervalOnException = false)
        {
            this.interval = interval;
            this.isIntervalSinceInvokeTime = isIntervalSinceInvokeTime;
            this.resetIntervalOnException = resetIntervalOnException;
        }
        #endregion

        #region --- public methods ---
        /// <summary>
        /// Call the Function that will be invoked if it was the only one at a specific time interval
        /// </summary>
        /// <param name="functToInvoke">Function that will be invoked</param>
        /// <returns>Task with a result that will complete when one of called Function will be invoked</returns>
        public Task<T> ThrottleAsync(Func<Task<T>> functToInvoke)
        {
            lock (locker)
            {
                this.functToInvoke = functToInvoke;

                if (busy)
                {
                    return GetWaitingTask();
                }

                Task<T> actionTask = GetActionTask();

                intervalTask = Task.Run(() => ProcessInterval(actionTask));

                return actionTask;
            }
        }
        #endregion

        #region --- private methods ---
        private Task<T> GetActionTask()
        {
            busy = true;
            invokeStartTime = DateTime.UtcNow;
            Task<T> actionTask = this.functToInvoke.Invoke();
            return actionTask;
        }

        private Task<T> GetWaitingTask()
        {
            if (waiting)
            {
                return waitingTask;
            }

            waiting = true;
            waitingTask = Task.Run(() => ProcessWaitingTask());

            return waitingTask;
        }

        private Task<T> ProcessWaitingTask()
        {
            intervalTask.Wait();
            lock (locker)
            {
                waiting = false;
                return ThrottleAsync(this.functToInvoke);
            }
        }

        private void ProcessInterval(Task actionTask)
        {
            try
            {
                actionTask.Wait();
                DelayToNextProcess();
            }
            catch
            {
                if (!resetIntervalOnException)
                {
                    DelayToNextProcess();
                }
            }
            finally
            {
                lock (locker)
                {
                    busy = false;
                }
            }
        }

        private void DelayToNextProcess()
        {
            int delay = interval;
            if (isIntervalSinceInvokeTime && invokeStartTime.HasValue)
            {
                delay = (int)(interval - (DateTime.UtcNow - invokeStartTime.Value).TotalMilliseconds);
            }
            if (delay > 0)
            {
                //Thread.Sleep(delay);
                Task.Delay(delay).Wait();
            }
        }
        #endregion
    }
}
