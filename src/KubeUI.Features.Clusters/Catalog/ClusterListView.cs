using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Styling;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Clusters.Workspace;


namespace KubeUI.Features.Clusters.Catalog;

public sealed partial class ClusterListView : KubeUIView<ClusterListViewModel>
{
    protected override object Build(ClusterListViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        return new Grid()
            .Rows("Auto,*")
            .Children(
                new StackPanel()
                    .Row(0)
                    .MinHeight(32)
                    .Orientation(Orientation.Horizontal)
                    .Spacing(2)
                    .Margin(2, 2, 0, 2)
                    .Children(
                        new Button()
                            .Content(new FluentIcon().Icon(Icon.AddSquare))
                            .ToolTip_Tip(ClustersLanguageManager.Instance.ClusterListView_AddKubeConfig.ToBinding())
                            .Command(vm, x => x.AddKubeConfigCommand),
                        new DropDownButton()
                            .Content(new FluentIcon().Icon(Icon.Delete))
                            .ToolTip_Tip(ClustersLanguageManager.Instance.ClusterListView_RemoveKubeConfig.ToBinding())
                            .IsEnabled(vm, x => x.RemovableKubeConfigs.Count, BindingMode.OneWay, new FuncValueConverter<int, bool>(count => count > 0))
                            .Flyout(new MenuFlyout()
                                .ItemsSource(vm.RemovableKubeConfigs)
                                .ItemContainerTheme(new ControlTheme(typeof(MenuItem))
                                {
                                    BasedOn = Application.Current?.FindResource(typeof(MenuItem)) as ControlTheme,
                                    Setters =
                                    {
                                        new Setter(global::Avalonia.Controls.Primitives.HeaderedSelectingItemsControl.HeaderProperty, CompiledBinding.Create<string, string>(x => x)),
                                        new Setter(MenuItem.CommandProperty, vm.RemoveKubeConfigCommand),
                                        new Setter(MenuItem.CommandParameterProperty, CompiledBinding.Create<string, string>(x => x)),
                                    }
                                }))),
                new DynamicTableView
                {
                    ContextMenuItemsFactory = targets => CreateContextMenuItems(vm, targets),
                    KeyBindings =
                    {
                        SelectionKeyBindingFactory.Create(Key.Delete, vm.DeleteCommand, () => vm.SelectedItem)
                    }
                }
                    .GridLinesVisibility(DynamicTableViewGridLinesVisibility.All)
                    .Source(vm.TableSource)
                    .ContextMenu(new ContextMenu())
                    .Row(1));
    }

    private static IEnumerable<MenuItem> CreateContextMenuItems(ClusterListViewModel vm, IReadOnlyList<object> targets)
    {
        foreach (var cluster in targets.OfType<ClusterWorkspace>())
        {
            yield return new MenuItem()
                .Command(vm, x => x.DeleteCommand)
                .CommandParameter(cluster)
                .Header(ClustersLanguageManager.Instance.ClusterListView_Delete.ToBinding())
                .InputGesture(new KeyGesture(Key.Delete))
                .Icon(new FluentIcon().Icon(Icon.Delete));
        }
    }
}
