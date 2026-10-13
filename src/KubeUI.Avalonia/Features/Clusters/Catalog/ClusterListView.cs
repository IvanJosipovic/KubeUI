using Avalonia.Data.Converters;
using Avalonia.Styling;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;

namespace KubeUI.Avalonia.Features.Clusters.Catalog;

public sealed partial class ClusterListView : ViewBase<ClusterListViewModel>
{
    public ClusterListView()
    {
        if (Design.IsDesignMode)
            DataContext = DesignTimePreview.Get<ClusterListViewModel>();
    }

    protected override object Build(ClusterListViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        return new Grid()
            .Rows("Auto,*")
            .Children(
                new StackPanel()
                    .Row(0)
                    .Orientation(Orientation.Horizontal)
                    .Spacing(8)
                    .Margin(4)
                    .Children(
                        new Button()
                            .Content(new StackPanel()
                                .Orientation(Orientation.Horizontal)
                                .Spacing(4)
                                .Children(
                                    new FluentIcon().Icon(Icon.Add),
                                    new TextBlock().Text(Assets.Resources.ClusterListView_AddKubeConfig)))
                            .Command(vm, x => x.AddKubeConfigCommand),
                        new DropDownButton()
                            .Content(new StackPanel()
                                .Orientation(Orientation.Horizontal)
                                .Spacing(4)
                                .Children(
                                    new FluentIcon().Icon(Icon.Delete),
                                    new TextBlock().Text(Assets.Resources.ClusterListView_RemoveKubeConfig)))
                            .IsEnabled(vm, x => x.RemovableKubeConfigs.Count, BindingMode.OneWay, new FuncValueConverter<int, bool>(count => count > 0))
                            .Flyout(new MenuFlyout()
                                .ItemsSource(vm.RemovableKubeConfigs)
                                .ItemContainerTheme(new ControlTheme(typeof(MenuItem))
                                {
                                    BasedOn = Application.Current?.FindResource(typeof(MenuItem)) as ControlTheme,
                                    Setters =
                                    {
                                        new Setter(MenuItem.HeaderProperty, CompiledBinding.Create<string, string>(x => x)),
                                        new Setter(MenuItem.CommandProperty, vm.RemoveKubeConfigCommand),
                                        new Setter(MenuItem.CommandParameterProperty, CompiledBinding.Create<string, string>(x => x)),
                                    }
                                })),
                        new CheckBox()
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Content(Assets.Resources.ClusterListView_AutoDiscovery)
                            .IsChecked(vm, x => x.KubeConfigAutoDiscoveryEnabled, BindingMode.TwoWay)),
                new DynamicTableView
                {
                    GridLinesVisibility = DynamicTableViewGridLinesVisibility.All,
                    Source = vm.TableSource,
                    ContextMenu = new ContextMenu(),
                    ContextMenuItemsFactory = targets => CreateContextMenuItems(vm, targets)
                }.Row(1));
    }

    private static IEnumerable<MenuItem> CreateContextMenuItems(ClusterListViewModel vm, IReadOnlyList<object> targets)
    {
        foreach (var cluster in targets.OfType<ClusterWorkspace>())
        {
            yield return new MenuItem()
                .Command(vm, x => x.DeleteCommand)
                .CommandParameter(cluster)
                .Header(Assets.Resources.ClusterListView_Delete)
                .Icon(new FluentIcon().Icon(Icon.Delete));
        }
    }
}
