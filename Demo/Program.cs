using DebounceThrottle;

Console.WriteLine("press any key to exit");


bool stop = false;
//trigger when to stop and exit
var stopTask = Task.Run(() => { Console.ReadKey(true); stop = true; });

using var throttleDispatcher = new ThrottleDispatcher(TimeSpan.FromMilliseconds(10000));
do
{
    //every iteration call dispatcher but the Action will be invoked only once in 1500 milliseconds (500 action work time + 1000 interval)
    throttleDispatcher.Throttle(() =>
    {
        Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss.fff}");
    });
}
while (!stop); //wait trigger to stop and exit

Console.WriteLine("Finished");
