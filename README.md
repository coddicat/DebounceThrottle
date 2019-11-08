//C# DebounceThrottle
//C# Debounce and Throttle dispatchers

class Program
{
    static void Main(string[] args)
    {
        ConsoleKey? key = null;
        Task.Run(() => { key = Console.ReadKey(true).Key; }); //when to stop
        var throttleDispatcher = new ThrottleDispatcher(1000);
        do
        {
            //Action will be invoked once in a 1000 milliseconds
            throttleDispatcher.Throttle(() =>
            {
                Console.WriteLine($"{ DateTime.UtcNow.ToString("hh:mm:ss.fff") }");
            });

            //repeat every 100 milliseconds
            Task.Delay(100).Wait();
        }
        while (!key.HasValue);
    }
}
