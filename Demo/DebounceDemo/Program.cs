using DebounceThrottle;
using System;

namespace DebounceDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            string str = "";
            bool stop = false;
            var debounceDispatcher = new DebounceDispatcher(1000);
            do
            {
                var key = Console.ReadKey(true);

                //trigger when to stop and exit
                if (key.Key == ConsoleKey.Escape) stop = true;

                str += key.KeyChar;

                //every keypress iteration call dispatcher but the Action will be invoked only after stop pressing and waiting 1000 milliseconds
                debounceDispatcher.Debounce(() =>
                {
                    Console.WriteLine($"{str} - {DateTime.UtcNow.ToString("hh:mm:ss.fff")}");
                    str = "";
                });
            } 
            while (!stop); //wait trigger to stop and exit
        }
    }
}
