# C# Debounce and Throttle dispatchers

Debounce and Throttle dispatchers support asynchronous actions, handle exceptions.
All Task results from dispatcher calls will be equal to result of the a single invoking.  

## Debounce demo
Show entered text after stopping pressing keys for 1000 milliseconds
```csharp
    class Program
    {
        static void Main(string[] args)
        {
            string str = "";
            var debounceDispatcher = new DebounceDispatcher(1000);
            while(true)
            {
                var key = Console.ReadKey(true);

                //trigger when to stop and exit
                if (key.Key == ConsoleKey.Escape) break;

                str += key.KeyChar;

                //every keypress iteration call dispatcher but the Action will be invoked only after stopping pressing and waiting 1000 milliseconds
                debounceDispatcher.Debounce(() =>
                {
                    Console.WriteLine($"{str} - {DateTime.UtcNow.ToString("hh:mm:ss.fff")}");
                    str = "";
                });
            }
        }
    }
```

## Throttle demo
Call action every 100 milliseconds but invoke only once in 1000 milliseconds (after last invoking completed)  
```csharp
    class Program
    {
        static void Main(string[] args)
        {
            bool stop = false;
            //trigger when to stop and exit
            Task.Run(() => { Console.ReadKey(true); stop = true; });

            var throttleDispatcher = new ThrottleDispatcher(1000);
            do
            {
                //every iteration call dispatcher but the Action will be invoked only once in 1500 milliseconds (500 action work time + 1000 interval)
                throttleDispatcher.ThrottleAsync(async () =>
                {
                    Console.WriteLine($"{ DateTime.UtcNow.ToString("hh:mm:ss.fff") }");
                    await Task.Delay(500);
                });

                //iteration every 100 milliseconds
                Thread.Sleep(100);
            }
            while (!stop); //wait trigger to stop and exit
        }
    }
```
