using System.Linq.Expressions;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
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
                            .Content(Assets.Resources.MRDiffDetectionView_Clear)),
                new DataGrid()
                    .Row(1)
                    .CanUserReorderColumns(true)
                    .CanUserResizeColumns(true)
                    .CanUserSortColumns(true)
                    .GridLinesVisibility(DataGridGridLinesVisibility.All)
                    .IsReadOnly(true)
                    .ItemsSource(vm, x => x.Rows)
                    .Columns([
                        CreateColumn<CrossplaneDiffRow, string>(x => x.Name, Assets.Resources.MRDiffDetectionView_Name),
                        CreateColumn<CrossplaneDiffRow, string>(x => x.Namespace, Assets.Resources.MRDiffDetectionView_Namespace),
                        CreateColumn<CrossplaneDiffRow, string>(x => x.ApiVersion, Assets.Resources.MRDiffDetectionView_ApiVersion),
                        CreateColumn<CrossplaneDiffRow, string>(x => x.Kind, Assets.Resources.MRDiffDetectionView_Kind),
                        CreateColumn<CrossplaneDiffRow, string>(x => x.DiffField, Assets.Resources.MRDiffDetectionView_DiffField),
                        CreateColumn<CrossplaneDiffRow, string>(x => x.OldValue, Assets.Resources.MRDiffDetectionView_OldValue),
                        CreateColumn<CrossplaneDiffRow, string>(x => x.NewValue, Assets.Resources.MRDiffDetectionView_NewValue),
                        CreateColumn<CrossplaneDiffRow, bool>(x => x.NewComputed, Assets.Resources.MRDiffDetectionView_NewComputed),
                        CreateColumn<CrossplaneDiffRow, bool>(x => x.NewRemoved, Assets.Resources.MRDiffDetectionView_NewRemoved),
                        CreateColumn<CrossplaneDiffRow, bool>(x => x.RequiresNew, Assets.Resources.MRDiffDetectionView_RequiresNew),
                        CreateColumn<CrossplaneDiffRow, bool>(x => x.Sensitive, Assets.Resources.MRDiffDetectionView_Sensitive),
                        CreateColumn<CrossplaneDiffRow, int>(x => x.InstanceCount, Assets.Resources.MRDiffDetectionView_InstanceCount),
                        CreateColumn<CrossplaneDiffRow, int>(x => x.Occurrences, Assets.Resources.MRDiffDetectionView_Occurrences)
                    ]));
    }

    private static DataGridTextColumn CreateColumn<TItem, TValue>(Expression<Func<TItem, TValue>> binding, string? header)
    {
        return new DataGridTextColumn
        {
            Binding = CompiledBinding.Create(binding),
            Header = header,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        };
    }
}
