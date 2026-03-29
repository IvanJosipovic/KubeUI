namespace KubeUI.Avalonia.Controls;

public partial class PropertyItem : UserControl
{
    [GeneratedDirectProperty]
    public partial string Key { get; set; }

    [GeneratedDirectProperty]
    public partial object? Value { get; set; }

    public PropertyItem()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
        {
            Key = "testKey";

            Value = "testValue";
        }
#endif
    }
}

