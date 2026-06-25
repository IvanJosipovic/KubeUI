using Avalonia.Markup.Declarative;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public partial class HeaderItem : UserControl, IDeclarativeViewBase
{
    [GeneratedDirectProperty]
    public partial string Text { get; set; }

    public HeaderItem()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
        {
            Text = "Test123";
        }
#endif
    }
}

