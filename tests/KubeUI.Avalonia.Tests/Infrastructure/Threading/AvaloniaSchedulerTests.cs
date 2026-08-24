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
}
