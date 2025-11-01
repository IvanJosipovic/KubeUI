namespace KubeUI.Controls;

public partial class PropertyItem : UserControl
{
    [GeneratedDirectProperty]
    public partial string Key { get; set; }

    [GeneratedDirectProperty]
    public partial string Value { get; set; }

    public PropertyItem()
    {
        InitializeComponent();
    }
}
