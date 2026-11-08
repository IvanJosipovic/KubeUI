using Avalonia.Controls.Templates;
using Avalonia.Input;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Controls.Layout;
using KubeUI.Controls.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Crossplane.MRDiffDetection;

public sealed class MRDiffDetectionView : KubeUIView<MRDiffDetectionViewModel>
{
    protected override object Build(MRDiffDetectionViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .Rows("Auto,*")
            .Children(
                new CompactToolbar
                {
                    Leading = CompactToolbar.Group(
                        new ComboBox()
                            .Width(260)
                            .PlaceholderText(CrossplaneLanguageManager.Instance.MRDiffDetectionView_SelectProvider.ToBinding())
                            .ItemsSource(vm, x => x.Providers)
                            .SelectedItem(vm, x => x.SelectedProvider, BindingMode.TwoWay)
                            .ItemTemplate(new FuncDataTemplate<CrossplaneProviderOption>(
                                (provider, _) => new TextBlock().Text(provider?.Name ?? string.Empty))),
                        new TextBox()
                            .Width(240)
                            .Background(Brushes.Transparent)
                            .PlaceholderText(CrossplaneLanguageManager.Instance.MRDiffDetectionView_Search.ToBinding())
                            .Text(vm, x => x.SearchQuery, BindingMode.TwoWay)),
                    Middle = new TextBlock()
                        .MaxWidth(400)
                        .TextWrapping(TextWrapping.Wrap)
                        .Text(vm, x => x.Status),
                    Trailing = new Button()
                        .Command(vm, x => x.ClearCommand)
                        .ToolTip_Tip(CrossplaneLanguageManager.Instance.MRDiffDetectionView_Clear.ToBinding())
                        .Content(new FluentIcon().Icon(Icon.Broom))
                },
                new DynamicTableView
                {
                    ContextMenuItemsFactory = targets => CreateContextMenuItems(vm, targets),
                    KeyBindings =
                    {
                        SelectionKeyBindingFactory.Create(
                            Key.Enter,
                            vm.ViewYamlCommand,
                            () => vm.TableSource.SelectionModel.SelectedItem)
                    }
                }
                    .GridLinesVisibility(DynamicTableViewGridLinesVisibility.All)
                    .Source(vm.TableSource)
                    .ContextMenu(new ContextMenu())
                    .Row(1));
    }

    private static IEnumerable<MenuItem> CreateContextMenuItems(
        MRDiffDetectionViewModel viewModel,
        IReadOnlyList<object> targets)
    {
        foreach (var row in targets.OfType<CrossplaneDiffRow>())
        {
            yield return new MenuItem()
                .Header(ResourcesLanguageManager.Instance.ResourceConfigBase_MenuItem_ViewYaml.ToBinding())
                .Command(viewModel, x => x.ViewYamlCommand)
                .CommandParameter(row)
                .InputGesture(new KeyGesture(Key.Enter));
        }
    }
}
