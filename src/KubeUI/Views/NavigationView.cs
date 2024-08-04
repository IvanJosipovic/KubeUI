using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using Avalonia.Svg.Skia;
using KubeUI.Client;

namespace KubeUI.Views;

public sealed class NavigationView : MyViewBase<NavigationViewModel>
{
    private static FuncValueConverter<string?, Geometry> s_resoureConverter => new((x) =>
    {
        if (x == null) return null;
        return (Geometry)Application.Current.FindResource(x);
    });

    private static FuncValueConverter<string?, bool> s_stringNullConverter => new((x) =>
    {
        return string.IsNullOrEmpty(x);
    });

    protected override StyleGroup? BuildStyles() => [
        new Style(x => x.Name("PART_PinnedDockGrid").Child().OfType<GridSplitter>().Child().OfType<Border>())
            .Setter(Border.OpacityProperty, 0.0)
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
                        .Orientation(Orientation.Horizontal)
                        .Children([
                            new CheckBox()
                                .MinHeight(10)
                                .Margin(0,0,4,0)
                                .IsChecked(@vm.Connected)
                                .IsEnabled(false)
                                .Styles([
                                    new Style<CheckBox>().Selector(x => x.Class(":checked").Template().OfType<Ellipse>())
                                        .Setter(Ellipse.FillProperty, Brushes.LimeGreen),
                                    new Style<CheckBox>().Selector(x => x.Class(":unchecked").Template().OfType<Ellipse>())
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
                        .Orientation(Orientation.Horizontal)
                        .Children([
                            new PathIcon()
                                .Width(17)
                                .Height(17)
                                .Margin(0,0,4,0)
                                .Data(@vm.StyleIcon, s_resoureConverter)
                                .IsVisible(@vm.StyleIcon, s_stringNullConverter),
                            new Avalonia.Svg.Skia.Svg(new Uri("/"))
                                .Width(17)
                                .Height(17)
                                .Margin(0,0,4,0)
                                .Set(Avalonia.Svg.Skia.Svg.PathProperty, @vm.SvgIcon)
                                .IsVisible(@vm.SvgIcon, s_stringNullConverter),
                            new TextBlock()
                                .Text(@vm.Name)
                            ])
                ,(x) => x.NavigationItems),
            ])
        ;
}
