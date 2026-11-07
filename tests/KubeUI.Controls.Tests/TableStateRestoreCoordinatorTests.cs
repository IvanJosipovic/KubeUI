using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using KubeUI.Controls.Presentation;
using Shouldly;

namespace KubeUI.Controls.Tests;

public sealed class TableStateRestoreCoordinatorTests
{
    [AvaloniaFact]
    public void Restore_posts_state_then_selection_once_per_table_and_view_model_pair()
    {
        var coordinator = new TableStateRestoreCoordinator<string>();
        var table = new object();
        var viewModel = new object();
        var restored = new List<string>();

        coordinator.Restore(table, viewModel, "column-state", () => true, state => restored.Add(state), () => restored.Add("selection"));
        Dispatcher.UIThread.RunJobs();

        coordinator.Restore(table, viewModel, "new-state", () => true, state => restored.Add(state), () => restored.Add("selection"));
        Dispatcher.UIThread.RunJobs();
        restored.ShouldBe(["column-state", "selection", "selection"]);

        coordinator.Restore(new object(), viewModel, "replacement-table", () => true, state => restored.Add(state), () => restored.Add("selection"));
        Dispatcher.UIThread.RunJobs();
        restored.ShouldBe(["column-state", "selection", "selection", "replacement-table", "selection"]);
    }

    [AvaloniaFact]
    public void Restore_skips_stale_callback_and_still_supports_selection_only_state()
    {
        var coordinator = new TableStateRestoreCoordinator<string>();
        var restored = new List<string>();

        coordinator.Restore(new object(), new object(), "stale", () => false, state => restored.Add(state), () => restored.Add("selection"));
        Dispatcher.UIThread.RunJobs();
        restored.ShouldBeEmpty();

        coordinator.Restore(new object(), new object(), null, () => true, state => restored.Add(state), () => restored.Add("selection"));
        Dispatcher.UIThread.RunJobs();
        restored.ShouldBe(["selection"]);
    }
}
