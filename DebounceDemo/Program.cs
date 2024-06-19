using DebounceThrottle;
using System.Diagnostics;

Console.WriteLine("Enter text and see debounce effect, press ESC to exit");

string str = "";
using var debounceDispatcher = new DebounceDispatcher(
    interval: TimeSpan.FromSeconds(1),
    maxDelay: TimeSpan.FromSeconds(5));

var initStopWatch = new Stopwatch();
var lastStopWatch = new Stopwatch();
while (true)
{
    var key = Console.ReadKey(true);

    //trigger when to stop and exit
    if (key.Key == ConsoleKey.Escape) break;

    str += key.KeyChar;

    initStopWatch.Start();
    lastStopWatch.Restart();

    //every keypress iteration call dispatcher but the Action will be invoked only after stopping pressing and waiting 1000 milliseconds
    debounceDispatcher.Debounce(() =>
    {
        string output = $"{str} - start:{initStopWatch.Elapsed.TotalMilliseconds} - last:{lastStopWatch.Elapsed.TotalMilliseconds}";
        Console.WriteLine(output);
        str = "";
        initStopWatch.Reset();
        lastStopWatch.Reset();        
    });
}

debounceDispatcher.FlushAndDispose();

Console.WriteLine("Finished");