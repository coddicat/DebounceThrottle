using DebounceThrottle;

string str = "";
var debounceDispatcher = new DebounceDispatcher(500);
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