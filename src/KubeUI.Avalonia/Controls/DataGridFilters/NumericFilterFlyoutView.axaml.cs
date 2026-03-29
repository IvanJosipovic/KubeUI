namespace KubeUI.Avalonia.Controls.DataGridFilters;

public partial class NumericFilterFlyoutView : UserControl
{
    public NumericFilterFlyoutView()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode && DataContext == null)
        {
            var context = new NumericFilterFlyoutContext(
                title: "Count",
                apply: static () => { },
                clear: static () => { })
            {
                Value = 2,
                SecondValue = 8
            };

            context.SelectedOperator = context.Operators.First(x => x.Operator == global::Avalonia.Controls.DataGridFiltering.FilteringOperator.Between);
            DataContext = context;
        }
#endif
    }
}
