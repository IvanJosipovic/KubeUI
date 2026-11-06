using System.Windows.Input;
using Avalonia.Input;
using Avalonia.VisualTree;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Controls.Presentation;
using KubeUI.Kubernetes;


namespace KubeUI.Features.Resources.Workloads.v1.Pod;

public partial class PortForwarderListView : KubeUIView<PortForwarderListViewModel>
{
    protected override object Build(PortForwarderListViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        var table = new DynamicTableView
        {
            ContextMenuItemsFactory = targets => CreateContextMenuItems(vm, targets)
        }
            .GridLinesVisibility(DynamicTableViewGridLinesVisibility.All)
            .Source(vm.TableSource)
            .ContextMenu(new ContextMenu());
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
        => SelectionKeyBindingFactory.Create(key, command, () => viewModel.SelectedItem);

    private static TableViewRow? FindRow(object? source)
        => source is Visual visual ? visual.GetSelfAndVisualAncestors().OfType<TableViewRow>().FirstOrDefault() : null;

    private static IEnumerable<MenuItem> CreateContextMenuItems(PortForwarderListViewModel vm, IReadOnlyList<object> targets)
    {
        foreach (var portForwarder in targets.OfType<PortForwarder>())
        {
            yield return new MenuItem()
                .Header(L10n.Instance.PortForwarderListView_OpenInBrowser.ToBinding())
                .Command(vm, x => x.OpenCommand)
                .CommandParameter(portForwarder)
                .InputGesture(new KeyGesture(Key.Enter))
                .Icon(new FluentIcon().Icon(Icon.Open));
            yield return new MenuItem()
                .Header(L10n.Instance.PortForwarderListView_Remove.ToBinding())
                .Command(vm, x => x.RemoveCommand)
                .CommandParameter(portForwarder)
                .InputGesture(new KeyGesture(Key.Delete))
                .Icon(new FluentIcon().Icon(Icon.Delete));
        }
    }
}
