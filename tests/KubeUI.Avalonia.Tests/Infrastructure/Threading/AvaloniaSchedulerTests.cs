using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using KubeUI.Avalonia.Infrastructure.Threading;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Threading;

public sealed class AvaloniaSchedulerTests
{
    [AvaloniaFact]
    public void Invoke_executes_action_on_ui_thread()
    {
        var executedOnUiThread = false;

        AvaloniaScheduler.Instance.Invoke(() => executedOnUiThread = Dispatcher.UIThread.CheckAccess());

        executedOnUiThread.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task Invoke_dispatches_background_action_to_ui_thread()
    {
        var executedOnUiThread = false;

        await Task.Run(() => AvaloniaScheduler.Instance.Invoke(() => executedOnUiThread = Dispatcher.UIThread.CheckAccess()));

        executedOnUiThread.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task Schedule_does_not_grow_the_call_stack_for_nested_zero_delay_work()
    {
        var completion = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var currentDepth = 0;
        var maximumDepth = 0;

        void ScheduleNext(IScheduler scheduler, int remaining)
        {
            scheduler.Schedule(
                remaining,
                TimeSpan.Zero,
                (nextScheduler, count) =>
                {
                    currentDepth++;
                    maximumDepth = Math.Max(maximumDepth, currentDepth);

                    if (count == 0)
                    {
                        completion.TrySetResult(maximumDepth);
                    }
                    else
                    {
                        ScheduleNext(nextScheduler, count - 1);
                    }

                    currentDepth--;
                    return System.Reactive.Disposables.Disposable.Empty;
                });
        }

        ScheduleNext(AvaloniaScheduler.Instance, 1000);

        (await completion.Task).ShouldBe(1);
    }
}
