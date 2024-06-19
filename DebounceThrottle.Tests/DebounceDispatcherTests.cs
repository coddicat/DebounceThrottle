namespace DebounceThrottle.Tests
{
	public class DebounceDispatcherTests
	{
        [Fact]
        public async Task DebounceAsync_MultipleCallsWithinInterval_ExecutesFunctionOnce()
        {
            var debounceDispatcher = new DebounceDispatcher<int>(TimeSpan.FromMilliseconds(100));
            int counter = 0;
            Func<int> functToInvoke = () =>
            {
                counter++;
                return counter;
            };

            var tasks = new[]
            {
                debounceDispatcher.DebounceAsync(functToInvoke),
                debounceDispatcher.DebounceAsync(functToInvoke),
                debounceDispatcher.DebounceAsync(functToInvoke)
            };

            int[] results = await Task.WhenAll(tasks);
            Assert.Equal(1, counter);
            Assert.All(results, r => Assert.Equal(1, r));
        }

        [Fact]
        public async Task DebounceAsync_MultipleCallsOutsideInterval_ExecutesFunctionMultipleTimes()
        {
            var debounceDispatcher = new DebounceDispatcher<int>(TimeSpan.FromMilliseconds(100));
            int counter = 0;
            Func<int> functToInvoke = () =>
            {
                counter++;
                return counter;
            };
            Task<int> CallDebounceAsyncAfterDelay(int delay)
            {
                Task.Delay(delay).ConfigureAwait(false).GetAwaiter().GetResult();
                return debounceDispatcher.DebounceAsync(functToInvoke);
            };

            var tasks = new []
            {
                debounceDispatcher.DebounceAsync(functToInvoke),
                CallDebounceAsyncAfterDelay(150),
                CallDebounceAsyncAfterDelay(300)
            };

            int[] results = await Task.WhenAll(tasks);
            Assert.Equal(3, counter);
            Assert.Equal(new[] { 1, 2, 3 }, results);
        }
    }
}

