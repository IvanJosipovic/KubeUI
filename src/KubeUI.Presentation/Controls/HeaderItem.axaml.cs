namespace KubeUI.Controls;

public partial class HeaderItem : UserControl
{
    [GeneratedDirectProperty]
    public partial string Text { get; set; }

    [GeneratedDirectProperty]
    public partial ObservableCollection<Control> Controls { get; set; }

    public HeaderItem()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
        {
            Text = "Test123";

            Controls = [
                            new TextBox() {Text = "Text"},
                        ];
        }
#endif
    }
}
