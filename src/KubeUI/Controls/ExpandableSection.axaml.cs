using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit.Utils;

namespace KubeUI.Controls;

public partial class ExpandableSection : Expander
{
    public ExpandableSection()
    {
        InitializeComponent();
    }

    protected override Type StyleKeyOverride => typeof(Expander);
}
