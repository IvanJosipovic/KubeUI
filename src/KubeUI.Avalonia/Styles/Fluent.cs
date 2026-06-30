using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;
using Avalonia.Xaml.Interactivity;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
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
            .Setter(DocumentControl.HeaderTemplateProperty, new FuncDataTemplate<IDockable>((_, _) =>
                new StackPanel()
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Orientation(LayoutOrientation.Horizontal)
                    .Children(
                        new Rectangle()
                            .Width(10)
                            .Height(10)
                            .Margin(0, 0, 2, 0)
                            .BindValue(Shape.FillProperty, new Binding("Cluster.ClusterColor"))
                            .BindValue(Rectangle.IsVisibleProperty, new Binding("Cluster") { Converter = Converters.Converters.NotNull, FallbackValue = false })
                            .Stroke(Brushes.Gray)
                            .StrokeThickness(0.6)
                            .BindValue(ToolTip.TipProperty, new Binding("Cluster.Name")),
                        new TextBlock()
                            .VerticalAlignment(VerticalAlignment.Center)
                            .BindValue(TextBlock.TextProperty, new Binding("Title")))
                    , false)));

        Add(new Style(x => x.OfType<HostWindow>().Class(":toolwindow"))
            .Setter(HostWindow.BackgroundProperty, new DynamicResourceExtension("SystemRegionBrush"))
            .Setter(HostWindow.OpacityProperty, 1d)
            .Setter(HostWindow.RequestedThemeVariantProperty, new Binding(nameof(Application.RequestedThemeVariant)) { Source = Application.Current })
            .Setter(HostWindow.TransparencyLevelHintProperty, WindowTransparencyLevel.None));

        Add(new Style<DataGrid>()
            .Setter(DataGrid.RowHeightProperty, new DynamicResourceExtension("DataGridRowHeight"))
            .Setter(DataGrid.FontSizeProperty, new DynamicResourceExtension("DataGridFontSize")));

        Add(new Style<DataGridColumnHeader>()
            .Setter(DataGridColumnHeader.FontSizeProperty, new DynamicResourceExtension("DataGridFontSize"))
            .Setter(DataGridColumnHeader.MinHeightProperty, new DynamicResourceExtension("DataGridColumnHeaderMinHeight")));

        Add(new Style(x => x.OfType<DataGrid>().Descendant().OfType<TextBlock>())
            .Setter(TextBlock.MaxLinesProperty, 1)
            .Setter(TextBlock.FontSizeProperty, new DynamicResourceExtension("DataGridFontSize")));

        Add(new Style(x => x.OfType<TextBlock>().Name("CellTextBlock"))
            .Setter(TextBlock.MaxLinesProperty, 1)
            .Setter(TextBlock.FontSizeProperty, new DynamicResourceExtension("DataGridFontSize")));

        Add(new Style(x => x.OfType<StackPanel>().Class("filter-flyout-root"))
            .Setter(StackPanel.MinWidthProperty, 296d)
            .Setter(StackPanel.MaxWidthProperty, 296d)
            .Setter(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Left)
            .Setter(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Top)
            .Setter(StackPanel.SpacingProperty, 8d)
            .Setter(StackPanel.MarginProperty, new Thickness(4)));

        Add(new Style(x => x.OfType<TextBlock>().Class("filter-flyout-title"))
            .Setter(TextBlock.FontWeightProperty, FontWeight.SemiBold));

        Add(new Style(x => x.OfType<Grid>().Class("filter-flyout-row"))
            .Setter(Grid.ColumnSpacingProperty, 8d)
            .Setter(Grid.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));

        Add(new Style(x => x.OfType<TextBlock>().Class("filter-flyout-label"))
            .Setter(TextBlock.WidthProperty, 72d)
            .Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));

        Add(new Style(x => x.OfType<ComboBox>().Class("filter-flyout-editor"))
            .Setter(ComboBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));

        Add(new Style(x => x.OfType<TextBox>().Class("filter-flyout-editor"))
            .Setter(TextBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));

        Add(new Style(x => x.OfType<NumericUpDown>().Class("filter-flyout-editor"))
            .Setter(NumericUpDown.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));

        Add(new Style(x => x.OfType<Grid>().Class("filter-flyout-composite-editor"))
            .Setter(Grid.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));

        Add(new Style(x => x.OfType<StackPanel>().Class("filter-flyout-actions"))
            .Setter(StackPanel.OrientationProperty, LayoutOrientation.Horizontal)
            .Setter(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Right)
            .Setter(StackPanel.SpacingProperty, 6d));

        Add(new Style(x => x.OfType<Button>().Class("filter-flyout-action"))
            .Setter(Layoutable.MinWidthProperty, 76d));

        Add(new Style<MultiComboBoxItem>()
            .Setter(MultiComboBoxItem.PaddingProperty, new Thickness(4, 2, 4, 2))
            .Setter(MultiComboBoxItem.WidthProperty, 200d)
            .Setter(MultiComboBoxItem.MinHeightProperty, 22d));

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

        theme.Setters.Add(new Setter(ContentControl.PaddingProperty, new Thickness(0)));
        theme.Setters.Add(new Setter(ContentControl.MinWidthProperty, 0d));
        theme.Setters.Add(new Setter(ContentControl.MinHeightProperty, 0d));
        theme.Setters.Add(new Setter(ContentControl.HorizontalAlignmentProperty, HorizontalAlignment.Left));
        theme.Setters.Add(new Setter(ContentControl.VerticalAlignmentProperty, VerticalAlignment.Top));

        return theme;
    }
}
