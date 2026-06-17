using Avalonia.Markup.Declarative;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public partial class PropertyItem : UserControl, IDeclarativeViewBase
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

