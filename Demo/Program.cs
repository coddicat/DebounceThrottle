using DebounceThrottle;

bool stop = false;
//trigger when to stop and exit
var stopTask = Task.Run(() => { Console.ReadKey(true); stop = true; });

var throttleDispatcher = new ThrottleDispatcher(1000);
do
{
    //every iteration call dispatcher but the Action will be invoked only once in 1500 milliseconds (500 action work time + 1000 interval)
    throttleDispatcher.Throttle(() =>
    {
        Console.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss.fff")}");
    });
}
while (!stop); //wait trigger to stop and exit