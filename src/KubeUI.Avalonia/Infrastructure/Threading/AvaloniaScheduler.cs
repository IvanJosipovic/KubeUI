using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Infrastructure.Threading;

/// <summary>
/// Provides a scheduler that executes actions on the Avalonia UI thread, enabling scheduling of work to run on the
/// dispatcher.
/// </summary>
/// <remarks>Use <see cref="Instance"/> to access the singleton instance. This scheduler is
/// typically used to marshal work onto the Avalonia UI thread, ensuring thread-safe interaction with UI components.
/// Actions scheduled with zero delay are posted to the dispatcher to prevent recursive scheduling from growing the
/// call stack.</remarks>
public sealed class AvaloniaScheduler : LocalScheduler, IThreadDispatcher
{
    /// <summary>
    /// Gets the singleton instance of the AvaloniaScheduler.
    /// </summary>
    /// <remarks>Use this property to access the default scheduler for Avalonia operations. The instance is
    /// thread-safe and intended for global use throughout the application.</remarks>
    public static readonly AvaloniaScheduler Instance = new();

    /// <inheritdoc />
    public IScheduler Scheduler => this;

    /// <inheritdoc />
    public void Invoke(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }

        Dispatcher.UIThread.Invoke(action);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaScheduler"/> class.
    /// </summary>
    private AvaloniaScheduler()
    {
    }

    /// <inheritdoc/>
    public override IDisposable Schedule<TState>(
        TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        IDisposable PostOnDispatcher()
        {
            var composite = new CompositeDisposable(2);

            var cancellation = new CancellationDisposable();

            Dispatcher.UIThread.Post(
                                     () =>
                                     {
                                         if (!cancellation.Token.IsCancellationRequested)
                                         {
                                             composite.Add(action(this, state));
                                         }
                                     },
                                     DispatcherPriority.Background);

            composite.Add(cancellation);

            return composite;
        }

        if (dueTime == TimeSpan.Zero)
        {
            return PostOnDispatcher();
        }

        {
            var composite = new CompositeDisposable(2);

            composite.Add(DispatcherTimer.RunOnce(() => composite.Add(action(this, state)), dueTime, DispatcherPriority.Background));

            return composite;
        }
    }
}
