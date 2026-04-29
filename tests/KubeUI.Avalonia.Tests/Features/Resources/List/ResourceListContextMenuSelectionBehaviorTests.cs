using Avalonia.Controls.Selection;
using Avalonia.Headless.XUnit;
using KubeUI.Avalonia.Features.Resources.List.Behaviors;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.List;

public sealed class ResourceListContextMenuSelectionBehaviorTests
{
    [AvaloniaFact]
    public void try_select_selects_index_when_not_selected()
    {
        var selectionModel = new SelectionModel<object>();
        selectionModel.Source = new[] { new object(), new object() };

        ResourceListContextMenuSelectionBehavior.TrySelect(selectionModel, 1).ShouldBeTrue();
        selectionModel.SelectedIndexes.ShouldBe([1]);
    }

    [AvaloniaFact]
    public void try_select_keeps_existing_selection_when_index_is_already_selected()
    {
        var selectionModel = new SelectionModel<object>();
        selectionModel.Source = new[] { new object(), new object() };
        selectionModel.Select(0);

        ResourceListContextMenuSelectionBehavior.TrySelect(selectionModel, 0).ShouldBeTrue();
        selectionModel.SelectedIndexes.ShouldBe([0]);
    }
}
