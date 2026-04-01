using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using KubeUI.Avalonia.Infrastructure.Threading;

namespace KubeUI.Avalonia.Infrastructure.Threading;

/// <summary>
/// Provides a scheduler that executes actions on the Avalonia UI thread, enabling scheduling of work to run on the
/// dispatcher.
/// </summary>
/// <remarks>Use <see cref="AvaloniaScheduler.Instance"/> to access the singleton instance. This scheduler is
/// typically used to marshal work onto the Avalonia UI thread, ensuring thread-safe interaction with UI components.
/// Actions scheduled with zero delay may be executed immediately if already on the dispatcher thread, but excessive
/// immediate scheduling is limited to prevent stack overflows.</remarks>
public sealed class AvaloniaScheduler : LocalScheduler
{
    /// <summary>
    /// Gets the singleton instance of the AvaloniaScheduler.
    /// </summary>
    /// <remarks>Use this property to access the default scheduler for Avalonia operations. The instance is
    /// thread-safe and intended for global use throughout the application.</remarks>
    public static readonly AvaloniaScheduler Instance = new();

    /// <summary>
    /// Users can schedule actions on the dispatcher thread while being on the correct thread already.
    /// We are optimizing this case by invoking user callback immediately which can lead to stack overflows in certain cases.
    /// To prevent this we are limiting amount of reentrant calls to <see cref="Schedule{TState}"/> before we will
    /// schedule on a dispatcher anyway.
    /// </summary>
    private const int MaxReentrantSchedules = 32;

    private int _reentrancyGuard;

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
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

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
            if (!Dispatcher.UIThread.CheckAccess())
            {
                return PostOnDispatcher();
            }

            if (_reentrancyGuard >= MaxReentrantSchedules)
            {
                return PostOnDispatcher();
            }

            try
            {
                _reentrancyGuard++;

                return action(this, state);
            }
            finally
            {
                _reentrancyGuard--;
            }
        }

        {
            var composite = new CompositeDisposable(2);

            composite.Add(DispatcherTimer.RunOnce(() => composite.Add(action(this, state)), dueTime, DispatcherPriority.Background));

            return composite;
        }
    }
}

