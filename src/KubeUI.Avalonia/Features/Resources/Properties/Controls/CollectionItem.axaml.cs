using Avalonia.Controls.Templates;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public partial class CollectionItem : UserControl
{
    [GeneratedDirectProperty]
    public partial string Key { get; set; }

    [GeneratedDirectProperty]
    public partial IEnumerable Value { get; set; }

    [GeneratedDirectProperty]
    public partial IDataTemplate ItemTemplate { get; set; }

    public CollectionItem()
    {
        InitializeComponent();

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
}
