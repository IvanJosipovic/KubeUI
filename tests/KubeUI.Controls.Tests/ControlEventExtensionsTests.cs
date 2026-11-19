using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using KubeUI.Controls.Presentation;
using Shouldly;

namespace KubeUI.Controls.Tests;

public sealed class ControlEventExtensionsTests
{
    [AvaloniaFact]
    public void Tree_view_selection_changes_reach_the_subscribed_handler()
    {
        var tree = new TreeView { ItemsSource = new[] { "row" } };
        var raised = 0;

        tree.OnSelectionChanged((_, _) => raised++).ShouldBeSameAs(tree);

        tree.SelectedItem = "row";

        raised.ShouldBe(1);
    }
}
