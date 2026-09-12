using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.MarkupExtensions;
using KubeUI.Avalonia.Converters;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public partial class PropertyItem : UserControl, IDeclarativeViewBase
{
    private static readonly PropertyItemValueConverter ValueConverter = new();
    private static readonly StringNotNullOrEmptyConverter StringNotNullOrEmptyConverter = new();
    private static readonly StringNullOrWhiteSpaceConverter StringNullOrWhiteSpaceConverter = new();

    [GeneratedDirectProperty]
    public partial string Key { get; set; }

    [GeneratedDirectProperty]
    public partial object? Value { get; set; }

    public PropertyItem()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
        Content = CreateContent();

#if DEBUG
        if (Design.IsDesignMode)
        {
            Key = "testKey";

            Value = "testValue";
        }
#endif
    }

    private Border CreateContent()
    {
        return new Border()
            .BorderBrush(new DynamicResourceExtension("SystemAltHighColor"))
            .BorderThickness(0, 0, 0, 1)
            .Child(
                new Grid()
                    .VerticalAlignment(VerticalAlignment.Top)
                    .Cols("*,2*")
                    .Children(
                        CreateKeyTextBlock(),
                        CreateKeyedValueViewer(),
                        CreateSpanningValueViewer()));
    }

    private SelectableTextBlock CreateKeyTextBlock()
    {
        return new SelectableTextBlock()
            .Col(0)
            .Padding(5)
            .IsVisible(this, x => x.Key, BindingMode.OneWay, StringNotNullOrEmptyConverter)
            .Text(this, x => x.Key)
            .TextWrapping(TextWrapping.NoWrap)
            .ToolTip_Tip(this, x => x.Key);
    }

    private ScrollViewer CreateKeyedValueViewer()
    {
        return new ScrollViewer()
            .Col(1)
            .MaxHeight(200)
            .IsVisible(this, x => x.Key, BindingMode.OneWay, StringNotNullOrEmptyConverter)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
            .Content(
                new SelectableTextBlock()
                    .Padding(5)
                    .Text(this, x => x.Value, BindingMode.OneWay, ValueConverter)
                    .TextWrapping(TextWrapping.Wrap));
    }

    private ScrollViewer CreateSpanningValueViewer()
    {
        return new ScrollViewer()
            .ColSpan(2)
            .MaxHeight(200)
            .IsVisible(this, x => x.Key, BindingMode.OneWay, StringNullOrWhiteSpaceConverter)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
            .Content(
                new SelectableTextBlock()
                    .Padding(5)
                    .Text(this, x => x.Value, BindingMode.OneWay, ValueConverter)
                    .TextWrapping(TextWrapping.Wrap));
    }
}

