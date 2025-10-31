using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace KubeUI.Controls;

public partial class ExpandableSection : Expander
{
    public ExpandableSection()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
        {
            Content = new TextBox() { Text = "Test123"};
        }
#endif
    }

    protected override Type StyleKeyOverride => typeof(Expander);
}
