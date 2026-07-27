using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Svg.Skia;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

public sealed partial class VisualizationView : ViewBase<VisualizationViewModel>
{
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
                new Label()
                    .Width(200)
                    .VerticalContentAlignment(VerticalAlignment.Center)
                    .BindValue(ContentControl.ContentProperty, CompiledBinding.Create<VisualizationViewModel, int>(x => x.Graph!.Resources.Count,
                        source: vm,
                        stringFormat: Assets.Resources.VisualizationView_ItemsFormat)));
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
                    .ItemsSource(vm, x => x.Cluster.Runtime.Namespaces)
                    .SelectedItems(vm, x => x.Cluster.SelectedNamespaces)
                    .SelectedItemTemplate(template)
                    .ItemTemplate(template)
                    .IsVisible(vm, x => x.RootResource, converter: Converters.Converters.IsNull),
                new TextBlock()
                    .Name("ResourceToolbarText")
                    .Text(vm, x => x.RootResourceDisplay)
                    .IsVisible(vm, x => x.RootResource, converter: Converters.Converters.NotNull));
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "ResourceGraphControl ownership is transferred to the visual tree via fluent builder.")]
    private static ResourceGraphControl CreateGraphViewer(VisualizationViewModel vm)
    {
        return new ResourceGraphControl()
            .Row(1)
            .BindValue(ResourceGraphControl.GraphProperty, CompiledBinding.Create<VisualizationViewModel, KubeUI.Kubernetes.Resources.Relationships.ResourceRelationshipGraph?>(x => x.Graph, source: vm));
    }

    internal static StackPanel CreateResourceNode(ResourceNodeViewModel node)
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

        return new StackPanel()
            .BindValue(ToolTip.TipProperty, content)
            .Width(128)
            .Height(128)
            .Children(
                new Image()
                    .Width(64)
                    .Height(64)
                    .Source(node, x => x.IconPath, BindingMode.OneWay, new ResourceIconToSvgImageConverter())
                    .ContextFlyout(ResourceActionPresenter.CreateFlyout(node.ContextMenuItems)),
                new TextBlock()
                    .TextAlignment(TextAlignment.Center)
                    .TextWrapping(TextWrapping.Wrap)
                    .Text(node, x => x.Resource.Kind),
                new TextBlock()
                    .TextAlignment(TextAlignment.Center)
                    .TextWrapping(TextWrapping.Wrap)
                    .Text(node, x => x.Resource.Metadata.Name)
            );
    }

    private sealed class ResourceIconToSvgImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string path || string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            return new SvgImage
            {
                Source = SvgSource.Load(path, new Uri("avares://KubeUI.Avalonia"))
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    private async Task InitializeDesignTimeDataAsync()
    {
        DataContext = await DesignTimePreview.CreateClusterBoundViewModelAsync<VisualizationViewModel, V1Pod>();
    }
}
