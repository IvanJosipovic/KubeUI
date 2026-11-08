using System.Diagnostics.CodeAnalysis;
using Avalonia.Animation;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Controls.DependencyInjection;
using KubeUI.Controls.Layout;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Clusters.Presentation;

namespace KubeUI.Features.Resources.Visualization;

public sealed partial class VisualizationView : KubeUIView<VisualizationViewModel>
{
    private static readonly FuncValueConverter<bool, double> NotReadyBorderOpacityConverter = new(
        isNotReady => isNotReady ? 1d : 0d);
    private static readonly FuncValueConverter<bool, double> UpdateFlashOpacityConverter = new(
        isUpdated => isUpdated ? 1d : 0d);

    public VisualizationView()
    {
        DesignTimePreview.Run(InitializeDesignTimeDataAsync);
    }

    protected override object Build(VisualizationViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .Rows("Auto,*")
            .Children(
                CreateToolbar(vm),
                CreateGraphViewer(vm),
                CreateErrorViewer(vm));
    }

    private static CompactToolbar CreateToolbar(VisualizationViewModel vm)
    {
        return new CompactToolbar
        {
            Leading = CompactToolbar.Group(
                new ToggleButton()
                    .IsChecked(vm, x => x.HideNoise, BindingMode.TwoWay)
                    .ToolTip_Tip(ResourcesLanguageManager.Instance.VisualizationView_HideNoiseTooltip.ToBinding())
                    .Content(new FluentIcon().Icon(Icon.EyeOff)),
                new ToggleButton()
                    .IsChecked(vm, x => x.ShowNotReadyOnly, BindingMode.TwoWay)
                    .ToolTip_Tip(ResourcesLanguageManager.Instance.VisualizationView_ShowNotReadyOnlyTooltip.ToBinding())
                    .Content(new FluentIcon().Icon(Icon.Warning)),
                new Label()
                    .Content(CompiledBinding.Create<VisualizationViewModel, int>(x => x.Graph!.Resources.Count,
                        source: vm,
                        stringFormat: ResourcesLanguageManager.Instance.VisualizationView_ItemsFormat.CurrentValue()!)),
                new Label()
                    .Content(vm, x => x.RootResourceDisplay)
                    .IsVisible(vm, x => x.IsRootResourceDisplayVisible)),
            Trailing = CompactToolbar.Group(
                new Ursa.Controls.MultiComboBox()
                    .Width(200)
                    .Classes("ClearButton")
                    .ToolTip_Tip(ResourcesLanguageManager.Instance.VisualizationView_ResourceTypesTooltip.ToBinding())
                    .ItemsSource(vm, x => x.ResourceTypes)
                    .SelectedItems(vm, x => x.SelectedResourceTypes)
                    .IsEnabled(vm, x => x.HasResourceTypes),
                new NamespaceScopePicker()
                    .BindValue(NamespaceScopePicker.NamespacesProperty, CompiledBinding.Create<VisualizationViewModel, IEnumerable>(x => x.Cluster.Runtime.Namespaces, source: vm, mode: BindingMode.OneWay))
                    .BindValue(NamespaceScopePicker.SelectedNamespacesProperty, CompiledBinding.Create<VisualizationViewModel, IList>(x => x.SelectedNamespaces, source: vm, mode: BindingMode.OneWay))
                    .BindValue(NamespaceScopePicker.IsSelectionLinkedProperty, CompiledBinding.Create<VisualizationViewModel, bool>(x => x.IsNamespaceSelectionLinked, source: vm, mode: BindingMode.TwoWay))
                    .BindValue(NamespaceScopePicker.IsSelectionEnabledProperty, CompiledBinding.Create<VisualizationViewModel, bool>(x => x.IsNamespaceSelectionEnabled, source: vm))
                    .BindValue(NamespaceScopePicker.IsLinkEnabledProperty, CompiledBinding.Create<VisualizationViewModel, bool>(x => x.IsNamespaceSelectionEnabled, source: vm))
                    .IsVisible(vm, x => x.IsNamespaceSelectorVisible))
                .MaxWidth(420)
        };
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "ResourceGraphControl ownership is transferred to the visual tree via fluent builder.")]
    private static ResourceGraphControl CreateGraphViewer(VisualizationViewModel vm)
    {
        return new ResourceGraphControl()
            .Row(1)
            .BindValue(ResourceGraphControl.GraphProperty, CompiledBinding.Create<VisualizationViewModel, Kubernetes.Resources.Relationships.ResourceRelationshipGraph?>(x => x.Graph, source: vm));
    }

    private static Border CreateErrorViewer(VisualizationViewModel vm)
    {
        return new Border()
            .Row(1)
            .Padding(12)
            .IsVisible(vm, x => x.ErrorMessage, converter: Controls.Converters.Converters.NotNull)
            .Child(new Grid()
                .Rows("Auto,*")
                .Children(
                    new TextBlock()
                        .Row(0)
                        .Text(ResourcesLanguageManager.Instance.VisualizationView_ErrorHeader.ToBinding()),
                    new SelectableTextBlock()
                        .Row(1)
                        .Text(vm, x => x.ErrorMessage)
                        .TextWrapping(TextWrapping.Wrap)));
    }

    internal static Border CreateResourceNode(ResourceNodeViewModel node)
    {

        var content = new MultiBinding
        {
            StringFormat = "{0}\n{1}",
            Bindings =
                {
                    CompiledBinding.Create<ResourceNodeViewModel, string?>(x => x.Resource.Kind, node),
                    CompiledBinding.Create<ResourceNodeViewModel, string?>(x => x.Resource.Metadata.Name, node)
                }
        };

        return new Border()
            .BorderBrush(Brushes.Transparent)
            .BorderThickness(2)
            .Child(
                new Grid()
                    .Width(128)
                    .Height(128)
                    .Children(
                        new Border
                        {
                            Transitions = new Transitions
                            {
                                new DoubleTransition
                                {
                                    Property = OpacityProperty,
                                    Duration = TimeSpan.FromMilliseconds(250),
                                },
                            },
                        }
                            .Background(new DynamicResourceExtension("SystemBaseMediumHighColor"))
                            .Opacity(CompiledBinding.Create<ResourceNodeViewModel, bool>(
                                x => x.IsUpdated,
                                source: node,
                                converter: UpdateFlashOpacityConverter)),
                        new Border
                        {
                            BorderThickness = new Thickness(2),
                        }
                            .BorderBrush(new DynamicResourceExtension("SystemChromeHighColor"))
                            .Opacity(CompiledBinding.Create<ResourceNodeViewModel, bool>(
                                x => x.IsNotReady,
                                source: node,
                                converter: NotReadyBorderOpacityConverter)),
                        ResourceGraphNodeContent.Create(
                                new Image().Source(node, x => x.Icon, BindingMode.OneWay),
                                new TextBlock().Text(node, x => x.Resource.Kind),
                                new TextBlock().Text(node, x => x.Resource.Metadata.Name))
                            .ToolTip_Tip(content)
                            .ContextFlyout(MenuPresenter.CreateFlyout(node.ContextMenuItems))));
    }

    private async Task InitializeDesignTimeDataAsync()
    {
        DataContext = await DesignTimePreview.CreateClusterBoundViewModelAsync<VisualizationViewModel, V1Pod>();
    }
}
