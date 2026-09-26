using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace KubeUI.DynamicTableView;

/// <summary>Schedules source collection notifications on Avalonia's UI dispatcher.</summary>
internal sealed class AvaloniaDispatcherScheduler : LocalScheduler
{
    /// <inheritdoc />
    public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (dueTime < TimeSpan.Zero)
            dueTime = TimeSpan.Zero;

        var composite = new CompositeDisposable();
        var cancellation = new CancellationDisposable();
        composite.Add(cancellation);

        void Run()
        {
            if (!cancellation.Token.IsCancellationRequested)
                composite.Add(action(this, state));
        }

        if (dueTime == TimeSpan.Zero)
        {
            Dispatcher.UIThread.Post(Run, DispatcherPriority.Background);
        }
        else
        {
            composite.Add(DispatcherTimer.RunOnce(Run, dueTime, DispatcherPriority.Background));
        }

        return composite;
    }
}
