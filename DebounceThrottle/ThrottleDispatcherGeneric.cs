using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace DebounceThrottle
{
    /// <summary>
    /// The Throttle dispatcher, on the other hand, limits the invocation of an action to a specific time interval. This means that the action will only be executed once within the given time frame, regardless of how many times it is called.
    /// </summary>
    /// <typeparam name="T">Return Type of the executed tasks</typeparam>
    public class ThrottleDispatcher<T> : IDisposable
    {
        private readonly TimeSpan _interval;
        private readonly bool _delayAfterExecution;
        private readonly bool _resetIntervalOnException;
        private readonly object _locker = new object();
        private Task<T> _lastTask;
        private readonly Stopwatch _invocationStopWatch = new Stopwatch();
        private bool _busy;
        private bool _isDisposed;
        private readonly SemaphoreSlim _sync = new SemaphoreSlim(1, 1);

        private bool ShouldWait =>
            !_isDisposed &&
            TimeLeftToInvoke < _interval;

        private TimeSpan TimeLeftToInvoke =>
            _invocationStopWatch.Elapsed;            


        /// <summary>
        /// ThrottleDispatcher is a utility class for throttling the execution of asynchronous tasks.
        /// It limits the rate at which a function can be invoked based on a specified interval.
        /// </summary>
        /// <param name="interval">The minimum interval between invocations of the throttled function.</param>
        /// <param name="delayAfterExecution">If true, the interval is calculated from the end of the previous task execution, otherwise from the start.</param>
        /// <param name="resetIntervalOnException">If true, the interval is reset when an exception occurs during the execution of the throttled function.</param>
        public ThrottleDispatcher(
            TimeSpan interval,
            bool delayAfterExecution = false,
            bool resetIntervalOnException = false)
        {
            _interval = interval;
            _delayAfterExecution = delayAfterExecution;
            _resetIntervalOnException = resetIntervalOnException;
            _isDisposed = false;
        }

        /// <summary>
        /// Throttling of the function invocation
        /// </summary>
        /// <param name="function">The function returns Task to be invoked asynchronously.</param>
        /// <param name="cancellationToken">An optional CancellationToken</param>
        /// <returns>Returns a last executed Task</returns>
        public Task<T> ThrottleAsync(Func<Task<T>> function, CancellationToken cancellationToken = default)
        {
            try
            {
                _sync.Wait();

                if (_isDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return HandleAsync(function, cancellationToken);
            }
            finally
            {
                _sync.Release();
            }            
        }

        public Task<T> HandleAsync(Func<Task<T>> function, CancellationToken cancellationToken = default)
        {
            lock (_locker)
            {                
                if (_lastTask != null && (_busy || ShouldWait))
                {
                    return _lastTask;
                }

                if (_isDisposed)
                {
                    return Task.FromCanceled<T>(cancellationToken);
                }

                _busy = true;

                _invocationStopWatch.Restart();

                _lastTask = function.Invoke();

                _lastTask.ContinueWith(task =>
                {
                    if (_delayAfterExecution)
                    {
                        _invocationStopWatch.Restart();
                    }
                    _busy = false;
                }, cancellationToken);

                if (_resetIntervalOnException)
                {
                    _lastTask.ContinueWith((task, obj) =>
                    {
                        _lastTask = null;
                        _invocationStopWatch.Reset();
                    }, cancellationToken, TaskContinuationOptions.OnlyOnFaulted);
                }

                return _lastTask;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _sync.Wait();
                    _sync.Dispose();
                }

                _isDisposed = true;
            }
        }
    }
}

