namespace KubeUI.Avalonia.Infrastructure.Threading;

public sealed class AvaloniaUiRefreshClock : IUiRefreshClock
{
    private readonly DispatcherTimer _timer = new(DispatcherPriority.Default)
    {
        Interval = TimeSpan.FromSeconds(1)
    };

    private readonly Dictionary<long, Action> _callbacks = [];
    private long _nextSubscriptionId;

    public AvaloniaUiRefreshClock()
    {
        _timer.Tick += Timer_Tick;
    }

    public IDisposable Subscribe(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        long subscriptionId = ++_nextSubscriptionId;
        _callbacks.Add(subscriptionId, callback);
        if (_callbacks.Count == 1)
        {
            _timer.Start();
        }

        return new Subscription(this, subscriptionId);
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        foreach (Action callback in _callbacks.Values.ToArray())
        {
            callback();
        }
    }

    private void Unsubscribe(long subscriptionId)
    {
        if (_callbacks.Remove(subscriptionId) && _callbacks.Count == 0)
        {
            _timer.Stop();
        }
    }

    private sealed class Subscription : IDisposable
    {
        private AvaloniaUiRefreshClock? _owner;
        private readonly long _subscriptionId;

        public Subscription(AvaloniaUiRefreshClock owner, long subscriptionId)
        {
            _owner = owner;
            _subscriptionId = subscriptionId;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _owner, null)?.Unsubscribe(_subscriptionId);
        }
    }
}
