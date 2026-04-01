namespace KubeUI.Avalonia.Controls.DataGridFilters;

public partial class EnumFilterFlyoutView : UserControl
{
    public EnumFilterFlyoutView()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode && DataContext == null)
        {
            var context = new EnumFilterFlyoutContext(
                title: "Status",
                enumType: typeof(DesignerFilterStatus),
                apply: static () => { },
                clear: static () => { });

            context.SelectedValue = context.Options.ElementAtOrDefault(1) ?? context.Options.FirstOrDefault();
            DataContext = context;
        }
#endif
    }

#if DEBUG
    private enum DesignerFilterStatus
    {
        Pending,
        Running,
        Failed
    }
#endif
}
