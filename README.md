# DebounceThrottle

The DebounceThrottle library provides robust and precise mechanisms for debouncing and throttling actions in .NET applications. These dispatchers are designed to manage the execution of asynchronous and synchronous tasks, ensuring efficient and controlled invocation patterns.

- **[NuGet Package](https://www.nuget.org/packages/DebounceThrottle)**: Download and integrate the package from NuGet.
- **[Source Code](https://github.com/coddicat/DebounceThrottle)**: View the source code and contribute on GitHub.

## Features

- **Debounce Dispatcher**: Delays the invocation of an action until a predetermined interval has precisely elapsed since the last call. This guarantees that the action is only invoked once after the calls have stopped for the specified duration.
- **Throttle Dispatcher**: Limits the invocation of an action to a specific time interval, ensuring that the action is executed only once within the given time frame, regardless of how many times it is called.
- **High Precision**: Includes a specific dispatcher using threads to achieve maximum precision, although it's not recommended for general use due to potential complexity.

## Installation
Install via NuGet Package Manager:

```bash
dotnet add package DebounceThrottle
```
Or add it directly to your csproj file:

```xml
<PackageReference Include="DebounceThrottle" Version="3.0.1" />
```

# Usage

## Debounce Dispatcher
The Debounce Dispatcher delays the action until the specified interval has passed without any new calls. This is useful for scenarios like user input where you want to wait until the user has finished typing before processing.

### Basic Example:

```csharp

using DebounceThrottle;
using System;
using System.Diagnostics;

Console.WriteLine("Enter text and see debounce effect, press ESC to exit");
string str = "";
using var debounceDispatcher = new DebounceDispatcher(
    interval: TimeSpan.FromMilliseconds(500),
    maxDelay: TimeSpan.FromSeconds(3));

var initStopWatch = new Stopwatch();
var lastStopWatch = new Stopwatch();
while (true)
{
    var key = Console.ReadKey(true);
    if (key.Key == ConsoleKey.Escape) break; // Exit on ESC
    str += key.KeyChar;

    initStopWatch.Start();
    lastStopWatch.Restart();

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
```

## Throttle Dispatcher
The Throttle Dispatcher ensures the action is only executed once within the specified interval, regardless of how many times it is called. This is useful for rate-limiting operations like API calls.

### Basic Example:

```csharp
using DebounceThrottle;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

Console.WriteLine("Throttle demo, press any key to exit");

bool stop = false;
var stopTask = Task.Run(() => { Console.ReadKey(true); stop = true; });
var stopWatch = new Stopwatch();
using var throttleDispatcher = new ThrottleDispatcher(TimeSpan.FromMilliseconds(500));

do
{
    stopWatch.Start();
    throttleDispatcher.Throttle(() =>
    {
        Console.WriteLine($"{stopWatch.Elapsed.TotalMilliseconds}");
        stopWatch.Reset();
    });
}
while (!stop); //wait trigger to stop and exit

Console.WriteLine("Finished");
```

# High-Precision Dispatcher (Not Recommended)
A specific dispatcher using threads for maximum precision. Use with caution due to potential complexity and threading issues.

### Example:

```csharp
using DebounceThrottle;
using System;
using System.Threading;

using var dispatcher = new DebounceThreadDispatcher(TimeSpan.FromMilliseconds(500));
dispatcher.Debounce(() => Console.WriteLine("Action executed with high precision"), CancellationToken.None);
```

# API

## DebounceDispatcher

```csharp
public class DebounceDispatcher<T> : IDisposable
{
    /// ctor
    public DebounceDispatcher(TimeSpan interval, TimeSpan? maxDelay = null);

    /// returns a Task that will complete only when the invocation actually occurs, ensuring the action is executed after the debounce interval has fully elapsed. Don't await this Task.
    public Task<T> DebounceAsync(Func<T> function, CancellationToken cancellationToken = default);    
    
    /// Flushes any pending function and disposes of the dispatcher.
    public T FlushAndDispose();
}

## ThrottleDispatcher

```csharp

public class ThrottleDispatcher<T> : IDisposable
{
    /// ctor
    public ThrottleDispatcher(TimeSpan interval, bool delayAfterExecution = false, bool resetIntervalOnException = false);

    /// returns a Task representing the last invocation, which will complete when the action is executed at the next allowable time according to the throttle interval. Don't await this Task.
    public Task ThrottleAsync(Func<Task> function, CancellationToken cancellationToken = default);    
}
```

## DebounceThreadDispatcher

```csharp
public class DebounceThreadDispatcher : IDisposable
{
    public DebounceThreadDispatcher(TimeSpan interval, TimeSpan? maxDelay = null);
    public Thread Debounce(Action action, CancellationToken cancellationToken = default);
    public void FlushAndDispose();
}
```

# License
This project is licensed under the MIT License - see the LICENSE file for details.

# Contributing
Contributions are welcome! Please open an issue or submit a pull request for any improvements or bug fixes.