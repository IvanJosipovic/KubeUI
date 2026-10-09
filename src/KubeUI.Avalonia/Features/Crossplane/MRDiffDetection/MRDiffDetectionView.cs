using Avalonia.Controls.Templates;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;

public sealed class MRDiffDetectionView : ViewBase<MRDiffDetectionViewModel>
{
    protected override object Build(MRDiffDetectionViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        DynamicTableView table = new()
        {
            GridLinesVisibility = DynamicTableViewGridLinesVisibility.All,
            Source = vm.TableSource,
            ContextMenu = new ContextMenu(),
            ContextMenuItemsFactory = targets => CreateContextMenuItems(vm, targets)
        };

        return new Grid()
            .Rows("Auto,*")
            .Children(
                new Grid()
                    .Row(0)
                    .MinHeight(32)
                    .Margin(0, 2, 0, 2)
                    .Cols("Auto,Auto,*,Auto")
                    .Children(
                        new ComboBox()
                            .Col(0)
                            .Width(260)
                            .Margin(0, 0, 6, 0)
                            .VerticalAlignment(VerticalAlignment.Stretch)
                            .VerticalContentAlignment(VerticalAlignment.Center)
                            .PlaceholderText(Assets.Resources.MRDiffDetectionView_SelectProvider)
                            .ItemsSource(vm, x => x.Providers)
                            .SelectedItem(vm, x => x.SelectedProvider, BindingMode.TwoWay)
                            .ItemTemplate(new FuncDataTemplate<CrossplaneProviderOption>(
                                (provider, _) => new TextBlock().Text(provider?.Name ?? string.Empty))),
                        new TextBox()
                            .Col(1)
                            .Width(240)
                            .Margin(0, 0, 6, 0)
                            .HorizontalAlignment(HorizontalAlignment.Stretch)
                            .VerticalAlignment(VerticalAlignment.Stretch)
                            .VerticalContentAlignment(VerticalAlignment.Center)
                            .Background(Brushes.Transparent)
                            .PlaceholderText(Assets.Resources.MRDiffDetectionView_Search)
                            .Text(vm, x => x.SearchQuery, BindingMode.TwoWay),
                        new TextBlock()
                            .Col(2)
                            .Margin(0, 0, 6, 0)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(vm, x => x.Status),
                        new Button()
                            .Col(3)
                            .VerticalAlignment(VerticalAlignment.Stretch)
                            .Command(vm, x => x.ClearCommand)
                            .ToolTip_Tip(Assets.Resources.MRDiffDetectionView_Clear)
                            .Content(new FluentIcon().Icon(Icon.Broom))),
                table.Row(1));
    }

    private static IEnumerable<MenuItem> CreateContextMenuItems(
        MRDiffDetectionViewModel viewModel,
        IReadOnlyList<object> targets)
    {
        foreach (var row in targets.OfType<CrossplaneDiffRow>())
        {
            yield return new MenuItem()
                .Header(Assets.Resources.ResourceConfigBase_MenuItem_ViewYaml)
                .Command(viewModel, x => x.ViewYamlCommand)
                .CommandParameter(row);
        }
    }
}
