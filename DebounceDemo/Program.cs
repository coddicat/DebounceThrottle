using DebounceThrottle;

Console.WriteLine("Enter text and see debounce effect, press ESC to exit");

string str = "";
using var debounceDispatcher = new DebounceDispatcher(
    interval: TimeSpan.FromSeconds(1),
    maxDelay: TimeSpan.FromSeconds(5));

while (true)
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

await debounceDispatcher.FlushAndDisposeAsync();

Console.WriteLine("Finished");