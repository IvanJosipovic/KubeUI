using System.Globalization;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Svg.Skia;
using AvaloniaGraphControl;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s.Models;
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
                    .BindValue(ContentControl.ContentProperty, CompiledBinding.Create<VisualizationViewModel, int>(x => x.Resources.Count,
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
                    .ItemTemplate(template));
    }

    private static ScrollViewer CreateGraphViewer(VisualizationViewModel vm)
    {
        return new ScrollViewer()
            .Row(1)
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
            .Content(new ZoomBorder
            {
                ClipToBounds = true,
                EnableConstrains = false,
                EnableDoubleClickZoom = false,
                EnableGestures = true,
                Focusable = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Stretch = StretchMode.None,
                VerticalAlignment = VerticalAlignment.Stretch,
                WheelBehavior = WheelBehaviorMode.PanVertical,
                WheelWithShift = WheelBehaviorMode.PanHorizontal,
                WheelPanSensitivity = 2.0,
                WheelZoomSensitivity = 2.0,
                GestureRecognizers =
                {
                    new PinchGestureRecognizer()
                },
                Child = new GraphPanel()
                    .BindValue(GraphPanel.GraphProperty, CompiledBinding.Create<VisualizationViewModel, Graph>(x => x.Graph))
                    .BindValue(GraphPanel.LayoutMethodProperty, CompiledBinding.Create<VisualizationViewModel, GraphPanel.LayoutMethods>(x => x.LayoutMethod))
                    .DataTemplates(
                        new FuncDataTemplate<VisualizationViewModel.ResourceNodeViewModel>((node, _) => CreateResourceNode(node!)),
                        new FuncDataTemplate<Edge>((_, _) => new Connection { Brush = Brushes.Green })
                    )
            });
    }

    private static StackPanel CreateResourceNode(VisualizationViewModel.ResourceNodeViewModel node)
    {
        return new StackPanel()
            .Width(64)
            .Margin(40, 16, 40, 16)
            .BindValue(ToolTip.TipProperty, new MultiBinding
            {
                StringFormat = "{0}/{1} {2}",
                Bindings =
                {
                    CompiledBinding.Create<VisualizationViewModel.ResourceNodeViewModel, string?>(x => x.Resource.ApiVersion),
                    CompiledBinding.Create<VisualizationViewModel.ResourceNodeViewModel, string?>(x => x.Resource.Kind),
                    CompiledBinding.Create<VisualizationViewModel.ResourceNodeViewModel, string?>(x => x.Resource.Metadata.Name)
                }
            })
            .Children(
                new Image()
                    .Width(64)
                    .Source(node, x => x.IconPath, BindingMode.OneWay, new ResourceIconToSvgImageConverter())
                    .ContextFlyout(new MenuFlyout
                    {
                        Items =
                        {
                            new MenuItem()
                                .Command(node, x => x.ViewPropertiesCommand)
                                .CommandParameter(node, x => x.Resource)
                                .Header(Assets.Resources.VisualizationView_Properties),
                            new MenuItem()
                                .Command(node, x => x.ViewYamlCommand)
                                .CommandParameter(node, x => x.Resource)
                                .Header(Assets.Resources.VisualizationView_ViewYaml)
                        }
                    }),
                new TextBlock()
                    .Width(128)
                    .ClipToBounds(false)
                    .Text(node, x => x.Resource.Kind)
                    .TextAlignment(TextAlignment.Center)
                    .TextWrapping(TextWrapping.NoWrap),
                new TextBlock()
                    .Width(128)
                    .ClipToBounds(false)
                    .Text(node, x => x.Resource.Metadata.Name)
                    .TextAlignment(TextAlignment.Center)
                    .TextWrapping(TextWrapping.Wrap));
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
