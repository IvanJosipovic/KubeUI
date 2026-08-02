using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Xaml.Interactivity;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure;
using Ursa.Controls;
using AvaloniaStyles = Avalonia.Styling.Styles;
using LayoutOrientation = Avalonia.Layout.Orientation;
using NumericUpDown = Ursa.Controls.NumericUpDown;

namespace KubeUI.Avalonia.Styles;

public sealed class Fluent : AvaloniaStyles
{
    public Fluent()
    {
        Resources = new ResourceDictionary
        {
            { "DataGridFilterFlyoutPresenterTheme", CreateDataGridFilterFlyoutPresenterTheme() }
        };

        Add(new Style<DocumentControl>()
            .Setter(DocumentControl.HeaderTemplateProperty, new FuncDataTemplate<IDockable>((dockable, _) => CreateDocumentHeader(dockable!), false)));

        Add(new Style(x => x.OfType<HostWindow>().Class(":toolwindow"))
            .Setter(global::Avalonia.Controls.Primitives.TemplatedControl.BackgroundProperty, new DynamicResourceExtension("SystemRegionBrush"))
            .Setter(Visual.OpacityProperty, 1d)
            .Setter(TopLevel.RequestedThemeVariantProperty, CompiledBinding.Create<Application, ThemeVariant?>(x => x.RequestedThemeVariant, source: Application.Current))
            .Setter(TopLevel.TransparencyLevelHintProperty, new[] { WindowTransparencyLevel.None }));

        Add(new Style<DataGrid>()
            .Setter(DataGrid.RowHeightProperty, new DynamicResourceExtension("DataGridRowHeight"))
            .Setter(global::Avalonia.Controls.Primitives.TemplatedControl.FontSizeProperty, new DynamicResourceExtension("DataGridFontSize")));

        Add(new Style<DataGridColumnHeader>()
            .Setter(global::Avalonia.Controls.Primitives.TemplatedControl.FontSizeProperty, new DynamicResourceExtension("DataGridFontSize"))
            .Setter(Layoutable.MinHeightProperty, new DynamicResourceExtension("DataGridColumnHeaderMinHeight")));

        Add(new Style(x => x.OfType<DataGrid>().Descendant().Name("CellTextBlock"))
            .Setter(TextBlock.MaxLinesProperty, 1)
            .Setter(TextBlock.FontSizeProperty, new DynamicResourceExtension("DataGridFontSize")));

        Add(new Style(x => x.OfType<StackPanel>().Class("filter-flyout-root"))
            .Setter(Layoutable.MinWidthProperty, 296d)
            .Setter(Layoutable.MaxWidthProperty, 296d)
            .Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Left)
            .Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Top)
            .Setter(StackPanel.SpacingProperty, 8d)
            .Setter(Layoutable.MarginProperty, new Thickness(4)));

        Add(new Style(x => x.OfType<TextBlock>().Class("filter-flyout-title"))
            .Setter(TextBlock.FontWeightProperty, FontWeight.SemiBold));

        Add(new Style(x => x.OfType<Grid>().Class("filter-flyout-row"))
            .Setter(Grid.ColumnSpacingProperty, 8d)
            .Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));

        Add(new Style(x => x.OfType<TextBlock>().Class("filter-flyout-label"))
            .Setter(Layoutable.WidthProperty, 72d)
            .Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Center));

        Add(new Style(x => x.OfType<ComboBox>().Class("filter-flyout-editor"))
            .Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));

        Add(new Style(x => x.OfType<TextBox>().Class("filter-flyout-editor"))
            .Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));

        Add(new Style(x => x.OfType<NumericUpDown>().Class("filter-flyout-editor"))
            .Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));

        Add(new Style(x => x.OfType<Grid>().Class("filter-flyout-composite-editor"))
            .Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));

        Add(new Style(x => x.OfType<StackPanel>().Class("filter-flyout-actions"))
            .Setter(StackPanel.OrientationProperty, LayoutOrientation.Horizontal)
            .Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Right)
            .Setter(StackPanel.SpacingProperty, 6d));

        Add(new Style(x => x.OfType<Button>().Class("filter-flyout-action"))
            .Setter(Layoutable.MinWidthProperty, 76d));

        Add(new Style<MultiComboBoxItem>()
            .Setter(global::Avalonia.Controls.Primitives.TemplatedControl.PaddingProperty, new Thickness(4, 2, 4, 2))
            .Setter(Layoutable.WidthProperty, 200d)
            .Setter(Layoutable.MinHeightProperty, 22d));

        Add(new Style<MultiComboBoxSelectedItemList>()
            .Setter(Interaction.BehaviorsProperty,
                new BehaviorCollectionTemplate()
                {
                    Content = (IServiceProvider? _) =>
                        new TemplateResult<BehaviorCollection>(
                        [
                            new ToggleMultiComboBoxBehavior()
                        ],
                        new NameScope())
                }
            ));
    }

    private static ControlTheme CreateDataGridFilterFlyoutPresenterTheme()
    {
        ControlTheme theme = new(typeof(FlyoutPresenter))
        {
            BasedOn = Application.Current!.FindResource(typeof(FlyoutPresenter)) as ControlTheme,
        };

        theme.Setters.Add(new Setter(global::Avalonia.Controls.Primitives.TemplatedControl.PaddingProperty, new Thickness(0)));
        theme.Setters.Add(new Setter(Layoutable.MinWidthProperty, 0d));
        theme.Setters.Add(new Setter(Layoutable.MinHeightProperty, 0d));
        theme.Setters.Add(new Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Left));
        theme.Setters.Add(new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Top));

        return theme;
    }

    private static StackPanel CreateDocumentHeader(IDockable dockable)
    {
        var cluster = GetCluster(dockable);

        return new StackPanel()
            .VerticalAlignment(VerticalAlignment.Center)
            .Orientation(LayoutOrientation.Horizontal)
            .Children(
                CreateClusterIndicator(cluster),
                new TextBlock()
                    .VerticalAlignment(VerticalAlignment.Center)
                    .BindValue(TextBlock.TextProperty, CompiledBinding.Create<IDockable, string?>(x => x.Title, source: dockable)));
    }

    private static Rectangle CreateClusterIndicator(ClusterWorkspace? cluster)
    {
        var indicator = new Rectangle()
            .Width(10)
            .Height(10)
            .Margin(0, 0, 2, 0)
            .IsVisible(cluster != null)
            .Stroke(Brushes.Gray)
            .StrokeThickness(0.6);

        return cluster == null
            ? indicator
            : indicator
                .Fill(cluster, x => x.ClusterColor)
                .ToolTip_Tip(cluster, x => x.Runtime.Name);
    }

    private static ClusterWorkspace? GetCluster(IDockable dockable)
    {
        return dockable.GetType().GetProperty("Cluster")?.GetValue(dockable) as ClusterWorkspace;
    }
}
