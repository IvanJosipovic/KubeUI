using System.Globalization;
using System.Reactive.Linq;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Styling;
using Avalonia.Svg.Skia;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Avalonia.Features.Clusters.Catalog.Behaviors;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Shell.Navigation;

public sealed class NavigationView : ViewBase<NavigationViewModel>
{
    public NavigationView()
    {
        if (Design.IsDesignMode)
        {
            DataContext = DesignTimePreview.Get<NavigationViewModel>();
        }
    }

    protected override object Build(NavigationViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        var treeView = new TreeView()
            .Margin(new Thickness(-5, 0, 0, 0))
            .AutoScrollToSelectedItem(false)
            .ItemsSource(vm, x => x.Clusters)
            .Styles(
                new Style<TreeViewItem>()
                    .Setter(TreeViewItem.IsExpandedProperty, CompiledBinding.Create<IExpandableNavigationNode, bool>(x => x.IsExpanded)),
                new Style<Image>()
                    .Setter(HeightProperty, 16.0)
                    .Setter(WidthProperty, 16.0)
                    .Setter(MarginProperty, new Thickness(0, 0, 4, 0)),
                new Style<FluentIcon>()
                    .Setter(HeightProperty, 16.0)
                    .Setter(WidthProperty, 16.0)
                    .Setter(MarginProperty, new Thickness(0, 0, 4, 0))
            )
            .DataTemplates(
                CreateClusterTemplate(),
                CreateResourceTemplate(),
                CreateNavigationItemTemplate()
            )
            .OnSelectionChanged((args) => vm.HandleSelectionChangedCommand.Execute(args))
            .AddBehaviors(
                new TreeViewItemContextMenuBehavior()
            );

        return treeView;
    }

    private static FuncTreeDataTemplate<ClusterNavigationNode> CreateClusterTemplate()
    {
        return new FuncTreeDataTemplate<ClusterNavigationNode>(
            (node, _) =>
            new Border()
                .Margin(-5, 0, 0, 0)
                .HorizontalAlignment(HorizontalAlignment.Stretch)
                .Background(Brushes.Transparent)
                .ContextMenu(new ContextMenu()
                    .Items(
                        new MenuItem()
                            .Header(node, x => x.ConnectionMenuHeader)
                            .Command(node, x => x.ToggleConnectionCommand)
                            .CommandParameter(node)
                            .Icon(new FluentIcon().Icon(node, x => x.ConnectionMenuIcon)),
                        new MenuItem()
                            .Header(Assets.Resources.NavigationView_ContextMenu_Settings)
                            .Command(node, x => x.OpenSettingsCommand)
                            .CommandParameter(node)
                            .Icon(new FluentIcon().Icon(Icon.Settings))
                    ))
                .Child(
                    new StackPanel()
                        .VerticalAlignment(VerticalAlignment.Center)
                        .Orientation(Orientation.Horizontal)
                        .Children(
                            new Rectangle()
                                .Width(10)
                                .Height(10)
                                .Margin(0, 0, 4, 0)
                                .Fill(node, x => x.Cluster.ClusterColor)
                                .Stroke(Brushes.Gray)
                                .StrokeThickness(0.6),
                            new TextBlock()
                                .VerticalAlignment(VerticalAlignment.Center)
                                .Text(node, x => x.Cluster.Name))
                )
                .ToolTip_Tip(node, x => x.Cluster.Status, BindingMode.OneWay, new FuncValueConverter<ClusterStatus, string>(status =>
                {
                    return status switch
                    {
                        ClusterStatus.None => Assets.Resources.NavigationView_ToolTip_ClickToConnect!,
                        ClusterStatus.Connecting => Assets.Resources.NavigationView_ToolTip_Connecting!,
                        ClusterStatus.Errored => Assets.Resources.NavigationView_ToolTip_Errored!,
                        ClusterStatus.Connected => Assets.Resources.NavigationView_ToolTip_Connected!,
                        _ => string.Empty
                    };
                })),
            node => node.NavigationItems);
    }

    private static FuncTreeDataTemplate<ResourceNavigationLink> CreateResourceTemplate()
    {
        return new FuncTreeDataTemplate<ResourceNavigationLink>(
            (link, _) =>
                new Border()
                    .Margin(-5, 0, 0, 0)
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .Background(Brushes.Transparent)
                    .ContextMenu(new ContextMenu()
                        .Items([
                            new MenuItem()
                                .Header(Assets.Resources.NavigationView_ContextMenu_Open)
                                .Command(link, x => x.OpenCommand)
                                .CommandParameter(link)
                                .Icon(new FluentIcon().Icon(Icon.DocumentEdit)),
                            new MenuItem()
                                .Header(Assets.Resources.NavigationView_ContextMenu_OpenNewTab)
                                .Command(link, x => x.OpenInNewTabCommand)
                                .CommandParameter(link)
                                .Icon(new FluentIcon().Icon(Icon.AddSquare))
                        ]))
                        .Child(
                            new StackPanel()
                                .Orientation(Orientation.Horizontal)
                                .Children(
                                    new Image()
                                        .Source(new SvgImage
                                        {
                                            Source = SvgSource.Load(link.IconPath, new Uri("avares://KubeUI.Avalonia"))
                                        }),
                                    new TextBlock()
                                        .VerticalAlignment(VerticalAlignment.Center)
                                        .Text(link, x => x.Name),
                                    new TextBlock()
                                        .VerticalAlignment(VerticalAlignment.Center)
                                        .Margin(4, 0, 0, 0)
                                        .BindValue(TextBlock.TextProperty, new Binding("Count^"))
                                )
                        ),
                link => link.NavigationItems);
    }

    private static FuncTreeDataTemplate<NavigationItem> CreateNavigationItemTemplate()
    {
        return new FuncTreeDataTemplate<NavigationItem>(
            (item, _) =>
                new StackPanel()
                    .Margin(-5, 0, 0, 0)
                    .Orientation(Orientation.Horizontal)
                    .Children(
                        new FluentIcon()
                            .Icon(item, x => x.FluentIcon, BindingMode.OneWay)
                            .IsVisible(item, x => x.FluentIcon, BindingMode.OneWay, Converters.Converters.NotNull),
                        new PathIcon()
                            .Data(item, x => x.StyleIcon, BindingMode.OneWay, new StringToGeometryConverter())
                            .IsVisible(item, x => x.StyleIcon, BindingMode.OneWay, Converters.Converters.NotNull),
                        new Image()
                            .Width(16)
                            .Height(16)
                            .Source(item, x => x.SvgIcon, BindingMode.OneWay, new StringToSvgImageConverter())
                            .IsVisible(item, x => x.SvgIcon, BindingMode.OneWay, Converters.Converters.NotNull),
                        new TextBlock()
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(item, x => x.Name)
                    ),
                item => item.NavigationItems);
    }
}

internal sealed class StringToGeometryConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string geometryText && !string.IsNullOrWhiteSpace(geometryText))
        {
            return Geometry.Parse(geometryText);
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}

internal sealed class StringToSvgImageConverter : IValueConverter
{
    private static readonly Uri AppUri = new("avares://KubeUI.Avalonia");

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrWhiteSpace(path))
        {
            return new SvgImage
            {
                Source = SvgSource.Load(path, AppUri),
            };
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}
