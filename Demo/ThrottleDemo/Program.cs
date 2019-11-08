using DebounceThrottle;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            bool stop = false;

            //trigger when to stop and exit
            Task.Run(() =>
            {
                Console.ReadKey(true);
                stop = true;
            });

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
}
