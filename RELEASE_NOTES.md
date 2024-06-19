# Release Notes

## Version 3.0.0

### Improvements
- **Refactored Time Handling**: Updated to use `Stopwatch` instead of `DateTime` for timing. This change ensures that the debounce and throttle mechanisms are independent of system clock changes.
- **Enhanced Precision**: Achieved precision improvements to less than 1 millisecond by combining `Stopwatch` and threading for precise timing control.

### Changes
- Replaced `DateTime` with `Stopwatch` to eliminate issues related to system clock adjustments and daylight saving time changes.
- Improved the timing accuracy of the debounce and throttle operations, making them more reliable in high-precision scenarios.

## Version 2.1.0

### New Features
- **Disposable Pattern**: Added support for the `IDisposable` interface to allow for proper resource management and cleanup.
- **FlushAndDispose Method**: Introduced the `FlushAndDispose` method to ensure that any pending actions are executed before disposing of the dispatcher.

### Changes
- Added `IDisposable` implementation to both `DebounceDispatcher` and `ThrottleDispatcher` classes.
- Implemented the `FlushAndDispose` method to provide a mechanism for flushing any pending operations before disposal, ensuring no actions are left unexecuted.

---

### Additional Information

- **[NuGet Package](https://www.nuget.org/packages/DebounceThrottle)**: Download and integrate the package from NuGet.
- **[Source Code](https://github.com/coddicat/DebounceThrottle)**: View the source code and contribute on GitHub.
