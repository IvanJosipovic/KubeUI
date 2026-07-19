namespace KubeUI.Avalonia.Controls.DataGridFilters;

internal sealed partial class NumericFilterFlyoutView : ViewBase<NumericFilterFlyoutContext>
{
    public Control? Content => Child;

    public NumericFilterFlyoutView()
    {
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

    protected override object Build(NumericFilterFlyoutContext vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return DataGridFilterFlyoutViewBuilder.CreateRoot(
            DataGridFilterFlyoutViewBuilder.CreateTitle(vm),
            DataGridFilterFlyoutViewBuilder.CreateOperatorRow(vm),
            DataGridFilterFlyoutViewBuilder.CreateRow(
                Assets.Resources.DataGridFilterFlyout_Value,
                CreateNumericEditor()
                    .Value(vm, x => x.Value, BindingMode.TwoWay)),
            DataGridFilterFlyoutViewBuilder.CreateRow(
                Assets.Resources.DataGridFilterFlyout_And,
                CreateNumericEditor()
                    .Value(vm, x => x.SecondValue, BindingMode.TwoWay))
                .IsVisible(vm, x => x.IsRangeVisible),
            DataGridFilterFlyoutViewBuilder.CreateActions(vm));
    }

    private static NumericUpDown CreateNumericEditor()
    {
        return new NumericUpDown()
            .Col(1)
            .Classes("filter-flyout-editor")
            .Increment(1);
    }
}
