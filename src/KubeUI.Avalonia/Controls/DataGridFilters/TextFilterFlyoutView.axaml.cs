namespace KubeUI.Avalonia.Controls.DataGridFilters;

public partial class TextFilterFlyoutView : UserControl
{
    public TextFilterFlyoutView()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode && DataContext == null)
        {
            DataContext = new TextFilterFlyoutContext(
                title: "Name",
                apply: static () => { },
                clear: static () => { })
            {
                Query = "nginx"
            };
        }
#endif
    }
}
