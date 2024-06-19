using DebounceThrottle;
using System.Diagnostics;

Console.WriteLine("Throttle demo, press any key to exit");

bool stop = false;
//trigger when to stop and exit
var stopTask = Task.Run(() => { Console.ReadKey(true); stop = true; });
var stopWatch = new Stopwatch();
using var throttleDispatcher = new ThrottleDispatcher(TimeSpan.FromMilliseconds(500));
do
{
    stopWatch.Start();
    //every iteration call dispatcher but the Action will be invoked only once in 1500 milliseconds (500 action work time + 1000 interval)
    throttleDispatcher.Throttle(() =>
    {
        Console.WriteLine($"{stopWatch.Elapsed.TotalMilliseconds}");
        stopWatch.Reset();
    });
}
while (!stop); //wait trigger to stop and exit

Console.WriteLine("Finished");
