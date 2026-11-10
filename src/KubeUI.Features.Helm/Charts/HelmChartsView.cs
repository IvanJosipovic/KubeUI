using System.Diagnostics.CodeAnalysis;
using Avalonia.Xaml.Interactions.Core;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Helm.NET;
using KubeUI.Controls.Layout;
using KubeUI.Controls.Presentation;

namespace KubeUI.Features.Helm.Charts;

/// <summary>Repository-filtered Helm chart search with install actions.</summary>
public sealed class HelmChartsView : KubeUIView<HelmChartsViewModel>
{
    /// <inheritdoc />
    [RequiresUnreferencedCode("DynamicTableView creates columns from runtime Helm chart metadata.")]
    protected override object Build(HelmChartsViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        DynamicTableView table = new()
        {
            GridLinesVisibility = DynamicTableViewGridLinesVisibility.All,
            Source = vm.TableSource,
            ContextMenu = new ContextMenu(),
            ContextMenuItemsFactory = targets => targets.OfType<HelmChartSearchResult>().Select(chart =>
                new MenuItem()
                    .Header(HelmLanguageManager.Instance.HelmChartsView_Install.ToBinding())
                    .Command(vm, x => x.InstallChartCommand)
                    .CommandParameter(chart)
                    .Icon(new FluentIcon().Icon(Icon.AddSquare)))
        };
        var root = new Grid()
            .Rows("Auto,*").Children(
            new CompactToolbar
            {
                Middle = new Label()
                    .Content(CompiledBinding.Create<HelmChartsViewModel, int>(x => x.ItemCount,
                        source: vm, stringFormat: HelmLanguageManager.Instance.HelmChartsView_ItemsFormat.CurrentValue()!)),
                Trailing = CompactToolbar.Group(
                        new TextBox()
                            .Width(180)
                            .MinWidth(120)
                            .PlaceholderText(HelmLanguageManager.Instance.HelmChartsView_SearchPlaceholder.ToBinding())
                            .Text(vm, x => x.SearchText, BindingMode.TwoWay),
                        new ComboBox().Name("Repository", Scope)
                            .Width(200)
                            .MinWidth(140)
                            .Classes("ClearButton")
                            .PlaceholderText(HelmLanguageManager.Instance.HelmReleaseEditorView_Repository.ToBinding())
                            .IsEnabled(vm, x => !x.IsLoading)
                            .ItemsSource(vm, x => x.Repositories)
                            .SelectedItem(vm, x => x.SelectedRepository, BindingMode.TwoWay),
                        new Button()
                            .Command(vm, x => x.RefreshCommand)
                            .Content(new FluentIcon().Icon(Icon.ArrowSync))
                            .ToolTip_Tip(FoundationLanguageManager.Instance.Shared_Refresh.ToBinding()))
                    .MaxWidth(760)
            },
            table.Row(1));
        Interaction.GetBehaviors(root).Add(new EventTriggerBehavior
        {
            EventName = nameof(Loaded),
            Actions = [new InvokeCommandAction { Command = vm.RefreshCommand }]
        });
        return root;
    }

}
