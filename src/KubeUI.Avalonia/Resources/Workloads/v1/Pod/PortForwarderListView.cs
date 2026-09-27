using Avalonia.Input;
using Avalonia.VisualTree;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Kubernetes;
using System.Windows.Input;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public partial class PortForwarderListView : ViewBase<PortForwarderListViewModel>
{
    protected override object Build(PortForwarderListViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        DynamicTableView table = new()
        {
            GridLinesVisibility = DynamicTableViewGridLinesVisibility.All,
            Source = vm.TableSource,
            ContextMenu = new ContextMenu(),
            ContextMenuItemsFactory = targets => CreateContextMenuItems(vm, targets)
        };
        table.KeyBindings.Add(CreateSelectionKeyBinding(Key.Enter, vm, vm.OpenCommand));
        table.KeyBindings.Add(CreateSelectionKeyBinding(Key.Delete, vm, vm.RemoveCommand));
        table.DoubleTapped += (_, e) =>
        {
            if (FindRow(e.Source)?.DataContext is PortForwarder portForwarder && vm.OpenCommand.CanExecute(portForwarder))
                vm.OpenCommand.Execute(portForwarder);
        };
        return table;
    }

    private static KeyBinding CreateSelectionKeyBinding(Key key, PortForwarderListViewModel viewModel, ICommand command)
        => new()
        {
            Gesture = new KeyGesture(key),
            Command = new RelayCommand(
                () => ExecuteForSelection(viewModel, command),
                () => viewModel.SelectedItem is { } selected && command.CanExecute(selected))
        };

    private static void ExecuteForSelection(PortForwarderListViewModel viewModel, ICommand command)
    {
        if (viewModel.SelectedItem is { } selected && command.CanExecute(selected))
            command.Execute(selected);
    }

    private static TableViewRow? FindRow(object? source)
        => source is Visual visual ? visual.GetSelfAndVisualAncestors().OfType<TableViewRow>().FirstOrDefault() : null;

    private static IEnumerable<MenuItem> CreateContextMenuItems(PortForwarderListViewModel vm, IReadOnlyList<object> targets)
    {
        foreach (var portForwarder in targets.OfType<PortForwarder>())
        {
            yield return new MenuItem()
                .Header(Assets.Resources.PortForwarderListView_OpenInBrowser)
                .Command(vm, x => x.OpenCommand)
                .CommandParameter(portForwarder)
                .Icon(new FluentIcon().Icon(Icon.Open));
            yield return new MenuItem()
                .Header(Assets.Resources.PortForwarderListView_Remove)
                .Command(vm, x => x.RemoveCommand)
                .CommandParameter(portForwarder)
                .Icon(new FluentIcon().Icon(Icon.Delete));
        }
    }
}
