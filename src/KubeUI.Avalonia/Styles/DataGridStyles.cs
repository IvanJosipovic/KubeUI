using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Data;
using LiveChartsCore.Measure;
using AvaloniaStyles = Avalonia.Styling.Styles;
using NumericUpDown = Ursa.Controls.NumericUpDown;

namespace KubeUI.Avalonia.Styles;

internal static class DataGridStyles
{
    public static void AddTo(AvaloniaStyles styles)
    {
        ArgumentNullException.ThrowIfNull(styles);

        styles.Resources["DataGridFilterFlyoutPresenterTheme"] = CreateFilterFlyoutPresenterTheme();
        styles.Resources["DataGridCellTextBlockTheme"] = CreateCellTextBlockTheme();

        styles.Add(new Style<DataGrid>()
            .RowHeight(new DynamicResourceExtension("DataGridRowHeight"))
            .FontSize(new DynamicResourceExtension(Typography.AppFontSizeResourceKey)));

        styles.Add(new Style<DataGridColumnHeader>()
            .FontSize(new DynamicResourceExtension(Typography.AppFontSizeResourceKey))
            .MinHeight(new DynamicResourceExtension("DataGridColumnHeaderMinHeight")));

        styles.Add(new Style<TextBlock>(x => x.OfType<DataGrid>().Descendant().Name("CellTextBlock"))
            .MaxLines(1)
            .FontSize(new DynamicResourceExtension(Typography.AppFontSizeResourceKey)));

        styles.Add(new Style<StackPanel>(x => x.OfType<StackPanel>().Class("filter-flyout-root"))
            .MinWidth(296d)
            .MaxWidth(296d)
            .HorizontalAlignment(HorizontalAlignment.Left)
            .VerticalAlignment(VerticalAlignment.Top)
            .Spacing(8d)
            .Margin(new Thickness(4)));

        styles.Add(new Style<TextBlock>(x => x.OfType<TextBlock>().Class("filter-flyout-title"))
            .FontWeight(FontWeight.SemiBold));

        styles.Add(new Style<Grid>(x => x.OfType<Grid>().Class("filter-flyout-row"))
            .ColumnSpacing(8d)
            .HorizontalAlignment(HorizontalAlignment.Stretch));

        styles.Add(new Style<TextBlock>(x => x.OfType<TextBlock>().Class("filter-flyout-label"))
            .Width(72d)
            .VerticalAlignment(VerticalAlignment.Center));

        styles.Add(new Style<ComboBox>(x => x.OfType<ComboBox>().Class("filter-flyout-editor"))
            .HorizontalAlignment(HorizontalAlignment.Stretch));

        styles.Add(new Style<TextBox>(x => x.OfType<TextBox>().Class("filter-flyout-editor"))
            .HorizontalAlignment(HorizontalAlignment.Stretch));

        styles.Add(new Style<NumericUpDown>(x => x.OfType<NumericUpDown>().Class("filter-flyout-editor"))
            .HorizontalAlignment(HorizontalAlignment.Stretch));

        styles.Add(new Style<Grid>(x => x.OfType<Grid>().Class("filter-flyout-composite-editor"))
            .HorizontalAlignment(HorizontalAlignment.Stretch));

        styles.Add(new Style<StackPanel>(x => x.OfType<StackPanel>().Class("filter-flyout-actions"))
            .Orientation(Orientation.Horizontal)
            .HorizontalAlignment(HorizontalAlignment.Right)
            .Spacing(6d));

        styles.Add(new Style<Button>(x => x.OfType<Button>().Class("filter-flyout-action"))
            .MinWidth(76d));

    }

    private static ControlTheme CreateFilterFlyoutPresenterTheme()
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

    private static ControlTheme CreateCellTextBlockTheme()
    {
        var theme = new ControlTheme(typeof(TextBlock));
        theme.Setters.Add(new Setter(Layoutable.MarginProperty, new Thickness(12, 0, 12, 0)));
        theme.Setters.Add(new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Center));
        theme.Setters.Add(new Setter(TextBlock.MaxLinesProperty, 1));
        theme.Setters.Add(new Setter(
            ToolTip.TipProperty,
            // DataGrid applies this through ControlTheme; compiled self bindings do not resolve there.
            new Binding
            {
                Path = nameof(TextBlock.Text),
                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            }));
        return theme;
    }
}
