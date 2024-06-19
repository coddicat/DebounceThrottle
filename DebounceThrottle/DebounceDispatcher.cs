using System;
using System.Threading;
using System.Threading.Tasks;

namespace DebounceThrottle
{
    /// <summary>
    /// The Debounce dispatcher delays the invocation of an action until a predetermined interval has elapsed since the last call. This ensures that the action is only invoked once after the calls have stopped for the specified duration.
    /// </summary>
    public class DebounceDispatcher : DebounceDispatcher<bool>
    {
        /// <summary>
        /// Debouncing the execution of asynchronous tasks.
        /// It ensures that a function is invoked only once within a specified interval, even if multiple invocations are requested.
        /// </summary>
        /// <param name="interval">The minimum interval between invocations of the debounced function.</param>
        /// <param name="maxDelay">The maximum delay for an execution since the first trigger, after which the action must be executed. Can be null.</param>
        public DebounceDispatcher(TimeSpan interval, TimeSpan? maxDelay = null) : base(interval, maxDelay)
        {
        }

        /// <summary>
        /// Method manages the debouncing of the function invocation.
        /// </summary>
        /// <param name="function">The action to be invoked</param>
        /// <param name="cancellationToken">An optional CancellationToken</param>
        /// <returns>Returns Task to be executed with minimal delay</returns>
        public Task DebounceAsync(Action action, CancellationToken cancellationToken = default)
        {
            return base.DebounceAsync(() =>
            {
                action.Invoke();
                return true;
            }, cancellationToken);
        }

        /// <summary>
        /// Method manages the debouncing of the function invocation.
        /// </summary>
        /// <param name="action">The action to be invoked</param>
        /// <param name="cancellationToken">An optional CancellationToken</param>
        public void Debounce(Action action, CancellationToken cancellationToken = default)
        {
            base.DebounceAsync(() =>
            {
                action.Invoke();
                return true;
            }, cancellationToken);
        }
    }
}