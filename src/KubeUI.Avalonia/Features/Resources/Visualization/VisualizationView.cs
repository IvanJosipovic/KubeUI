using System.Diagnostics.CodeAnalysis;
using Avalonia.Animation;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

public sealed partial class VisualizationView : ViewBase<VisualizationViewModel>
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
                CreateLeftToolbar(vm),
                CreateRightToolbar(vm),
                CreateGraphViewer(vm));
    }

    private static StackPanel CreateLeftToolbar(VisualizationViewModel vm)
    {
        return new StackPanel()
            .Row(0)
            .HorizontalAlignment(HorizontalAlignment.Left)
            .Orientation(Orientation.Horizontal)
            .Children(
                new ToggleButton()
                    .IsChecked(vm, x => x.HideNoise, BindingMode.TwoWay)
                    .ToolTip_Tip(Assets.Resources.VisualizationView_HideNoiseTooltip)
                    .Content(new FluentIcon().Icon(Icon.EyeOff)),
                new ToggleButton()
                    .IsChecked(vm, x => x.ShowNotReadyOnly, BindingMode.TwoWay)
                    .ToolTip_Tip(Assets.Resources.VisualizationView_ShowNotReadyOnlyTooltip)
                    .Content(new FluentIcon().Icon(Icon.Warning)),
                new Label()
                    .VerticalContentAlignment(VerticalAlignment.Center)
                    .Content(CompiledBinding.Create<VisualizationViewModel, int>(x => x.Graph!.Resources.Count,
                        source: vm,
                        stringFormat: Assets.Resources.VisualizationView_ItemsFormat)),
                new Label()
                    .Content(vm, x => x.RootResourceDisplay)
                    .VerticalContentAlignment(VerticalAlignment.Center)
                    .IsVisible(vm, x => x.RootResource, converter: Converters.Converters.NotNull));
    }

    private static StackPanel CreateRightToolbar(VisualizationViewModel vm)
    {
        var template = new FuncDataTemplate<V1Namespace>((ns, _) =>
            new TextBlock().Text(ns?.Metadata?.Name ?? string.Empty));

        return new StackPanel()
            .Row(0)
            .HorizontalAlignment(HorizontalAlignment.Right)
            .Orientation(Orientation.Horizontal)
            .Children(
                new Ursa.Controls.MultiComboBox()
                    .Width(200)
                    .MaxHeight(20)
                    .Classes("ClearButton")
                    .ToolTip_Tip(Assets.Resources.VisualizationView_ResourceTypesTooltip)
                    .ItemsSource(vm, x => x.ResourceTypes)
                    .SelectedItems(vm, x => x.SelectedResourceTypes)
                    .IsEnabled(vm, x => x.HasResourceTypes),
                new Ursa.Controls.MultiComboBox()
                    .Width(200)
                    .MaxHeight(20)
                    .Classes("ClearButton")
                    .ItemsSource(vm, x => x.Cluster.Runtime.Namespaces)
                    .SelectedItems(vm, x => x.SelectedNamespaces)
                    .SelectedItemTemplate(template)
                    .ItemTemplate(template)
                    .IsVisible(vm, x => x.IsNamespaceSelectorVisible),
                new ToggleButton()
                    .IsChecked(vm, x => x.IsNamespaceSelectionLinked, BindingMode.TwoWay)
                    .ToolTip_Tip(Assets.Resources.ResourceListView_NamespaceLink)
                    .Content(new FluentIcon().Icon(Icon.Link))
                    .IsVisible(vm, x => x.IsNamespaceSelectorVisible));
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "ResourceGraphControl ownership is transferred to the visual tree via fluent builder.")]
    private static ResourceGraphControl CreateGraphViewer(VisualizationViewModel vm)
    {
        return new ResourceGraphControl()
            .Row(1)
            .BindValue(ResourceGraphControl.GraphProperty, CompiledBinding.Create<VisualizationViewModel, Kubernetes.Resources.Relationships.ResourceRelationshipGraph?>(x => x.Graph, source: vm));
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
                        new StackPanel()
                            .ToolTip_Tip(content)
                            .ContextFlyout(ResourceActionPresenter.CreateFlyout(node.ContextMenuItems))
                            .Children(
                                new Image()
                                    .Width(64)
                                    .Height(64)
                                    .Source(node, x => x.Icon, BindingMode.OneWay),
                                new TextBlock()
                                    .TextAlignment(TextAlignment.Center)
                                    .TextWrapping(TextWrapping.Wrap)
                                    .Text(node, x => x.Resource.Kind),
                                new TextBlock()
                                    .TextAlignment(TextAlignment.Center)
                                    .TextWrapping(TextWrapping.Wrap)
                                    .Text(node, x => x.Resource.Metadata.Name)
                            )));
    }

    private async Task InitializeDesignTimeDataAsync()
    {
        DataContext = await DesignTimePreview.CreateClusterBoundViewModelAsync<VisualizationViewModel, V1Pod>();
    }
}
