using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;

public sealed class MRDiffDetectionContextMenuBehavior : Behavior<DataGrid>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject?.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
    }

    protected override void OnDetaching()
    {
        AssociatedObject?.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        base.OnDetaching();
    }

    private static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not DataGrid grid
            || e.GetCurrentPoint(grid).Properties.PointerUpdateKind != PointerUpdateKind.RightButtonPressed
            || e.Source is not Visual source
            || grid.ContextMenu is not ContextMenu contextMenu
            || grid.DataContext is not MRDiffDetectionViewModel viewModel)
        {
            return;
        }

        var row = source.GetSelfAndVisualAncestors().OfType<DataGridRow>().FirstOrDefault();
        if (row?.DataContext is not CrossplaneDiffRow diffRow)
        {
            contextMenu.ItemsSource = null;
            return;
        }

        contextMenu.ItemsSource = new[]
        {
            new MenuItem
            {
                Header = Assets.Resources.ResourceConfigBase_MenuItem_ViewYaml,
                Command = viewModel.ViewYamlCommand,
                CommandParameter = diffRow
            }
        };
    }
}
