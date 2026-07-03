using Avalonia.Markup.Declarative;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public partial class HeaderItem : UserControl, IDeclarativeViewBase
{
    [GeneratedDirectProperty]
    public partial string Text { get; set; }

    public HeaderItem()
    {
        Content = new Border()
            .MinHeight(28)
            .Padding(10, 0, 10, 0)
            .BindValue(Border.BackgroundProperty, new DynamicResourceExtension("SystemAltHighColor"))
            .BindValue(Border.BorderBrushProperty, new DynamicResourceExtension("SystemAltHighColor"))
            .BorderThickness(0, 0, 0, 1)
            .Child(
                new SelectableTextBlock()
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Text(this, x => x.Text));

#if DEBUG
        if (Design.IsDesignMode)
        {
            Text = "Test123";
        }
#endif
    }
}

