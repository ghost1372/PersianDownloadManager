namespace PersianDownloadManager.Common;

/// <summary>
/// Throttles frequent save requests with a debounce-plus-max-interval strategy:
/// <list type="bullet">
///   <item>Saves <paramref name="debounceDelay"/> after the last <see cref="RequestSave"/> call (quiet-period flush).</item>
///   <item>Guarantees a save at least every <paramref name="maxInterval"/> during continuous activity.</item>
/// </list>
/// <see cref="RequestSave"/> is safe to call from any thread.
/// </summary>
internal sealed partial class SaveDebouncer : IDisposable
{
    private readonly Action _save;
    private readonly DispatcherTimer _timer;
    private readonly long _debounceDelayTicks;
    private readonly long _maxIntervalTicks;

    private volatile bool _pending;
    private long _lastChangeTicks;
    private long _lastSaveTicks;
    private bool _disposed;

    /// <param name="save">Save action — always invoked on the UI thread.</param>
    /// <param name="debounceDelay">How long to wait after the last change before saving (quiet-period trigger).</param>
    /// <param name="maxInterval">Maximum time between saves during continuous activity (throttle ceiling).</param>
    public SaveDebouncer(Action save, TimeSpan debounceDelay, TimeSpan maxInterval)
    {
        _save = save;
        _debounceDelayTicks = debounceDelay.Ticks;
        _maxIntervalTicks = maxInterval.Ticks;

        long now = DateTime.UtcNow.Ticks;
        _lastChangeTicks = now;
        _lastSaveTicks = now;

        // Poll every second — cheap enough to be invisible, responsive enough to honour both thresholds.
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTick;
        _timer.Start();
    }

    /// <summary>Signals that a save is needed. Safe to call from any thread.</summary>
    public void RequestSave()
    {
        Interlocked.Exchange(ref _lastChangeTicks, DateTime.UtcNow.Ticks);
        _pending = true;
    }

    /// <summary>
    /// Immediately performs a pending save if one is waiting.
    /// Must be called on the UI thread (e.g., on app close).
    /// </summary>
    public void Flush()
    {
        if (_pending)
            ExecuteSave();
    }

    private void OnTick(object? sender, object e)
    {
        if (!_pending) return;

        long now = DateTime.UtcNow.Ticks;
        long sinceLastChange = now - Interlocked.Read(ref _lastChangeTicks);
        long sinceLastSave   = now - Interlocked.Read(ref _lastSaveTicks);

        bool quietEnough   = sinceLastChange >= _debounceDelayTicks;
        bool overdueForSave = sinceLastSave  >= _maxIntervalTicks;

        if (quietEnough || overdueForSave)
            ExecuteSave();
    }

    private void ExecuteSave()
    {
        _pending = false;
        Interlocked.Exchange(ref _lastSaveTicks, DateTime.UtcNow.Ticks);
        _save();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Tick -= OnTick;
        _timer.Stop();
    }
}
