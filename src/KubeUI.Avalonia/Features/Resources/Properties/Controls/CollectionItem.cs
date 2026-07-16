using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Declarative;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public partial class CollectionItem : UserControl, IDeclarativeViewBase
{
    [GeneratedDirectProperty]
    public partial string Key { get; set; }

    [GeneratedDirectProperty]
    public partial IEnumerable Value { get; set; }

    [GeneratedDirectProperty]
    public partial IDataTemplate ItemTemplate { get; set; } = CreateDefaultItemTemplate();

    public CollectionItem()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
        Content = CreateContent();

#if DEBUG
        if (Design.IsDesignMode)
        {
            Key = "testKey";

            Value = new List<string>()
            {
                "testValue", "testValue2", "testValue3", "testValue4"
            };
        }
#endif
    }

    private static IDataTemplate CreateDefaultItemTemplate()
    {
        return new FuncDataTemplate<object?>((item, _) =>
            new SelectableTextBlock()
                .Padding(0)
                .Text(item?.ToString() ?? string.Empty));
    }

    private Border CreateContent()
    {
        return new Border()
            .BindValue(Border.BorderBrushProperty, new DynamicResourceExtension("SystemAltHighColor"))
            .BorderThickness(0, 0, 0, 1)
            .Child(
                new Grid()
                    .VerticalAlignment(VerticalAlignment.Top)
                    .Cols("*,2*")
                    .Children(
                        new SelectableTextBlock()
                            .Col(0)
                            .Padding(5)
                            .Text(this, x => x.Key)
                            .TextWrapping(TextWrapping.NoWrap)
                            .ToolTip_Tip(this, x => x.Key),
                        new ScrollViewer()
                            .Col(1)
                            .MaxHeight(200)
                            .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
                            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
                            .Content(
                                new ItemsControl()
                                    .Padding(5)
                                    .ItemTemplate(this, x => x.ItemTemplate)
                                    .ItemsSource(this, x => x.Value))));
    }
}
