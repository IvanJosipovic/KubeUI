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
