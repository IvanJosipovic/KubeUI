using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using KubeUI.Client;

namespace KubeUI.Views;

public sealed class NavigationView : MyViewBase<NavigationViewModel>
{
    private static FuncValueConverter<string?, Geometry> s_resoureConverter => new((x) =>
    {
        if (x == null) return null;
        return (Geometry)Application.Current.FindResource(x);
    });

    protected override StyleGroup? BuildStyles() => [
        new Style(x => x.OfType<TreeViewItem>())
            .Setter(TreeViewItem.IsExpandedProperty, new Binding(nameof(Cluster.IsExpanded))),

        new Style(x => x.OfType<StackPanel>().Class("navigation-view-stack-panel"))
            .Setter(StackPanel.OrientationProperty, Orientation.Horizontal),

        new Style(x => Selectors.Or(x.OfType<Avalonia.Svg.Skia.Svg>(), x.OfType<PathIcon>()))
            .Setter(Control.HeightProperty, 17.0)
            .Setter(Control.WidthProperty, 17.0)
            .Setter(Control.MarginProperty, new Thickness(0,0,4,0))
        ];

    protected override object Build(NavigationViewModel vm) =>
        new TreeView()
            .ItemsSource(@vm.ClusterManager.Clusters)
            .OnSelectionChanged((e) =>
            {
                if (e.AddedItems.Count == 1 && DataContext is NavigationViewModel model)
                {
                    model.TreeView_SelectionChanged(e.AddedItems[0]);
                }
            })
            .DataTemplates([
                new FuncTreeDataTemplate<Cluster>((vm,ns) =>
                    new StackPanel()
                        .Classes("navigation-view-stack-panel")
                        .Children([
                            new CheckBox()
                                .MinHeight(10)
                                .Margin(0,0,4,0)
                                .IsChecked(@vm.Connected)
                                .IsEnabled(false)
                                .Styles([
                                    new Style<CheckBox>().Selector(x => x.PropertyEquals(CheckBox.IsCheckedProperty, true).Template().OfType<Ellipse>())
                                        .Setter(Ellipse.FillProperty, Brushes.LimeGreen),
                                     new Style<CheckBox>().Selector(x => x.PropertyEquals(CheckBox.IsCheckedProperty, false).Template().OfType<Ellipse>())
                                        .Setter(Ellipse.FillProperty, Brushes.Red)
                                ])
                                .Template(new FuncControlTemplate((_, _) =>
                                                    new Ellipse()
                                                        .Width(10)
                                                        .Height(10)
                                                        .Stroke(Brushes.Gray)
                                                        .StrokeThickness(1)
                                        )
                                ),
                            new TextBlock()
                                .Text(@vm.Name)
                            ])
                ,(x) => x.NavigationItems),

                new FuncTreeDataTemplate<ResourceNavigationLink>((vm,ns) =>
                    new StackPanel()
                        .Classes("navigation-view-stack-panel")
                        .Children([
                            new PathIcon()
                                .Data(@vm.StyleIcon, s_resoureConverter)
                                .IsVisible(@vm.StyleIcon, Utilities.NotNullConverter),
                            new Avalonia.Svg.Skia.Svg(new Uri("avares://KubeUI/"))
                                .Path(@vm.SvgIcon)
                                .IsVisible(@vm.SvgIcon, Utilities.NotNullConverter),
                            new TextBlock()
                                .Text(@vm.Name),
                            new TextBlock()
                                .Text(" "),
                            new TextBlock()
                                .Text(new Binding($"{nameof(ResourceNavigationLink.Objects)}.{nameof(ResourceNavigationLink.Objects.Count)}", BindingMode.OneWay)), //todo fix
                            ])
                ,(x) => x.NavigationItems),

                new FuncTreeDataTemplate<NavigationItem>((vm,ns) =>
                    new StackPanel()
                        .Classes("navigation-view-stack-panel")
                        .Children([
                            new PathIcon()
                                .Data(@vm.StyleIcon, s_resoureConverter)
                                .IsVisible(@vm.StyleIcon, Utilities.NotNullConverter),
                            new Avalonia.Svg.Skia.Svg(new Uri("avares://KubeUI/"))
                                .Path(@vm.SvgIcon)
                                .IsVisible(@vm.SvgIcon, Utilities.NotNullConverter),
                            new TextBlock()
                                .Text(@vm.Name)
                            ])
                ,(x) => x.NavigationItems),
            ])
        ;
}
