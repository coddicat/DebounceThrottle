﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DebounceThrottle
{
    /// <summary>
    /// The Debounce dispatcher delays the invocation of an action until a predetermined interval has elapsed since the last call. This ensures that the action is only invoked once after the calls have stopped for the specified duration.
    /// </summary>
    /// <typeparam name="T">Type of the debouncing Task</typeparam>
    public class DebounceDispatcher<T> : IDisposable
    {
        private Task<T> _waitingTask;
        private Func<T> _functToInvoke;
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
        /// Debouncing the execution of action.
        /// It ensures that a function is invoked only once within a specified interval, even if multiple invocations are requested.
        /// </summary>
        /// <param name="interval">The minimum interval between invocations of the debounced function.</param>
        /// <param name="maxDelay">The maximum delay for an execution since the first trigger, after which the action must be executed. Can be null.</param>
        public DebounceDispatcher(TimeSpan interval, TimeSpan? maxDelay = null)
        {
            _isDisposed = false;
            _interval = interval;
            _maxDelay = maxDelay ?? TimeSpan.MaxValue;
        }

        /// <summary>
        /// DebounceAsync method manages the debouncing of the function invocation.
        /// </summary>
        /// <param name="function">The function to be invoked</param>
        /// <param name="cancellationToken">An optional CancellationToken</param>
        /// <returns>Returns Task to be executed with minimal delay</returns>
        public Task<T> DebounceAsync(Func<T> function, CancellationToken cancellationToken = default)
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

        private Task<T> HandleAsync(Func<T> function, CancellationToken cancellationToken = default)
        {
            lock (_locker)
            {
                _functToInvoke = function;
                _invocationStopWatch.Restart();
                _initialStopWatch.Start();

                if (_waitingTask != null)
                {
                    return _waitingTask;
                }

                _waitingTask = Task.Run(() =>
                {
                    do
                    {
                        if (_isDisposed)
                        {
                            return default;
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    while (DelayCondition);
                   
                    return Invoke();

                }, cancellationToken);

                return _waitingTask;
            }
        }

        private T Invoke()
        {
            T res;
            try
            {
                _initialStopWatch.Reset();
                res = _functToInvoke.Invoke();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                lock (_locker)
                {
                    _functToInvoke = null;
                    _waitingTask = null;
                }
            }
            return res;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public T FlushAndDispose()
        {
            T res = _functToInvoke == null 
                ? default 
                : _functToInvoke.Invoke();

            Dispose();
            return res;
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

