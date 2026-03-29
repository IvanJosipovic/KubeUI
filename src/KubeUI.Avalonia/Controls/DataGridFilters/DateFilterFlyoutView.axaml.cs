namespace KubeUI.Avalonia.Controls.DataGridFilters;

public partial class DateFilterFlyoutView : UserControl
{
    public DateFilterFlyoutView()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode && DataContext == null)
        {
            var context = new DateFilterFlyoutContext(
                title: "Last Seen",
                apply: static () => { },
                clear: static () => { })
            {
                Amount = 6
            };

            context.SelectedUnit = context.Units.First(x => x.Unit == DateRelativeUnit.Hours);
            DataContext = context;
        }
#endif
    }
}
