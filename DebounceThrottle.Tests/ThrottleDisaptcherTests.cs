namespace DebounceThrottle.Tests;

public class ThrottleDisaptcherTests
{
    [Fact]
    public async Task ThrottleDispatcher_ThrottleAsync_InvokesFunction()
    {
        var dispatcher = new ThrottleDispatcher<int>(TimeSpan.FromMilliseconds(10));
        int result = await dispatcher.ThrottleAsync(() => Task.FromResult(42));
        Assert.Equal(42, result);
    }   

    [Fact]
    public async Task ThrottleDispatcher_ThrottleAsync()
    {
        int counter = 0;
        var dispatcher = new ThrottleDispatcher<int>(TimeSpan.FromMilliseconds(100));
        DateTime startTime = DateTime.UtcNow;
        List<Task<int>> tasks = new List<Task<int>>();
        Func<Task<int>> functToInvoke = () =>
        {
            counter++;
            Console.WriteLine("func:" + DateTime.UtcNow.ToString("mm:ss:fff"));
            return Task.FromResult(counter);
        };
        Task<int> CallThrottleAsyncAfterDelay(int delay)
        {
            Task.Delay(delay).ConfigureAwait(false).GetAwaiter().GetResult();
            Console.WriteLine("throttle:" + DateTime.UtcNow.ToString("mm:ss:fff"));
            return dispatcher.ThrottleAsync(functToInvoke);
        };

        for (int i = 0; i < 20; i++)
        {
            tasks.Add(CallThrottleAsyncAfterDelay(50));
        }
        var results = await Task.WhenAll(tasks);
        var group = results.GroupBy(x => x);
        Assert.True(group.All(x => x.Count() == 2));
        Assert.Equal(10, group.Count());
        Assert.Equal(10, counter);
    }

    [Fact]
    public async Task ThrottleDispatcher_ThrottleDelayAfterExecutionAsync()
    {
        int counter = 0;
        var dispatcher = new ThrottleDispatcher<int>(TimeSpan.FromMilliseconds(100), true);
        DateTime startTime = DateTime.UtcNow;
        List<Task<int>> tasks = new List<Task<int>>();
        Func<Task<int>> functToInvoke = async () =>
        {
            counter++;
            Console.WriteLine("start func:" + DateTime.UtcNow.ToString("mm:ss:fff"));
            await Task.Delay(50);
            Console.WriteLine("end func:" + DateTime.UtcNow.ToString("mm:ss:fff"));
            return counter;
        };
        Task<int> CallThrottleAsyncAfterDelay(int delay)
        {
            Task.Delay(delay).ConfigureAwait(false).GetAwaiter().GetResult();
            Console.WriteLine("throttle:" + DateTime.UtcNow.ToString("mm:ss:fff"));
            return dispatcher.ThrottleAsync(functToInvoke);
        };

        for (int i = 0; i < 20; i++)
        {
            tasks.Add(CallThrottleAsyncAfterDelay(50));
        }
        var results = await Task.WhenAll(tasks);
        var group = results.GroupBy(x => x);
        Assert.Equal(7, group.Count());
        Assert.Equal(7, counter);
    }
}
