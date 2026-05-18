namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public partial class HeaderItem : UserControl
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

