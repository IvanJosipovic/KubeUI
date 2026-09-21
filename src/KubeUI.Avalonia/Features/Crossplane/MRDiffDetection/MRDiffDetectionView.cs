using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Controls.DataGridSorting;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;

public sealed class MRDiffDetectionView : ViewBase<MRDiffDetectionViewModel>
{
    protected override object Build(MRDiffDetectionViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .Rows("Auto,*")
            .Children(
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(6)
                    .Children(
                        new ComboBox()
                            .Width(260)
                            .PlaceholderText(Assets.Resources.MRDiffDetectionView_SelectProvider)
                            .ItemsSource(vm, x => x.Providers)
                            .SelectedItem(vm, x => x.SelectedProvider, BindingMode.TwoWay)
                            .ItemTemplate(new FuncDataTemplate<CrossplaneProviderOption>((provider, _) => new TextBlock().Text(provider?.Name ?? string.Empty))),
                        new Button()
                            .Command(vm, x => x.ClearCommand)
                            .ToolTip_Tip(Assets.Resources.MRDiffDetectionView_Clear)
                            .Content(new FluentIcon().Icon(Icon.Broom))),
                new DataGrid
                    {
                        SortingAdapterFactory = vm.SortingAdapterFactory,
                        FilteringAdapterFactory = vm.FilteringAdapterFactory,
                        SearchAdapterFactory = vm.SearchAdapterFactory
                    }
                    .Row(1)
                    .CanUserReorderColumns(true)
                    .CanUserResizeColumns(true)
                    .CanUserSortColumns(true)
                    .GridLinesVisibility(DataGridGridLinesVisibility.All)
                    .IsReadOnly(true)
                    .ColumnDefinitionsSource(vm, x => x.ColumnDefinitions)
                    .ItemsSource(vm, x => x.View)
                    .FilteringModel(vm, x => x.FilteringModel)
                    .SearchModel(vm, x => x.SearchModel)
                    .SortingModel(vm, x => x.SortingModel)
                    .ContextMenu(new ContextMenu())
                    .Behaviors([new MRDiffDetectionContextMenuBehavior()])
                    );
    }
}
