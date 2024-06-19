using System;
using System.Diagnostics;
using System.Threading;

namespace DebounceThrottle
{
    /// <summary>
    /// The Debounce thread dispatcher delays the invocation of an action with high accuracy, ensuring the action is only executed after a specified interval has precisely elapsed since the last call. This mechanism guarantees that the action is triggered only once, even after multiple calls, achieving reliable timing and precise control over action execution.
    /// </summary>
    public class DebounceThreadDispatcher : IDisposable
    {
        private Thread _waitingThread;
        private Action _actionToInvoke;
        private object _locker = new object();

        private readonly Stopwatch _invocationStopWatch = new Stopwatch();
        private readonly Stopwatch _initialStopWatch = new Stopwatch();

        private readonly TimeSpan _interval;
        private readonly TimeSpan _maxDelay;

        private bool _isDisposed;
        private readonly SemaphoreSlim _sync = new SemaphoreSlim(1, 1);

        private TimeSpan TimeSinceLastInvoke =>
            _invocationStopWatch.Elapsed;
        private TimeSpan TimeSinceInitital =>
            _initialStopWatch.Elapsed;
        private TimeSpan TimeLeftToMaxDelay =>
            _maxDelay - TimeSinceInitital;

        private bool DelayCondition =>
            !_isDisposed &&
            TimeSinceLastInvoke < _interval &&
            TimeLeftToMaxDelay > TimeSpan.Zero;

        /// <summary>
        /// Debouncing the execution of asynchronous tasks.
        /// It ensures that a function is invoked only once within a specified interval, even if multiple invocations are requested.
        /// </summary>
        /// <param name="interval">The minimum interval between invocations of the debounced function.</param>
        /// <param name="maxDelay">The maximum delay for an execution since the first trigger, after which the action must be executed. Can be null.</param>
        public DebounceThreadDispatcher(TimeSpan interval, TimeSpan? maxDelay = null)
        {
            _isDisposed = false;
            _interval = interval;
            _maxDelay = maxDelay ?? TimeSpan.MaxValue;
        }

        /// <summary>
        /// DebounceAsync method manages the debouncing of the function invocation.
        /// </summary>
        /// <param name="action">The function to be invoked</param>
        /// <param name="cancellationToken">An optional CancellationToken</param>
        /// <returns>Returns Task to be executed with minimal delay</returns>
        public Thread Debounce(Action action, CancellationToken cancellationToken = default)
        {
            try
            {
                _sync.Wait();

                if (_isDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }               
               
                return HandleAsync(action, cancellationToken);
            }
            finally
            {
                _sync.Release();
            }            
        }

        private Thread HandleAsync(Action action, CancellationToken cancellationToken = default)
        {
            lock (_locker)
            {
                _actionToInvoke = action;
                _invocationStopWatch.Restart();
                _initialStopWatch.Start();

                if (_waitingThread != null)
                {
                    return _waitingThread;
                }                

                _waitingThread = new Thread(() =>
                {
                    do
                    {
                        if (_isDisposed)
                        {
                            return;
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    while (DelayCondition);

                    Invoke();
                });

                _waitingThread.Start();

                return _waitingThread;
            }
        }

        private void Invoke()
        {
            try
            {
                _initialStopWatch.Reset();
                _actionToInvoke.Invoke();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                lock (_locker)
                {
                    _actionToInvoke = null;
                    _waitingThread = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void FlushAndDispose()
        {
            _actionToInvoke?.Invoke();
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _sync.Wait();
                    _sync?.Dispose();
                    _invocationStopWatch?.Reset();
                    _initialStopWatch?.Reset();
                }
                
                _isDisposed = true;
            }
        }
    }
}

